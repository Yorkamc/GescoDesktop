using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
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

                // Aplicar migraciones pendientes si las hay
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("Migrations applied successfully");
                }

                // ✅ EJECUTAR SEED DATA MANUALMENTE
                await EnsureSeedDataAsync(context);

                // Ejecutar script de optimización después de las migraciones
                await RunOptimizationScriptAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization");
                throw;
            }
        }

        // ✅ MÉTODO PARA FORZAR SEED DATA
        private async Task EnsureSeedDataAsync(LocalDbContext context)
        {
            try
            {
                _logger.LogInformation("Checking seed data...");

                // Verificar si ya hay datos
                var hasUsers = await context.Users.AnyAsync();
                var hasOrganizations = await context.Organizations.AnyAsync();
                var hasRoles = await context.Roles.AnyAsync();

                if (!hasUsers || !hasOrganizations || !hasRoles)
                {
                    _logger.LogInformation("Seed data missing, creating...");
                    await CreateSeedDataAsync(context);
                    _logger.LogInformation("Seed data created successfully");
                }
                else
                {
                    _logger.LogInformation("Seed data already exists");
                    
                    // ✅ VERIFICAR QUE EL ADMIN TENGA LA CONTRASEÑA CORRECTA
                    await VerifyAdminPasswordAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring seed data");
                throw;
            }
        }

        // ✅ CREAR SEED DATA MANUALMENTE
        private async Task CreateSeedDataAsync(LocalDbContext context)
        {
            // ============================================
            // IDS PARA SEED DATA
            // ============================================
            var orgId = Guid.NewGuid();
            var adminUserId = "118640123";
            var adminRoleId = 1;
            var salesRoleId = 2;
            var supervisorRoleId = 3;

            // ============================================
            // ORGANIZACIÓN
            // ============================================
            if (!await context.Organizations.AnyAsync())
            {
                var organization = new Organization
                {
                    Id = orgId,
                    Name = "Demo Organization",
                    ContactEmail = "demo@gesco.com",
                    ContactPhone = "2222-2222",
                    Address = "San José, Costa Rica",
                    PurchaserName = "Demo Administrator",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Organizations.Add(organization);
                await context.SaveChangesAsync();
                _logger.LogInformation("Organization created: {Name}", organization.Name);
            }

            // ============================================
            // ROLES
            // ============================================
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role
                    {
                        Id = adminRoleId,
                        Name = "Administrator",
                        Description = "Full system access",
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Id = salesRoleId,
                        Name = "Salesperson",
                        Description = "Sales and cash register access",
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Id = supervisorRoleId,
                        Name = "Supervisor",
                        Description = "Activity supervision",
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
                _logger.LogInformation("Roles created: {Count}", roles.Length);
            }

            // ============================================
            // USUARIO ADMIN CON CONTRASEÑA CORRECTA
            // ============================================
            if (!await context.Users.AnyAsync(u => u.Username == "admin"))
            {
                // ✅ USAR EL HASH QUE REALMENTE FUNCIONA CON TU PASSWORDHELPER
                var adminPasswordHash = "$2a$12$LQV.K4/OOOgwdEXCfC7jC.QLwpZ9HkqhXfOr9p6mTyYFEYGHZcP/a";

                var adminUser = new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Email = "admin@gesco.com",
                    Password = adminPasswordHash, // ✅ Hash que funciona
                    FullName = "System Administrator",
                    Phone = "8888-8888",
                    OrganizationId = orgId,
                    RoleId = adminRoleId,
                    Active = true,
                    FirstLogin = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                _logger.LogInformation("Admin user created: {Username} with ID: {UserId}", adminUser.Username, adminUser.Id);
            }

            // ============================================
            // ACTIVITY STATUSES
            // ============================================
            if (!await context.ActivityStatuses.AnyAsync())
            {
                var statuses = new[]
                {
                    new ActivityStatus { Id = 1, Name = "Not Started", Description = "Activity not started", Active = true, CreatedAt = DateTime.UtcNow },
                    new ActivityStatus { Id = 2, Name = "In Progress", Description = "Activity in development", Active = true, CreatedAt = DateTime.UtcNow },
                    new ActivityStatus { Id = 3, Name = "Completed", Description = "Activity completed", Active = true, CreatedAt = DateTime.UtcNow },
                    new ActivityStatus { Id = 4, Name = "Cancelled", Description = "Activity cancelled", Active = true, CreatedAt = DateTime.UtcNow }
                };

                context.ActivityStatuses.AddRange(statuses);
                await context.SaveChangesAsync();
                _logger.LogInformation("Activity statuses created: {Count}", statuses.Length);
            }

            // ============================================
            // SALES STATUSES
            // ============================================
            if (!await context.SalesStatuses.AnyAsync())
            {
                var salesStatuses = new[]
                {
                    new SalesStatus { Id = 1, Name = "Pending", Description = "Sale pending processing", Active = true, CreatedAt = DateTime.UtcNow },
                    new SalesStatus { Id = 2, Name = "Completed", Description = "Sale completed successfully", Active = true, CreatedAt = DateTime.UtcNow },
                    new SalesStatus { Id = 3, Name = "Cancelled", Description = "Sale cancelled", Active = true, CreatedAt = DateTime.UtcNow }
                };

                context.SalesStatuses.AddRange(salesStatuses);
                await context.SaveChangesAsync();
                _logger.LogInformation("Sales statuses created: {Count}", salesStatuses.Length);
            }

            // ============================================
            // PAYMENT METHODS
            // ============================================
            if (!await context.PaymentMethods.AnyAsync())
            {
                var paymentMethods = new[]
                {
                    new PaymentMethod { Id = 1, Name = "Cash", RequiresReference = false, Active = true, CreatedAt = DateTime.UtcNow },
                    new PaymentMethod { Id = 2, Name = "Card", RequiresReference = true, Active = true, CreatedAt = DateTime.UtcNow },
                    new PaymentMethod { Id = 3, Name = "SINPE Mobile", RequiresReference = true, Active = true, CreatedAt = DateTime.UtcNow }
                };

                context.PaymentMethods.AddRange(paymentMethods);
                await context.SaveChangesAsync();
                _logger.LogInformation("Payment methods created: {Count}", paymentMethods.Length);
            }

            // ============================================
            // INVENTORY MOVEMENT TYPES
            // ============================================
            if (!await context.InventoryMovementTypes.AnyAsync())
            {
                var movementTypes = new[]
                {
                    new InventoryMovementType { Id = 1, Name = "Stock In", Description = "Merchandise entry to inventory", RequiresJustification = false, Active = true, CreatedAt = DateTime.UtcNow },
                    new InventoryMovementType { Id = 2, Name = "Sale", Description = "Stock out by product sale", RequiresJustification = false, Active = true, CreatedAt = DateTime.UtcNow },
                    new InventoryMovementType { Id = 3, Name = "Adjustment", Description = "Inventory adjustment for differences", RequiresJustification = true, Active = true, CreatedAt = DateTime.UtcNow }
                };

                context.InventoryMovementTypes.AddRange(movementTypes);
                await context.SaveChangesAsync();
                _logger.LogInformation("Inventory movement types created: {Count}", movementTypes.Length);
            }

            // ============================================
            // SYSTEM CONFIGURATION
            // ============================================
            if (!await context.SystemConfigurations.AnyAsync())
            {
                var configs = new[]
                {
                    new SystemConfiguration { Id = 1, Key = "system.version", Value = "1.0.0", DataType = "string", Category = "system", Description = "System version", IsEditable = false, AccessLevel = "admin", CreatedAt = DateTime.UtcNow },
                    new SystemConfiguration { Id = 2, Key = "backup.interval_hours", Value = "6", DataType = "int", Category = "backup", Description = "Backup interval in hours", IsEditable = true, AccessLevel = "admin", CreatedAt = DateTime.UtcNow },
                    new SystemConfiguration { Id = 3, Key = "license.check_interval_days", Value = "7", DataType = "int", Category = "license", Description = "License check interval in days", IsEditable = true, AccessLevel = "admin", CreatedAt = DateTime.UtcNow }
                };

                context.SystemConfigurations.AddRange(configs);
                await context.SaveChangesAsync();
                _logger.LogInformation("System configurations created: {Count}", configs.Length);
            }

            _logger.LogInformation("✅ All seed data created successfully");
        }

        // ✅ VERIFICAR Y CORREGIR CONTRASEÑA DEL ADMIN
        private async Task VerifyAdminPasswordAsync(LocalDbContext context)
        {
            try
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser != null)
                {
                    // ✅ TU HASH QUE FUNCIONA
                    var correctHash = "$2a$12$LQV.K4/OOOgwdEXCfC7jC.QLwpZ9HkqhXfOr9p6mTyYFEYGHZcP/a";
                    
                    if (adminUser.Password != correctHash)
                    {
                        _logger.LogInformation("Updating admin password hash...");
                        adminUser.Password = correctHash;
                        adminUser.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Admin password hash updated to working hash");
                    }
                    
                    // ✅ VERIFICAR QUE FUNCIONA CON PASSWORDHELPER
                    try 
                    {
                        var testVerify = Gesco.Desktop.Core.Utils.PasswordHelper.VerifyPassword("admin123", adminUser.Password);
                        _logger.LogInformation("Password verification test result: {Result}", testVerify);
                        
                        if (!testVerify)
                        {
                            _logger.LogWarning("Password verification failed! Hash may be incompatible with PasswordHelper");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error testing password verification");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying admin password");
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