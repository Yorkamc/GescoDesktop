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
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(options =>
{
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
        Description = "API REST para GESCO Desktop - Sistema Offline-First con Sync Híbrido",
        Contact = new OpenApiContact
        {
            Name = "GESCO Support",
            Email = "support@gesco.com"
        }
    });
    
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

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// =====================================================
// BASES DE DATOS - CONFIGURACIÓN HÍBRIDA
// =====================================================

// SQLITE LOCAL - SIEMPRE DISPONIBLE (Base principal)
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

// POSTGRESQL - CONDICIONAL (Solo para sync)
var postgresConnectionString = GetPostgreSQLConnectionString(builder.Configuration);
if (!string.IsNullOrEmpty(postgresConnectionString))
{
    builder.Services.AddDbContext<SyncDbContext>(options =>
    {
        options.UseNpgsql(postgresConnectionString);
        
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        }
    });
    
    Console.WriteLine("✅ PostgreSQL configurado para sincronización");
    Console.WriteLine($"   Connection: {MaskConnectionString(postgresConnectionString)}");
}
else
{
    // Registrar SyncDbContext como null service para evitar errores de DI
    builder.Services.AddScoped<SyncDbContext>(_ => null!);
    Console.WriteLine("⚠️ PostgreSQL no configurado - funcionando solo con SQLite local");
}

// =====================================================
// SERVICIOS DE NEGOCIO HÍBRIDOS
// =====================================================

// Servicios básicos
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();

// Servicios híbridos (pueden funcionar con o sin PostgreSQL)
builder.Services.AddScoped<IAuthService, AuthService>(); // Mantener el original por ahora
builder.Services.AddScoped<IActivationService, ActivationService>(); // Mantener el original por ahora
builder.Services.AddScoped<IActivityService, ActivityService>();

// Migration service como Singleton
builder.Services.AddSingleton<IMigrationService>(provider =>
{
    return new MigrationService(provider, provider.GetRequiredService<ILogger<MigrationService>>());
});

// Database initialization
builder.Services.AddHostedService<DatabaseInitializationService>();

// =====================================================
// HTTP CLIENTS PARA APIS EXTERNAS
// =====================================================

// Laravel API Client para activación y auth central
var laravelApiUrl = GetLaravelApiUrl(builder.Configuration);
if (!string.IsNullOrEmpty(laravelApiUrl))
{
    builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10); // Timeout corto para no bloquear offline
        client.BaseAddress = new Uri(laravelApiUrl);
        client.DefaultRequestHeaders.Add("User-Agent", "GESCO-Desktop/1.0.0");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
    
    Console.WriteLine($"✅ Laravel API configurado: {laravelApiUrl}");
}
else
{
    // Null object pattern para evitar errores cuando no hay API
    builder.Services.AddScoped<ILaravelApiClient, NullLaravelApiClient>();
    Console.WriteLine("⚠️ Laravel API no configurado - activación offline limitada");
}

// =====================================================
// SERVICIOS DE SINCRONIZACIÓN
// =====================================================

// Servicio de sincronización que maneja ambos contextos
builder.Services.AddScoped<ISyncService>(provider =>
{
    var localContext = provider.GetRequiredService<LocalDbContext>();
    var syncContext = provider.GetService<SyncDbContext>(); // Puede ser null
    var logger = provider.GetRequiredService<ILogger<DualDatabaseSyncService>>();
    
    return new DualDatabaseSyncService(localContext, syncContext, logger);
});

// Sync en background (solo si PostgreSQL está disponible)
if (!string.IsNullOrEmpty(postgresConnectionString))
{
    builder.Services.AddHostedService<BackgroundSyncService>();
}

// =====================================================
// AUTENTICACIÓN JWT
// =====================================================

var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
             builder.Configuration["Jwt:SecretKey"] ??
             "TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
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

// =====================================================
// CORS PARA FRONTEND
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:3000",
                "file://",
                "null"
              )
              .SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// =====================================================
// LOGGING
// =====================================================

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
}

var app = builder.Build();

// =====================================================
// PIPELINE DE MIDDLEWARE
// =====================================================

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO Desktop API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
    c.DocumentTitle = "GESCO Desktop API - Híbrido";
});

app.UseCors("ReactApp");
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<RequestLoggingMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// =====================================================
// ENDPOINTS BÁSICOS
// =====================================================

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapGet("/ping", () => Results.Ok(new { 
    message = "pong", 
    timestamp = DateTime.UtcNow,
    mode = "hybrid",
    sqlite = "available",
    postgresql = !string.IsNullOrEmpty(postgresConnectionString) ? "available" : "not configured",
    laravel_api = !string.IsNullOrEmpty(laravelApiUrl) ? "available" : "not configured"
}))
.WithTags("System")
.WithName("Ping")
.ExcludeFromDescription();

