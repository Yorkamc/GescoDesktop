using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(IServiceProvider serviceProvider, ILogger<MigrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                _logger.LogInformation("Checking database state...");

                // Verificar si la base de datos existe
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogInformation("Database doesn't exist, creating...");
                    await context.Database.EnsureCreatedAsync();
                    _logger.LogInformation("Database created successfully");
                }

                // Aplicar migraciones pendientes
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("Migrations applied successfully");
                }

                // Ejecutar script de optimización después de las migraciones
                await RunOptimizationScriptAsync();

                // Verificar datos semilla
                await VerifySeedDataAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization");
                throw;
            }
        }

        public async Task RunOptimizationScriptAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                _logger.LogInformation("Running SQLite optimization script...");
                await context.RunOptimizationScriptAsync();
                _logger.LogInformation("Optimization script completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running optimization script");
                // No relanzar la excepción para que no falle el startup
            }
        }

        private async Task VerifySeedDataAsync(LocalDbContext context)
        {
            try
            {
                // Verificar usuario admin
                var adminUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                if (adminUser != null)
                {
                    _logger.LogInformation("Admin user found: {Email}", adminUser.Email);
                }
                else
                {
                    _logger.LogWarning("Admin user not found in database");
                }

                // Verificar conteos básicos
                var userCount = await context.Users.CountAsync();
                var roleCount = await context.Roles.CountAsync();
                var orgCount = await context.Organizations.CountAsync();

                _logger.LogInformation("Database verification: Users: {UserCount}, Roles: {RoleCount}, Organizations: {OrgCount}",
                    userCount, roleCount, orgCount);

                if (userCount == 0 || roleCount == 0 || orgCount == 0)
                {
                    _logger.LogWarning("Missing seed data detected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying seed data");
            }
        }
    }

    // CORREGIDO: DatabaseInitializationService ahora funciona con IMigrationService como Singleton
    public class DatabaseInitializationService : IHostedService
    {
        private readonly IMigrationService _migrationService;
        private readonly ILogger<DatabaseInitializationService> _logger;

        public DatabaseInitializationService(
            IMigrationService migrationService,
            ILogger<DatabaseInitializationService> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting database initialization...");
                await _migrationService.EnsureDatabaseCreatedAsync();
                _logger.LogInformation("Database initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                // Decidir si fallar el startup o continuar
                throw; // Fallar startup si la DB no se puede inicializar
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}