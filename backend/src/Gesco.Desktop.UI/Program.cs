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
using System.Text.Json.Serialization;
using System.Security.Claims;
using DotNetEnv;
using Gesco.Desktop.Core.Security;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CARGAR VARIABLES DE ENTORNO
// ============================================
try 
{ 
    Env.Load(); 
    Console.WriteLine("✓ .env file loaded successfully");
} 
catch 
{
    Console.WriteLine("⚠ No .env file found - using default configuration");
}

// ============================================
// LOGGING CONFIGURATION
// ============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(
    builder.Environment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning
);

// ============================================
// WEB HOST CONFIGURATION
// ============================================
builder.WebHost.UseUrls("http://localhost:5100");

// ============================================
// CONTROLLERS AND JSON SERIALIZATION
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ============================================
// SWAGGER CONFIGURATION
// ============================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GESCO Desktop API", 
        Version = "v1.0.0",
        Description = "Sistema de Gestión Comercial - API REST",
        Contact = new OpenApiContact
        {
            Name = "GESCO Support",
            Email = "support@gesco.com"
        }
    });
    
    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
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
});

// ============================================
// DATABASE CONFIGURATION - SQLITE
// ============================================
builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var connectionString = SecureSettings.GetSecureConnectionString();
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30);
        sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
}, ServiceLifetime.Scoped);

// ============================================
// DATABASE CONFIGURATION - POSTGRESQL (OPTIONAL)
// ============================================
var postgresConnectionString = GetConnectionString("POSTGRESQL_CONNECTION_STRING", builder.Configuration);
if (!string.IsNullOrEmpty(postgresConnectionString))
{
    builder.Services.AddDbContext<SyncDbContext>(options =>
    {
        options.UseNpgsql(postgresConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.EnableRetryOnFailure(3);
        });
    }, ServiceLifetime.Scoped);
}
else
{
    builder.Services.AddScoped<SyncDbContext>(_ => null!);
}

// ============================================
// CACHING
// ============================================
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// ============================================
// CORE SERVICES
// ============================================
builder.Services.AddSingleton<DatabaseEncryption>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();


// ============================================
// BUSINESS SERVICES
// ============================================
builder.Services.AddScoped<ICachedLookupService, CachedLookupService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICashRegisterService, CashRegisterService>();
builder.Services.AddScoped<ISalesTransactionService, SalesTransactionService>();
builder.Services.AddScoped<IActivityProductService, ActivityProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISalesComboService, SalesComboService>();
// ============================================
// MIGRATION SERVICE
// ============================================
builder.Services.AddSingleton<IMigrationService>(provider =>
    new MigrationService(provider, provider.GetRequiredService<ILogger<MigrationService>>()));

builder.Services.AddHostedService<DatabaseInitializationService>();

// ============================================
// LARAVEL API CLIENT (OPTIONAL)
// ============================================
var laravelApiUrl = GetConnectionString("LARAVEL_API_URL", builder.Configuration);
if (!string.IsNullOrEmpty(laravelApiUrl))
{
    builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.BaseAddress = new Uri(laravelApiUrl);
    });
}
else
{
    builder.Services.AddScoped<ILaravelApiClient, NullLaravelApiClient>();
}

// ============================================
// JWT AUTHENTICATION CONFIGURATION
// ============================================
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
             "TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345";

// Log JWT configuration in development
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("========================================");
    Console.WriteLine("JWT CONFIGURATION");
    Console.WriteLine("========================================");
    Console.WriteLine($"Secret Key Length: {jwtKey.Length}");
    Console.WriteLine($"Secret Key Preview: {jwtKey.Substring(0, Math.Min(20, jwtKey.Length))}...");
    Console.WriteLine("========================================");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Solo para desarrollo local
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // Sin tolerancia de tiempo
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
    
    // Eventos para debugging (solo en desarrollo)
    if (builder.Environment.IsDevelopment())
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT ERROR] Authentication failed: {context.Exception.Message}");
                if (context.Exception.InnerException != null)
                {
                    Console.WriteLine($"[JWT ERROR] Inner exception: {context.Exception.InnerException.Message}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                var username = context.Principal?.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
                Console.WriteLine($"[JWT SUCCESS] Token validated for user: {username} (ID: {userId})");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"[JWT CHALLENGE] Error: {context.Error}, Description: {context.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[JWT] Token received: {token.Substring(0, Math.Min(30, token.Length))}...");
                }
                else
                {
                    Console.WriteLine($"[JWT] No token in Authorization header");
                }
                return Task.CompletedTask;
            }
        };
    }
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// ============================================
// CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://127.0.0.1:5173"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================
// BUILD APPLICATION
// ============================================
var app = builder.Build();

