using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
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

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LocalDbContext>(options =>
{
    // Usar la misma ruta que LocalDbContext
    var dbPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "data",
        Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? "gesco_local.db"
    );
    
    // Crear directorio si no existe
    var directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
    
    options.UseSqlite($"Data Source={dbPath}");
    options.EnableSensitiveDataLogging(); // Para debugging
});

// Servicios de negocio
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<ILaravelApiClient, LaravelApiClient>();
builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>();

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalElectron", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Crear base de datos si no existe
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
    context.Database.EnsureCreated();
    Console.WriteLine(" Base de datos local verificada/creada");
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LocalElectron");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// ENDPOINTS
// =====================================================

// Health check
app.MapGet("/api/health", () => Results.Ok(new
{

    status = "healthy",

    timestamp = DateTime.Now,
    version = "1.0.0"
}));

// Login
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request.Usuario, request.Password);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// Logout
app.MapPost("/api/auth/logout", async (IAuthService authService) =>
{
    await authService.LogoutAsync();
    return Results.Ok(new { message = "Sesión cerrada" });
});

// Activar licencia
app.MapPost("/api/license/activate", async (ActivationRequest request, IActivationService activationService) =>
{
    var result = await activationService.ActivateAsync(request.CodigoActivacion, request.OrganizacionId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// Estado de licencia
app.MapGet("/api/license/status", async (IActivationService activationService) =>
{
    var status = await activationService.GetLicenseStatusAsync();
    return Results.Ok(status);
});

// Servir archivo de login por defecto
app.MapFallbackToFile("wwwroot/login.html");

app.Run();
