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
using Gesco.Desktop.Core.Security;

// =====================================================
// STARTUP OPTIMIZATION
// =====================================================

var builder = WebApplication.CreateBuilder(args);

// Fast environment loading
try { Env.Load(); } catch { }

// Configure minimal logging for startup
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// =====================================================
// CORE SERVICES SETUP
// =====================================================

builder.WebHost.UseUrls("http://localhost:5100");

// Optimized JSON configuration
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Minimal API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "GESCO Desktop API", 
        Version = "v1.0.0",
        Description = "Sistema de Gestion Comercial - API REST"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
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

// =====================================================
// DATABASE CONFIGURATION
// =====================================================

// SQLite with optimized settings
builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var connectionString = SecureSettings.GetSecureConnectionString();
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
}, ServiceLifetime.Scoped);

// PostgreSQL for sync (optional)
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

// =====================================================
// BUSINESS SERVICES
// =====================================================

// Core services
builder.Services.AddSingleton<DatabaseEncryption>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Migration service
builder.Services.AddSingleton<IMigrationService>(provider =>
    new MigrationService(provider, provider.GetRequiredService<ILogger<MigrationService>>()));

// Database initialization
builder.Services.AddHostedService<DatabaseInitializationService>();

// =====================================================
// EXTERNAL APIS
// =====================================================

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

// =====================================================
// AUTHENTICATION
// =====================================================

var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
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
// CORS
// =====================================================

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

// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();

// Quick encryption test
await InitializeEncryption(app);

// =====================================================
// MIDDLEWARE PIPELINE
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// =====================================================
// API ENDPOINTS
// =====================================================

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapGet("/ping", () => Results.Ok(new { 
    status = "ok", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithTags("System");

// =====================================================
// STARTUP MESSAGE
// =====================================================

PrintStartupInfo(app.Environment, postgresConnectionString, laravelApiUrl);

app.Run();

// =====================================================
// HELPER METHODS
// =====================================================

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
        
        SecureSettings.ApplySecuritySettings(context);
        
        // Quick encryption test
        var testText = "TEST" + DateTime.Now.Ticks;
        var encrypted = encryption.EncryptString(testText);
        var decrypted = encryption.DecryptString(encrypted);
        
        if (testText != decrypted)
        {
            throw new InvalidOperationException("Encryption test failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Encryption initialization failed: {ex.Message}");
    }
}

static void PrintStartupInfo(IWebHostEnvironment environment, string postgresConn, string laravelUrl)
{
    Console.WriteLine("========================================");
    Console.WriteLine("GESCO DESKTOP API");
    Console.WriteLine("========================================");
    Console.WriteLine($"Environment: {environment.EnvironmentName}");
    Console.WriteLine($"URL: http://localhost:5100");
    Console.WriteLine($"Swagger: http://localhost:5100/swagger");
    Console.WriteLine("========================================");
    Console.WriteLine("SERVICES:");
    Console.WriteLine($"SQLite: ACTIVE");
    Console.WriteLine($"PostgreSQL: {(!string.IsNullOrEmpty(postgresConn) ? "ACTIVE" : "DISABLED")}");
    Console.WriteLine($"Laravel API: {(!string.IsNullOrEmpty(laravelUrl) ? "ACTIVE" : "DISABLED")}");
    Console.WriteLine("========================================");
    Console.WriteLine("DEFAULT CREDENTIALS:");
    Console.WriteLine("Username: admin");
    Console.WriteLine("Password: admin123");
    Console.WriteLine("========================================");
    
    if (environment.IsDevelopment())
    {
        Console.WriteLine("DEV MODE: Detailed logging enabled");
        Console.WriteLine("========================================");
    }
}

// =====================================================
// NULL SERVICES
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