// ============================================
// INITIALIZE ENCRYPTION
// ============================================
await InitializeEncryption(app);

// ============================================
// MIDDLEWARE PIPELINE (ORDEN CRÍTICO)
// ============================================

// Development middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "GESCO Desktop API";
    });
}

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// CORS - debe ir antes de Authentication
app.UseCors("AllowFrontend");

// Response caching
app.UseResponseCaching();

// Security headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Rate limiting (solo en producción)
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

// CRÍTICO: Authentication ANTES de Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// ============================================
// ROOT ENDPOINTS
// ============================================

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapGet("/ping", () => Results.Ok(new { 
    status = "ok", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
}))
.WithTags("System")
.AllowAnonymous();

// Endpoint para verificar configuración JWT (solo desarrollo)
app.MapGet("/jwt-config", (IWebHostEnvironment env) =>
{
    if (!env.IsDevelopment())
    {
        return Results.NotFound();
    }
    
    return Results.Ok(new
    {
        message = "JWT Configuration",
        keyLength = jwtKey.Length,
        keyPreview = jwtKey.Substring(0, Math.Min(20, jwtKey.Length)) + "...",
        algorithm = "HS256",
        expirationHours = 24,
        clockSkew = "Zero"
    });
})
.WithTags("Development")
.ExcludeFromDescription();

// ============================================
// STARTUP INFO
// ============================================
PrintStartupInfo(app.Environment, postgresConnectionString, laravelApiUrl);

// ============================================
// RUN APPLICATION
// ============================================
app.Run();

// ============================================
// HELPER FUNCTIONS
// ============================================

static string GetConnectionString(string envKey, IConfiguration configuration)
{
    return Environment.GetEnvironmentVariable(envKey) ??
           configuration.GetConnectionString(envKey.Replace("_", "")) ??
           string.Empty;
}

static async Task InitializeEncryption(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var encryption = scope.ServiceProvider.GetRequiredService<DatabaseEncryption>();
        
        await Task.Run(() => SecureSettings.ApplySecuritySettings(context));
        
        var testText = "TEST" + DateTime.Now.Ticks;
        var encrypted = encryption.EncryptString(testText);
        var decrypted = encryption.DecryptString(encrypted);
        
        if (testText != decrypted)
        {
            throw new InvalidOperationException("Encryption test failed");
        }
        
        Console.WriteLine("✓ Database encryption initialized successfully");
        await Task.CompletedTask;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Encryption initialization failed: {ex.Message}");
    }
}

static void PrintStartupInfo(IWebHostEnvironment environment, string postgresConn, string laravelUrl)
{
    Console.WriteLine("");
    Console.WriteLine("========================================");
    Console.WriteLine("    GESCO DESKTOP API - READY");
    Console.WriteLine("========================================");
    Console.WriteLine($"Environment: {environment.EnvironmentName}");
    Console.WriteLine($"API URL: http://localhost:5100");
    Console.WriteLine($"Swagger: http://localhost:5100/swagger");
    Console.WriteLine("========================================");
    Console.WriteLine("ACTIVE SERVICES:");
    Console.WriteLine("  ✓ SQLite Database");
    Console.WriteLine($"  {(!string.IsNullOrEmpty(postgresConn) ? "✓" : "✗")} PostgreSQL Sync");
    Console.WriteLine($"  {(!string.IsNullOrEmpty(laravelUrl) ? "✓" : "✗")} Laravel API");
    Console.WriteLine("  ✓ JWT Authentication");
    Console.WriteLine("  ✓ Memory Cache");
    Console.WriteLine("  ✓ Response Cache");
    Console.WriteLine("========================================");
    Console.WriteLine("DEFAULT CREDENTIALS:");
    Console.WriteLine("  Username: admin");
    Console.WriteLine("  Password: admin123");
    Console.WriteLine("  Cedula: 118640123");
    Console.WriteLine("========================================");
    
    if (environment.IsDevelopment())
    {
        Console.WriteLine("DEV MODE FEATURES:");
        Console.WriteLine("  ✓ Detailed logging enabled");
        Console.WriteLine("  ✓ JWT debugging enabled");
        Console.WriteLine("  ✓ Swagger UI enabled");
        Console.WriteLine("  ✓ Sensitive data logging");
        Console.WriteLine("========================================");
    }
    
    Console.WriteLine("");
    Console.WriteLine("✓ Ready to accept requests!");
    Console.WriteLine("  Press Ctrl+C to shutdown");
    Console.WriteLine("");
}

// ============================================
// NULL LARAVEL CLIENT
// ============================================

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