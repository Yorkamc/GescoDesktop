using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.UI.Middleware;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Sync.LaravelApi;
using System.Text;
using System.Text.Json;
using System.Reflection;
using DotNetEnv;

// Cargar variables de entorno
try { Env.Load(); } catch { /* Ignorar si no existe .env */ }

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONFIGURACIÓN DE SERVICIOS
// =====================================================

// Configurar puerto
builder.WebHost.UseUrls("http://localhost:5100");

// Servicios básicos
builder.Services.AddControllers(options =>
{
    // Configuración global de controladores
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(options =>
{
    // Configuración JSON para APIs
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// API Explorer y Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GESCO Desktop API", 
        Version = "v1.0.0",
        Description = "API REST para GESCO Desktop - Sistema de Gestión de Actividades y Ventas",
        Contact = new OpenApiContact
        {
            Name = "GESCO Support",
            Email = "support@gesco.com"
        }
    });
    
    // Configuración JWT para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML si existen
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Base de datos
builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_local.db");
    var directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
    
    options.UseSqlite($"Data Source={dbPath}");
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// Servicios de negocio
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<ILaravelApiClient, LaravelApiClient>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();

// HTTP Client para servicios externos
builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "GESCO-Desktop/1.0.0");
});

// JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
             "TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Solo para desarrollo
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS - Configuración para desarrollo y producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:3000", 
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:3000"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    // Política más restrictiva para producción
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Authorization", "Content-Type")
              .AllowCredentials();
    });
});

// Logging personalizado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
    // TODO: Agregar Serilog para logging en producción
}

// Servicios de background (opcional)
// builder.Services.AddHostedService<BackupHostedService>();

var app = builder.Build();

// =====================================================
// INICIALIZACIÓN DE BASE DE DATOS
// =====================================================
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var dbLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        dbLogger.LogInformation("Initializing database...");
        await context.Database.EnsureCreatedAsync();
        
        // Verificar datos semilla
        var userCount = await context.Usuarios.CountAsync();
        dbLogger.LogInformation("Database initialized successfully. Users: {UserCount}", userCount);
        
        // Si no hay usuarios, verificar que los datos semilla se crearon
        if (userCount == 0)
        {
            dbLogger.LogWarning("No users found in database. Check seed data configuration.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error inicializando base de datos: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw; // Re-lanzar para que la aplicación no se inicie con DB corrupta
    }
}

// =====================================================
// CONFIGURACIÓN DEL PIPELINE DE MIDDLEWARE
// =====================================================

// Swagger (solo en desarrollo por defecto)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO Desktop API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

// CORS debe ir temprano en el pipeline
app.UseCors(app.Environment.IsDevelopment() ? "ReactApp" : "Production");

// Middlewares de seguridad
app.UseMiddleware<SecurityHeadersMiddleware>();

// Solo agregar rate limiting en producción
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

// Request logging solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<RequestLoggingMiddleware>();
}

// Middlewares estándar de ASP.NET Core
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// Endpoint de redirección para la raíz
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

// Endpoint adicional para verificar que la API está funcionando
app.MapGet("/ping", () => Results.Ok(new { 
    message = "pong", 
    timestamp = DateTime.UtcNow 
}))
.WithTags("System")
.WithName("Ping")
.ExcludeFromDescription();

// =====================================================
// MANEJO GLOBAL DE ERRORES (opcional)
// =====================================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception occurred");

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                message = "An internal server error occurred",
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier
            }));
        }
    });
});

// =====================================================
// MENSAJES DE INICIO
// =====================================================
var logger = app.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Starting GESCO Desktop API...");

Console.WriteLine("=========================================");
Console.WriteLine("🚀 GESCO DESKTOP API - INICIADO");
Console.WriteLine("=========================================");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"📡 API Base URL: http://localhost:5100");
Console.WriteLine($"📚 Swagger UI: http://localhost:5100/swagger");
Console.WriteLine($"✅ Health Check: http://localhost:5100/api/system/health");
Console.WriteLine($"🔐 Auth: http://localhost:5100/api/auth");
Console.WriteLine($"📄 License: http://localhost:5100/api/license");
Console.WriteLine($"📊 Stats: http://localhost:5100/api/system/stats");
Console.WriteLine($"🛡️ Security: Headers de seguridad activos");

if (app.Environment.IsDevelopment())
{
    Console.WriteLine($"🔍 Debug Mode: Logging detallado activado");
    Console.WriteLine($"📝 Request Logging: Activo");
}

Console.WriteLine("=========================================");

logger.LogInformation("GESCO Desktop API started successfully");

// =====================================================
// EJECUTAR LA APLICACIÓN
// =====================================================
app.Run();