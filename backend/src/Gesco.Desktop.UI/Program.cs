using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.UI.Middleware;  // ← Namespace correcto para middlewares
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Sync.LaravelApi;
using System.Text;
using DotNetEnv;

// Cargar variables de entorno
try { Env.Load(); } catch { /* Ignorar si no existe .env */ }

var builder = WebApplication.CreateBuilder(args);

// Configurar puerto
builder.WebHost.UseUrls("http://localhost:5100");

// Servicios básicos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "GESCO Desktop API", 
        Version = "v1",
        Description = "API REST para GESCO Desktop - Sistema de Gestión de Actividades"
    });
    
    // Configuración JWT para Swagger
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

// Database
builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_local.db");
    var directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
    
    options.UseSqlite($"Data Source={dbPath}");
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// Servicios de negocio
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<ILaravelApiClient, LaravelApiClient>();
builder.Services.AddScoped<IAuditService, AuditService>();

// HTTP Client
builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>();

// JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345";
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

// CORS - CRÍTICO para React
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
              .AllowCredentials();
    });
});

var app = builder.Build();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("✅ Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error inicializando base de datos: {ex.Message}");
    }
}

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO Desktop API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("ReactApp");

// Middlewares de seguridad - ACTIVAR GRADUALMENTE
app.UseMiddleware<SecurityHeadersMiddleware>();           // ✅ Headers de seguridad básicos
// app.UseMiddleware<RateLimitingMiddleware>();           // ⚠️ Rate limiting (para producción)
// app.UseMiddleware<RequestLoggingMiddleware>();         // 📝 Logging detallado (para debug)

app.UseAuthentication();
app.UseAuthorization();

// =================== ENDPOINTS ===================

// Health Check
app.MapGet("/api/health", () => Results.Ok(new 
{ 
    status = "healthy",
    timestamp = DateTime.Now,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}))
.WithTags("System")
.WithName("HealthCheck");

// Auth
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService, IAuditService auditService) =>
{
    if (string.IsNullOrEmpty(request.Usuario) || string.IsNullOrEmpty(request.Password))
        return Results.BadRequest(new { message = "Usuario y contraseña requeridos" });
    
    var result = await authService.LoginAsync(request.Usuario, request.Password);
    
    // Auditoría opcional
    try 
    {
        await auditService.LogLoginAttemptAsync(request.Usuario, result.Success, "127.0.0.1", 
            result.Success ? null : result.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error en auditoría: {ex.Message}");
    }
    
    return result.Success ? Results.Ok(result) : Results.Unauthorized();
})
.WithTags("Auth")
.WithName("Login");

app.MapPost("/api/auth/logout", async (IAuthService authService) =>
{
    await authService.LogoutAsync();
    return Results.Ok(new { message = "Sesión cerrada exitosamente" });
})
.WithTags("Auth")
.WithName("Logout")
.RequireAuthorization();

app.MapPost("/api/auth/validate", async (HttpContext context, IAuthService authService) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        return Results.Unauthorized();
    
    var token = authHeader.Replace("Bearer ", "");
    var isValid = await authService.ValidateTokenAsync(token);
    return isValid ? Results.Ok(new { valid = true }) : Results.Unauthorized();
})
.WithTags("Auth")
.WithName("ValidateToken");

// License
app.MapPost("/api/license/activate", async (ActivationRequest request, IActivationService activationService) =>
{
    if (string.IsNullOrEmpty(request.CodigoActivacion))
        return Results.BadRequest(new { message = "Código de activación requerido" });
    
    var result = await activationService.ActivateAsync(request.CodigoActivacion, request.OrganizacionId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithTags("License")
.WithName("ActivateLicense");

app.MapGet("/api/license/status", async (IActivationService activationService) =>
{
    var status = await activationService.GetLicenseStatusAsync();
    return Results.Ok(status);
})
.WithTags("License")
.WithName("GetLicenseStatus");

// Stats
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
.WithTags("Stats")
.WithName("GetStats")
.RequireAuthorization();

// Activities
app.MapGet("/api/activities", async (LocalDbContext context) =>
{
    var activities = await context.Actividades
        .Select(a => new 
        {
            a.Id,
            a.Nombre,
            a.Descripcion,
            a.FechaInicio,
            a.FechaFin
        })
        .ToListAsync();
    return Results.Ok(activities);
})
.WithTags("Activities")
.WithName("GetActivities")
.RequireAuthorization();

// Sales
app.MapGet("/api/sales/today", async (LocalDbContext context) =>
{
    var today = DateTime.Today;
    var sales = await context.TransaccionesVenta
        .Where(t => t.FechaTransaccion.Date == today)
        .SumAsync(t => (decimal?)t.Total) ?? 0m;
    return Results.Ok(new { date = today, total = sales });
})
.WithTags("Sales")
.WithName("GetTodaySales")
.RequireAuthorization();

// Redirect root
app.MapGet("/", () => Results.Redirect("/swagger"));

// Mensaje de inicio
Console.WriteLine("=========================================");
Console.WriteLine("🚀 GESCO DESKTOP API - INICIADO");
Console.WriteLine("=========================================");
Console.WriteLine($"📡 API: http://localhost:5100/api");
Console.WriteLine($"📚 Swagger: http://localhost:5100/swagger");
Console.WriteLine($"✅ Health: http://localhost:5100/api/health");
Console.WriteLine($"🛡️ Seguridad: Headers de seguridad activos");
Console.WriteLine("=========================================");

app.Run();