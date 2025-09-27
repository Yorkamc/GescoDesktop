using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Utils;

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
                _logger.LogInformation("🔄 Checking database state...");

                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogInformation("📦 Database doesn't exist, creating...");
                    await context.Database.EnsureCreatedAsync();
                    _logger.LogInformation("✅ Database created successfully");
                }

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("🔄 Applying {Count} pending migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("✅ Migrations applied successfully");
                }

                // EJECUTAR SEED DATA MANUALMENTE
                await EnsureSeedDataAsync(context);

                // Ejecutar script de optimización
                await RunOptimizationScriptAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during database initialization");
                throw;
            }
        }

        private async Task EnsureSeedDataAsync(LocalDbContext context)
        {
            try
            {
                _logger.LogInformation("🔍 Checking seed data...");

                var hasUsers = await context.Users.AnyAsync();
                var hasOrganizations = await context.Organizations.AnyAsync();
                var hasRoles = await context.Roles.AnyAsync();

                if (!hasUsers || !hasOrganizations || !hasRoles)
                {
                    _logger.LogInformation("🌱 Seed data missing, creating...");
                    await CreateSeedDataAsync(context);
                    _logger.LogInformation("✅ Seed data created successfully");
                }
                else
                {
                    _logger.LogInformation("✅ Seed data already exists");
                    
                    // VERIFICAR Y CORREGIR CONTRASEÑA DEL ADMIN
                    await VerifyAndFixAdminPasswordAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ensuring seed data");
                throw;
            }
        }

        private async Task CreateSeedDataAsync(LocalDbContext context)
        {
            // IDS PARA SEED DATA
            var orgId = Guid.NewGuid();
            var adminUserId = "118640123";
            var adminRoleId = 1;
            var salesRoleId = 2;
            var supervisorRoleId = 3;

            // ORGANIZACIÓN
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
                _logger.LogInformation("✅ Organization created: {Name}", organization.Name);
            }

            // ROLES
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
                _logger.LogInformation("✅ Roles created: {Count}", roles.Length);
            }

            // USUARIO ADMIN CON HASH CORRECTO
            if (!await context.Users.AnyAsync(u => u.Username == "admin"))
            {
                // GENERAR HASH CORRECTO USANDO PasswordHelper
                _logger.LogInformation("🔐 Generating admin password hash...");
                var adminPasswordHash = PasswordHelper.HashPassword("admin123");
                
                _logger.LogInformation("📝 Generated hash: {Hash}", adminPasswordHash);
                _logger.LogInformation("📏 Hash length: {Length}", adminPasswordHash.Length);
                
                // Verificar que el hash funciona inmediatamente
                var testVerification = PasswordHelper.VerifyPassword("admin123", adminPasswordHash);
                _logger.LogInformation("🧪 Hash verification test: {Result}", testVerification);
                
                if (!testVerification)
                {
                    throw new InvalidOperationException("Generated password hash failed verification test");
                }

                var adminUser = new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Email = "admin@gesco.com",
                    Password = adminPasswordHash, // HASH CORRECTO
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
                _logger.LogInformation("✅ Admin user created: {Username} with ID: {UserId}", 
                    adminUser.Username, adminUser.Id);
            }

            // CREAR RESTO DE SEED DATA
            await CreateAdditionalSeedDataAsync(context);
        }

        private async Task CreateAdditionalSeedDataAsync(LocalDbContext context)
        {
            // ACTIVITY STATUSES
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
                _logger.LogInformation("✅ Activity statuses created: {Count}", statuses.Length);
            }

            // SALES STATUSES
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
                _logger.LogInformation("✅ Sales statuses created: {Count}", salesStatuses.Length);
            }

            // PAYMENT METHODS
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
                _logger.LogInformation("✅ Payment methods created: {Count}", paymentMethods.Length);
            }

            // INVENTORY MOVEMENT TYPES
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
                _logger.LogInformation("✅ Inventory movement types created: {Count}", movementTypes.Length);
            }

            // SYSTEM CONFIGURATION
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
                _logger.LogInformation("✅ System configurations created: {Count}", configs.Length);
            }
        }

        /// <summary>
        /// Verifica y corrige la contraseña del admin si es necesario
        /// </summary>
        private async Task VerifyAndFixAdminPasswordAsync(LocalDbContext context)
        {
            try
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser == null)
                {
                    _logger.LogWarning("⚠️ Admin user not found");
                    return;
                }

                _logger.LogInformation("🔍 Verifying admin password hash...");
                _logger.LogInformation("📝 Current hash: {Hash}", adminUser.Password);
                _logger.LogInformation("📏 Hash length: {Length}", adminUser.Password?.Length ?? 0);

                // Verificar si el hash actual es válido
                var isValidFormat = PasswordHelper.IsValidBCryptHash(adminUser.Password);
                _logger.LogInformation("🔍 Hash format valid: {IsValid}", isValidFormat);

                if (!isValidFormat)
                {
                    _logger.LogWarning("⚠️ Admin password hash is invalid, regenerating...");
                    await RegenerateAdminPasswordAsync(context, adminUser);
                    return;
                }

                // Verificar que la contraseña funciona
                var passwordWorks = PasswordHelper.VerifyPassword("admin123", adminUser.Password);
                _logger.LogInformation("🧪 Password verification test: {Works}", passwordWorks);

                if (!passwordWorks)
                {
                    _logger.LogWarning("⚠️ Admin password verification failed, regenerating...");
                    await RegenerateAdminPasswordAsync(context, adminUser);
                }
                else
                {
                    _logger.LogInformation("✅ Admin password is working correctly");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error verifying admin password");
            }
        }

        /// <summary>
        /// Regenera la contraseña del admin
        /// </summary>
        private async Task RegenerateAdminPasswordAsync(LocalDbContext context, User adminUser)
        {
            try
            {
                _logger.LogInformation("🔧 Regenerating admin password...");
                
                // Generar nuevo hash
                var newHash = PasswordHelper.HashPassword("admin123");
                _logger.LogInformation("📝 New hash generated: {Hash}", newHash);
                _logger.LogInformation("📏 New hash length: {Length}", newHash.Length);
                
                // Verificar inmediatamente que funciona
                var testResult = PasswordHelper.VerifyPassword("admin123", newHash);
                _logger.LogInformation("🧪 New hash verification test: {Result}", testResult);
                
                if (!testResult)
                {
                    throw new InvalidOperationException("Generated hash failed verification test");
                }
                
                // Actualizar en base de datos
                adminUser.Password = newHash;
                adminUser.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                
                _logger.LogInformation("✅ Admin password regenerated successfully");
                
                // Log adicional para debugging
                _logger.LogInformation("🔍 Final verification test...");
                var finalTest = PasswordHelper.VerifyPassword("admin123", adminUser.Password);
                _logger.LogInformation("🧪 Final test result: {Result}", finalTest);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error regenerating admin password");
                throw;
            }
        }

        public async Task RunOptimizationScriptAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                _logger.LogInformation("🔧 Running SQLite optimization script...");
                await context.RunOptimizationScriptAsync();
                _logger.LogInformation("✅ Optimization script completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error running optimization script");
                // No relanzar la excepción para que no falle el startup
            }
        }
    }

    // CORREGIDO: DatabaseInitializationService funciona con IMigrationService
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
                _logger.LogInformation("🚀 Starting database initialization...");
                await _migrationService.EnsureDatabaseCreatedAsync();
                _logger.LogInformation("✅ Database initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Database initialization failed");
                throw; // Fallar startup si la DB no se puede inicializar
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}