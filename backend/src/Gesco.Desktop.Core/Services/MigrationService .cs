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
                _logger.LogInformation("Checking database state...");

                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogInformation("Creating database...");
                    await context.Database.EnsureCreatedAsync();
                }

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} migrations...", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                }

                await EnsureSeedDataAsync(context);
                await RunOptimizationScriptAsync();

                _logger.LogInformation("Database initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }

        private async Task EnsureSeedDataAsync(LocalDbContext context)
        {
            try
            {
                var hasUsers = await context.Users.AnyAsync();
                var hasOrganizations = await context.Organizations.AnyAsync();
                var hasRoles = await context.Roles.AnyAsync();

                if (!hasUsers || !hasOrganizations || !hasRoles)
                {
                    await CreateSeedDataAsync(context);
                }
                else
                {
                    await VerifyAndFixAdminPasswordAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring seed data");
                throw;
            }
        }

        private async Task CreateSeedDataAsync(LocalDbContext context)
        {
            var orgId = Guid.NewGuid();
            var adminUserId = "118640123";
            var adminRoleId = 1;
            var salesRoleId = 2;
            var supervisorRoleId = 3;

            // ORGANIZATION
            if (!await context.Organizations.AnyAsync())
            {
                var organization = new Organization
                {
                    Id = orgId,
                    Name = "Demo Organization",
                    ContactEmail = "demo@gesco.com",
                    ContactPhone = "2222-2222",
                    Address = "San Jose, Costa Rica",
                    PurchaserName = "Demo Administrator",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Organizations.Add(organization);
                await context.SaveChangesAsync();
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
            }

            // ADMIN USER
            if (!await context.Users.AnyAsync(u => u.Username == "admin"))
            {
                var adminPasswordHash = PasswordHelper.HashPassword("admin123");
                
                var testVerification = PasswordHelper.VerifyPassword("admin123", adminPasswordHash);
                if (!testVerification)
                {
                    throw new InvalidOperationException("Password hash verification failed");
                }

                var adminUser = new User
                {
                    Id = adminUserId,
                    Username = "admin",
                    Email = "admin@gesco.com",
                    Password = adminPasswordHash,
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
                _logger.LogInformation("Admin user created: {Username} with ID: {UserId}", 
                    adminUser.Username, adminUser.Id);
            }

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
            }

            // SYSTEM CONFIGURATION
            if (!await context.SystemConfigurations.AnyAsync())
            {
                var configs = new[]
                {
                    new SystemConfiguration
                    {
                        Id = 1,
                        Key = "system.version",
                        Value = "1.0.0",
                        DataType = "string",
                        Category = "system",
                        Description = "System version",
                        IsEditable = false,
                        AccessLevel = "admin",
                        CreatedAt = DateTime.UtcNow
                    },
                    new SystemConfiguration
                    {
                        Id = 2,
                        Key = "backup.interval_hours",
                        Value = "6",
                        DataType = "int",
                        Category = "backup",
                        Description = "Backup interval in hours",
                        IsEditable = true,
                        AccessLevel = "admin",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.SystemConfigurations.AddRange(configs);
                await context.SaveChangesAsync();
            }
        }

        private async Task VerifyAndFixAdminPasswordAsync(LocalDbContext context)
        {
            try
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser == null) return;

                var isValidFormat = PasswordHelper.IsValidBCryptHash(adminUser.Password);
                if (!isValidFormat)
                {
                    await RegenerateAdminPasswordAsync(context, adminUser);
                    return;
                }

                var passwordWorks = PasswordHelper.VerifyPassword("admin123", adminUser.Password);
                if (!passwordWorks)
                {
                    await RegenerateAdminPasswordAsync(context, adminUser);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying admin password");
            }
        }

        private async Task RegenerateAdminPasswordAsync(LocalDbContext context, User adminUser)
        {
            try
            {
                var newHash = PasswordHelper.HashPassword("admin123");
                var testResult = PasswordHelper.VerifyPassword("admin123", newHash);
                
                if (!testResult)
                {
                    throw new InvalidOperationException("Generated hash failed verification test");
                }
                
                adminUser.Password = newHash;
                adminUser.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Admin password regenerated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating admin password");
                throw;
            }
        }

        public async Task RunOptimizationScriptAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

            try
            {
                await context.RunOptimizationScriptAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running optimization script");
            }
        }
    }

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
                await _migrationService.EnsureDatabaseCreatedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database initialization failed");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}