// =====================================================
// ENDPOINTS PARA SYNC Y DIAGNOSTICO
// =====================================================

// Sync manual
app.MapPost("/api/system/sync", async (ISyncService syncService) =>
{
    try
    {
        var canSync = await syncService.CanSyncAsync();
        if (!canSync)
        {
            return Results.BadRequest(new { message = "Sync not available - PostgreSQL not configured or not reachable" });
        }

        await syncService.SyncToServerAsync();
        return Results.Ok(new { message = "Sync completed successfully", timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error during sync: {ex.Message}");
    }
})
.WithTags("Sync")
.WithName("ManualSync")
.RequireAuthorization();

// Status de sync
app.MapGet("/api/system/sync/status", async (ISyncService syncService) =>
{
    try
    {
        var status = await syncService.GetSyncStatusAsync();
        return Results.Ok(status);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error getting sync status: {ex.Message}");
    }
})
.WithTags("Sync")
.WithName("SyncStatus");

// Optimización de base de datos
app.MapPost("/api/system/optimize", async (IMigrationService migrationService) =>
{
    try
    {
        await migrationService.RunOptimizationScriptAsync();
        return Results.Ok(new { message = "Optimization script executed successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error executing optimization: {ex.Message}");
    }
})
.WithTags("System")
.WithName("RunOptimization")
.RequireAuthorization();

// =====================================================
// MANEJO GLOBAL DE ERRORES
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
logger.LogInformation("Starting GESCO Desktop API in Hybrid Mode...");

Console.WriteLine("=========================================");
Console.WriteLine("GESCO DESKTOP API - MODO HÍBRIDO");
Console.WriteLine("=========================================");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"API Base URL: http://localhost:5100");
Console.WriteLine($"Swagger UI: http://localhost:5100/swagger");
Console.WriteLine("=========================================");
Console.WriteLine("CONFIGURACIÓN:");
Console.WriteLine($"✅ SQLite Local: ACTIVO (Principal)");
Console.WriteLine($"{(!string.IsNullOrEmpty(postgresConnectionString) ? "✅" : "⚠️")} PostgreSQL Sync: {(!string.IsNullOrEmpty(postgresConnectionString) ? "ACTIVO" : "NO CONFIGURADO")}");
Console.WriteLine($"{(!string.IsNullOrEmpty(laravelApiUrl) ? "✅" : "⚠️")} Laravel API: {(!string.IsNullOrEmpty(laravelApiUrl) ? "ACTIVO" : "NO CONFIGURADO")}");
Console.WriteLine("=========================================");
Console.WriteLine("ENDPOINTS HÍBRIDOS:");
Console.WriteLine($"Health: GET /ping");
Console.WriteLine($"Auth: POST /api/auth/login (híbrido)");
Console.WriteLine($"License: POST /api/license/activate (híbrido)");
Console.WriteLine($"Sync Status: GET /api/system/sync/status");
Console.WriteLine($"Manual Sync: POST /api/system/sync");
Console.WriteLine($"Activities: /api/activities (local)");
Console.WriteLine($"Stats: /api/stats (local)");
Console.WriteLine("=========================================");
Console.WriteLine("CREDENCIALES DEFAULT:");
Console.WriteLine("Username: admin");
Console.WriteLine("Password: admin123");
Console.WriteLine("=========================================");

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("MODO DESARROLLO:");
    Console.WriteLine("- Logging detallado activado");
    Console.WriteLine("- Request logging activado");
    Console.WriteLine("- CORS permisivo para React");
}

if (string.IsNullOrEmpty(postgresConnectionString))
{
    Console.WriteLine("NOTA: PostgreSQL no configurado.");
    Console.WriteLine("Para habilitar sync, configure:");
    Console.WriteLine("- Variable: POSTGRESQL_CONNECTION_STRING");
    Console.WriteLine("- O appsettings: ConnectionStrings:PostgreSQL");
}

if (string.IsNullOrEmpty(laravelApiUrl))
{
    Console.WriteLine("NOTA: Laravel API no configurado.");
    Console.WriteLine("Para habilitar activación central, configure:");
    Console.WriteLine("- Variable: LARAVEL_API_URL");
    Console.WriteLine("- O appsettings: Laravel:ApiUrl");
}

Console.WriteLine("=========================================");

logger.LogInformation("GESCO Desktop API started successfully in hybrid mode");

app.Run();

// =====================================================
// FUNCIONES HELPER
// =====================================================

static string GetPostgreSQLConnectionString(IConfiguration configuration)
{
    // Orden de precedencia:
    // 1. Variable de entorno específica
    // 2. Variable de entorno con prefijo
    // 3. Configuration (appsettings)
    
    return Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING") ??
           Environment.GetEnvironmentVariable("GESCO_ConnectionStrings__PostgreSQL") ??
           configuration.GetConnectionString("PostgreSQL") ??
           string.Empty;
}

static string GetLaravelApiUrl(IConfiguration configuration)
{
    return Environment.GetEnvironmentVariable("LARAVEL_API_URL") ??
           Environment.GetEnvironmentVariable("GESCO_Laravel__ApiUrl") ??
           configuration["Laravel:ApiUrl"] ??
           string.Empty;
}

static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return "";
    
    // Mask password in connection string for logging
    var masked = connectionString;
    var passwordIndex = masked.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
    if (passwordIndex >= 0)
    {
        var start = passwordIndex + "Password=".Length;
        var end = masked.IndexOf(';', start);
        if (end < 0) end = masked.Length;
        
        masked = masked.Substring(0, start) + "***" + (end < masked.Length ? masked.Substring(end) : "");
    }
    
    return masked;
}

