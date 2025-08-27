using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.Core.Middleware;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Sync.LaravelApi;
using System.Text;
using DotNetEnv;

// Cargar variables de entorno
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configurar puerto
builder.WebHost.UseUrls($"http://localhost:{Environment.GetEnvironmentVariable("LOCAL_API_PORT") ?? "5100"}");

// =====================================================
// SERVICIOS
// =====================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GESCO Desktop API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Por favor ingrese el token JWT",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
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
    var dbPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "data",
        Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? "gesco_local.db"
    );
    
    var directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
    
    options.UseSqlite($"Data Source={dbPath}");
    options.EnableSensitiveDataLogging();
});

// Servicios de negocio
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<ILaravelApiClient, LaravelApiClient>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddHostedService<BackupService>();
builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>();

// Configuración de DataProtection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")))
    .SetApplicationName("GescoDesktop")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "TuClaveSecretaMuyLargaYSegura2024GescoDesktop!@#$%";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// CORS - IMPORTANTE para permitir conexiones desde React
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // React dev servers
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// =====================================================
// INICIALIZACIÓN DE BASE DE DATOS
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
    context.Database.EnsureCreated();
    Console.WriteLine("✅ Base de datos local verificada/creada");
}

// =====================================================
// MIDDLEWARE
// =====================================================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO Desktop API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

app.UseCors("ReactApp");
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// API ENDPOINTS
// =====================================================

// Health check
app.MapGet("/api/health", () => Results.Ok(new 
{ 
    status = "healthy",
    timestamp = DateTime.Now,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}))
.WithName("HealthCheck")
.WithOpenApi();

// Auth endpoints
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    if (string.IsNullOrEmpty(request.Usuario) || string.IsNullOrEmpty(request.Password))
    {
        return Results.BadRequest(new { message = "Usuario y contraseña son requeridos" });
    }
    
    var result = await authService.LoginAsync(request.Usuario, request.Password);
    return result.Success ? Results.Ok(result) : Results.Unauthorized();
})
.WithName("Login")
.WithOpenApi();

app.MapPost("/api/auth/logout", async (IAuthService authService) =>
{
    await authService.LogoutAsync();
    return Results.Ok(new { message = "Sesión cerrada exitosamente" });
})
.WithName("Logout")
.RequireAuthorization()
.WithOpenApi();

app.MapPost("/api/auth/validate", async (HttpContext context, IAuthService authService) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    if (string.IsNullOrEmpty(token))
    {
        return Results.Unauthorized();
    }
    
    var isValid = await authService.ValidateTokenAsync(token);
    return isValid ? Results.Ok(new { valid = true }) : Results.Unauthorized();
})
.WithName("ValidateToken")
.WithOpenApi();

// License endpoints
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
.WithOpenApi();

app.MapGet("/api/license/status", async (IActivationService activationService) =>
{
    var status = await activationService.GetLicenseStatusAsync();
    return Results.Ok(status);
})
.WithName("GetLicenseStatus")
.WithOpenApi();

// Activities endpoints
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
            a.PrecioEntrada
        })
        .ToListAsync();
    
    return Results.Ok(activities);
})
.WithName("GetActivities")
.RequireAuthorization()
.WithOpenApi();

// Sales endpoints
app.MapGet("/api/sales/today", async (LocalDbContext context) =>
{
    var today = DateTime.Today;
    var sales = await context.TransaccionesVenta
        .Where(t => t.FechaTransaccion.Date == today)
        .SumAsync(t => t.Total);
    
    return Results.Ok(new { date = today, total = sales });
})
.WithName("GetTodaySales")
.RequireAuthorization()
.WithOpenApi();

// Stats endpoint
app.MapGet("/api/stats", async (LocalDbContext context) =>
{
    var stats = new
    {
        actividades = await context.Actividades.CountAsync(),
        ventasHoy = await context.TransaccionesVenta
            .Where(t => t.FechaTransaccion.Date == DateTime.Today)
            .SumAsync(t => t.Total),
        transacciones = await context.TransaccionesVenta
            .Where(t => t.FechaTransaccion.Date == DateTime.Today)
            .CountAsync()
    };
    
    return Results.Ok(stats);
})
.WithName("GetStats")
.RequireAuthorization()
.WithOpenApi();

// =====================================================
// INICIO DE LA APLICACIÓN
// =====================================================
Console.WriteLine("=========================================");
Console.WriteLine("  GESCO DESKTOP API");
Console.WriteLine("=========================================");
Console.WriteLine($"  URL: http://localhost:{Environment.GetEnvironmentVariable("LOCAL_API_PORT") ?? "5100"}");
Console.WriteLine($"  Swagger: http://localhost:{Environment.GetEnvironmentVariable("LOCAL_API_PORT") ?? "5100"}/");
Console.WriteLine("=========================================");

app.Run();