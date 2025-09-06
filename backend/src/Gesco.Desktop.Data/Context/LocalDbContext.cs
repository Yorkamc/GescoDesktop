using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        // ============================================
        // DBSETS - NUEVAS ENTIDADES EN INGLÉS
        // ============================================
        
        // Core System
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        // Memberships and Subscriptions
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ActivationKey> ActivationKeys { get; set; }

        // Activities
        public DbSet<ActivityStatus> ActivityStatuses { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }

        // Sales
        public DbSet<SalesStatus> SalesStatuses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<TransactionPayment> TransactionPayments { get; set; }

        // Combos
        public DbSet<SalesCombo> SalesCombos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }

        // Inventory
        public DbSet<InventoryMovementType> InventoryMovementTypes { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        // Closures
        public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
        public DbSet<ActivityClosure> ActivityClosures { get; set; }

        // ============================================
        // CONSTRUCTORS
        // ============================================
        public LocalDbContext() { }
        
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        // ============================================
        // DATABASE CONFIGURATION
        // ============================================
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_local.db");
                var directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        // ============================================
        // MODEL CONFIGURATION
        // ============================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default delete behavior to Restrict
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ============================================
            // SEED DATA
            // ============================================
            SeedData(modelBuilder);
        }

        // ============================================
        // SEED DATA METHOD
        // ============================================
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Default Organization
            modelBuilder.Entity<Organization>().HasData(
                new Organization
                {
                    Id = Guid.NewGuid(),
                    Name = "Demo Organization",
                    ContactEmail = "demo@gesco.com",
                    ContactPhone = "2222-2222",
                    Address = "San José, Costa Rica",
                    PurchaserName = "Demo Administrator",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // System Roles
            modelBuilder.Entity<Role>().HasData(
                new Role 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Administrator", 
                    Description = "Full system access", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Role 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Salesperson", 
                    Description = "Sales and cash register access", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Role 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Supervisor", 
                    Description = "Activity supervision", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Admin User - Using BCrypt hash for "admin123"
            var adminPasswordHash = "$2a$12$6nybiEVKavFp/iZhsQrSLuNIhhAnRx2STs6Fmzj.BCF4gUAwMtCV6";
            var adminRoleId = Guid.NewGuid();
            var adminOrgId = Guid.NewGuid();
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Email = "admin@gesco.com",
                    Password = adminPasswordHash,
                    FullName = "System Administrator",
                    Phone = "8888-8888",
                    OrganizationId = adminOrgId,
                    RoleId = 1, // Will need to be updated to match role GUID
                    Active = true,
                    FirstLogin = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Activity Statuses
            modelBuilder.Entity<ActivityStatus>().HasData(
                new ActivityStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Not Started", 
                    Description = "Activity not started", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "In Progress", 
                    Description = "Activity in development", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Completed", 
                    Description = "Activity completed", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Cancelled", 
                    Description = "Activity cancelled", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Sales Statuses
            modelBuilder.Entity<SalesStatus>().HasData(
                new SalesStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Pending", 
                    Description = "Sale pending processing", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new SalesStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Completed", 
                    Description = "Sale completed successfully", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new SalesStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Cancelled", 
                    Description = "Sale cancelled", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Payment Methods
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Cash", 
                    RequiresReference = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new PaymentMethod 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Card", 
                    RequiresReference = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new PaymentMethod 
                { 
                    Id = Guid.NewGuid(),
                    Name = "SINPE Mobile", 
                    RequiresReference = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Inventory Movement Types
            modelBuilder.Entity<InventoryMovementType>().HasData(
                new InventoryMovementType 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Stock In", 
                    Description = "Merchandise entry to inventory", 
                    RequiresJustification = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new InventoryMovementType 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Sale", 
                    Description = "Stock out by product sale", 
                    RequiresJustification = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new InventoryMovementType 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Adjustment", 
                    Description = "Inventory adjustment for differences", 
                    RequiresJustification = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Subscription Statuses
            modelBuilder.Entity<SubscriptionStatus>().HasData(
                new SubscriptionStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Active", 
                    Description = "Active subscription", 
                    AllowsSystemUsage = true, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Suspended", 
                    Description = "Suspended subscription", 
                    AllowsSystemUsage = false, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionStatus 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Cancelled", 
                    Description = "Cancelled subscription", 
                    AllowsSystemUsage = false, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}