// =====================================================
// SERVICIOS DUMMY PARA CUANDO NO HAY CONFIGURACIÓN
// =====================================================

public class NullLaravelApiClient : ILaravelApiClient
{
    public Task<bool> IsConnectedAsync() => Task.FromResult(false);
    
    public Task<Gesco.Desktop.Shared.DTOs.LoginResultDto> LoginAsync(string usuario, string password)
        => Task.FromResult(new Gesco.Desktop.Shared.DTOs.LoginResultDto 
        { 
            Success = false, 
            Message = "Laravel API not configured" 
        });
    
    public Task<Gesco.Desktop.Shared.DTOs.ActivationResultDto> ActivateAsync(string codigo, int organizacionId)
        => Task.FromResult(new Gesco.Desktop.Shared.DTOs.ActivationResultDto 
        { 
            Success = false, 
            Message = "Laravel API not configured" 
        });
    
    public Task<bool> ValidateLicenseAsync(string codigo) => Task.FromResult(false);
}

// =====================================================
// SYNC SERVICE BÁSICO (implementación completa en siguiente paso)
// =====================================================

public interface ISyncService
{
    Task<bool> CanSyncAsync();
    Task SyncToServerAsync();
    Task SyncFromServerAsync();
    Task<SyncStatus> GetSyncStatusAsync();
}

public class DualDatabaseSyncService : ISyncService
{
    private readonly LocalDbContext _localContext;
    private readonly SyncDbContext? _syncContext;
    private readonly ILogger<DualDatabaseSyncService> _logger;

    public DualDatabaseSyncService(
        LocalDbContext localContext,
        SyncDbContext? syncContext,
        ILogger<DualDatabaseSyncService> logger)
    {
        _localContext = localContext;
        _syncContext = syncContext;
        _logger = logger;
    }

    public async Task<bool> CanSyncAsync()
    {
        if (_syncContext == null) return false;
        
        try
        {
            return await _syncContext.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task SyncToServerAsync()
    {
        if (!await CanSyncAsync())
        {
            throw new InvalidOperationException("Sync not available");
        }

        _logger.LogInformation("Starting sync to PostgreSQL...");
        
        // TODO: Implementar lógica de sync
        // Por ahora, solo log
        _logger.LogInformation("Sync to PostgreSQL completed (placeholder)");
    }

    public async Task SyncFromServerAsync()
    {
        if (!await CanSyncAsync()) return;
        
        _logger.LogInformation("Starting sync from PostgreSQL...");
        
        // TODO: Implementar lógica de sync
        _logger.LogInformation("Sync from PostgreSQL completed (placeholder)");
    }

    public async Task<SyncStatus> GetSyncStatusAsync()
    {
        var canSync = await CanSyncAsync();
        
        return new SyncStatus
        {
            CanSync = canSync,
            LastSync = null, // TODO: Implementar
            PendingChanges = 0, // TODO: Implementar
            Message = canSync ? "Ready to sync" : "PostgreSQL not available"
        };
    }
}

public class SyncStatus
{
    public bool CanSync { get; set; }
    public DateTime? LastSync { get; set; }
    public int PendingChanges { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Background service para sync automático
public class BackgroundSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(30); // Sync cada 30 minutos

    public BackgroundSyncService(IServiceProvider serviceProvider, ILogger<BackgroundSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background sync service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                
                if (await syncService.CanSyncAsync())
                {
                    await syncService.SyncToServerAsync();
                    _logger.LogInformation("Automatic sync completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automatic sync");
            }

            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Background sync service stopped");
    }
}