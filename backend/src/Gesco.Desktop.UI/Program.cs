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
using DotNetEnv;
using Gesco.Desktop.Core.Security;

var builder = WebApplication.CreateBuilder(args);

try { Env.Load(); } catch { }

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(
    builder.Environment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning
);

builder.WebHost.UseUrls("http://localhost:5100");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

builder.Services.AddSingleton<DatabaseEncryption>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IBackupService, BackupService>();

builder.Services.AddScoped<ICachedLookupService, CachedLookupService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddSingleton<IMigrationService>(provider =>
    new MigrationService(provider, provider.GetRequiredService<ILogger<MigrationService>>()));

builder.Services.AddHostedService<DatabaseInitializationService>();

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

var app = builder.Build();

await InitializeEncryption(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GESCO API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("AllowFrontend");
app.UseResponseCaching();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapGet("/ping", () => Results.Ok(new { 
    status = "ok", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithTags("System");

PrintStartupInfo(app.Environment, postgresConnectionString, laravelApiUrl);

app.Run();

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
        
        await Task.CompletedTask;
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
    Console.WriteLine($"Memory Cache: ACTIVE");
    Console.WriteLine($"Response Cache: ACTIVE");
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