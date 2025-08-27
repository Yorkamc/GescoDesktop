using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.UI.Middleware;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Sync.LaravelApi;
using System.Text;
using DotNetEnv;

// Cargar variables de entorno
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONFIGURACIÓN DEL SERVIDOR
// =====================================================
builder.WebHost.UseUrls("http://localhost:5100");

// =====================================================
// SERVICIOS
// =====================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger mejorada
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "GESCO Desktop API", 
        Version = "v1",
        Description = "API REST para GESCO Desktop - Sistema de Gestión de Actividades",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "GESCO Support",
            Email = "admin@gesco.com"
        }
    });
    
    // Configuración de autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_local.db");
    var directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
    
    options.UseSqlite($"Data Source={dbPath}");
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Business Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<ILaravelApiClient, LaravelApiClient>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddHostedService<BackupService>();

// HTTP Client para Laravel API
builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>();

// JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "TuClaveSecretaMuyLargaYSegura2024GescoDesktop!@#$%";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS Configuration - CRÍTICO para React
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:3000", 
                "http://localhost:5174"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(2520)); // Cache preflight
    });
});

var app = builder.Build();

// =====================================================
// INICIALIZAR BASE DE DATOS
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
    context.Database.EnsureCreated();
    Console.WriteLine("✅ Base de datos inicializada");
}

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================
// Swagger - SIEMPRE DISPONIBLE para desarrollo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO Desktop API v1");
    c.RoutePrefix = "swagger"; // Disponible en /swagger
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
});

// CORS - DEBE IR ANTES DE AUTHENTICATION
app.UseCors("ReactApp");

// Security Headers y Rate Limiting
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// API ENDPOINTS
// =====================================================

// Health Check
app.MapGet("/api/health", () => Results.Ok(new 
{ 
    status = "healthy",
    timestamp = DateTime.Now,
    version = "1.0.0",
    database = "connected"
}))
.WithName("HealthCheck")
.WithTags("System")
.WithOpenApi();

// =====================================================
// AUTH ENDPOINTS
// =====================================================
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService, IAuditService auditService) =>
{
    if (string.IsNullOrEmpty(request.Usuario) || string.IsNullOrEmpty(request.Password))
    {
        return Results.BadRequest(new { message = "Usuario y contraseña son requeridos" });
    }
    
    var result = await authService.LoginAsync(request.Usuario, request.Password);
    
    // Log de auditoría
    await auditService.LogLoginAttemptAsync(
        request.Usuario, 
        result.Success, 
        "127.0.0.1", 
        result.Success ? null : result.Message
    );
    
    return result.Success ? Results.Ok(result) : Results.Unauthorized();
})
.WithName("Login")
.WithTags("Authentication")
.WithOpenApi();

app.MapPost("/api/auth/logout", async (IAuthService authService) =>
{
    await authService.LogoutAsync();
    return Results.Ok(new { message = "Sesión cerrada exitosamente" });
})
.WithName("Logout")
.WithTags("Authentication")
.RequireAuthorization()
.WithOpenApi();

app.MapPost("/api/auth/validate", async (HttpContext context, IAuthService authService) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        return Results.Unauthorized();
    }
    
    var token = authHeader.Replace("Bearer ", "");
    var isValid = await authService.ValidateTokenAsync(token);
    return isValid ? Results.Ok(new { valid = true }) : Results.Unauthorized();
})
.WithName("ValidateToken")
.WithTags("Authentication")
.WithOpenApi();

// =====================================================
// LICENSE ENDPOINTS
// =====================================================
app.MapPost("/api/license/activate", async (ActivationRequest request, IActivationService activationService) =>
{
    if (string.IsNullOrEmpty(request.CodigoActivacion))
    {
        return Results.BadRequest(new { message = "Código de activación requerido" });
    }
    
    var result = await activationService.ActivateAsync(request.CodigoActivacion, request.OrganizacionId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("ActivateLicense")
.WithTags("License")
.WithOpenApi();

app.MapGet("/api/license/status", async (IActivationService activationService) =>
{
    var status = await activationService.GetLicenseStatusAsync();
    return Results.Ok(status);
})
.WithName("GetLicenseStatus")
.WithTags("License")
.WithOpenApi();

// =====================================================
// ACTIVITIES ENDPOINTS
// =====================================================
app.MapGet("/api/activities", async (LocalDbContext context) =>
{
    var activities = await context.Actividades
        .Select(a => new 
        {
            a.Id,
            a.Nombre,
            a.Descripcion,
            a.FechaInicio,
            a.FechaFin,
            a.PrecioEntrada,
            a.CapacidadMaxima
        })
        .ToListAsync();
    
    return Results.Ok(activities);
})
.WithName("GetActivities")
.WithTags("Activities")
.RequireAuthorization()
.WithOpenApi();

// =====================================================
// SALES ENDPOINTS
// =====================================================
app.MapGet("/api/sales/today", async (LocalDbContext context) =>
{
    var today = DateTime.Today;
    var sales = await context.TransaccionesVenta
        .Where(t => t.FechaTransaccion.Date == today)
        .SumAsync(t => t.Total);
    
    return Results.Ok(new { date = today, total = sales });
})
.WithName("GetTodaySales")
.WithTags("Sales")
.RequireAuthorization()
.WithOpenApi();

// =====================================================
// STATS ENDPOINT
// =====================================================
app.MapGet("/api/stats", async (LocalDbContext context) =>
{
    var stats = new
    {
        actividades = await context.Actividades.CountAsync(),
        ventasHoy = await context.TransaccionesVenta
            .Where(t => t.FechaTransaccion.Date == DateTime.Today)
            .SumAsync(t => (decimal?)t.Total) ?? 0m,
        transacciones = await context.TransaccionesVenta
            .Where(t => t.FechaTransaccion.Date == DateTime.Today)
            .CountAsync()
    };
    
    return Results.Ok(stats);
})
.WithName("GetStats")
.WithTags("Dashboard")
.RequireAuthorization()
.WithOpenApi();

// =====================================================
// REDIRECT ROOT TO SWAGGER
// =====================================================
app.MapGet("/", () => Results.Redirect("/swagger"))
.ExcludeFromDescription();

// =====================================================
// STARTUP MESSAGE
// =====================================================
Console.WriteLine("=========================================");
Console.WriteLine("  🚀 GESCO DESKTOP API");
Console.WriteLine("=========================================");
Console.WriteLine($"  📡 API URL: http://localhost:5100/api");
Console.WriteLine($"  📚 Swagger: http://localhost:5100/swagger");
Console.WriteLine($"  ✅ Health: http://localhost:5100/api/health");
Console.WriteLine("=========================================");

app.Run();