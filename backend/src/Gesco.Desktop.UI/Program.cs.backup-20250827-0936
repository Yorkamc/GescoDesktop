using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Sync.LaravelApi;
using System.Text;
using DotNetEnv;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5100");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LocalDbContext>(options =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_local.db");
    var directory = Path.GetDirectoryName(dbPath);
    if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivationService, ActivationService>();
builder.Services.AddScoped<ILaravelApiClient, LaravelApiClient>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpClient<ILaravelApiClient, LaravelApiClient>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
    context.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("ReactApp");

// Health check
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.Now }));

// Auth endpoints
app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request.Usuario, request.Password);
    return result.Success ? Results.Ok(result) : Results.Unauthorized();
});

app.MapPost("/api/auth/validate", async (HttpContext context, IAuthService authService) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    if (string.IsNullOrEmpty(token)) return Results.Unauthorized();
    var isValid = await authService.ValidateTokenAsync(token);
    return isValid ? Results.Ok(new { valid = true }) : Results.Unauthorized();
});

app.MapGet("/api/stats", async (LocalDbContext context) =>
{
    var stats = new
    {
        actividades = await context.Actividades.CountAsync(),
        ventasHoy = await context.TransaccionesVenta.Where(t => t.FechaTransaccion.Date == DateTime.Today).SumAsync(t => t.Total),
        transacciones = await context.TransaccionesVenta.Where(t => t.FechaTransaccion.Date == DateTime.Today).CountAsync()
    };
    return Results.Ok(stats);
});

app.Run();
