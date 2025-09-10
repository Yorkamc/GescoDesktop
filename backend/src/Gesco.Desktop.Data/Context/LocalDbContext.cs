using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;
using System.Linq;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        // ============================================
        // DBSETS - ENTIDADES ACTUALIZADAS CON NUEVOS TIPOS DE ID
        // ============================================
        
        // Core System - Guid
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }

        // Core System - int
        public DbSet<Role> Roles { get; set; }

        // Memberships and Subscriptions - int
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ActivationKey> ActivationKeys { get; set; }

        // Activities - int
        public DbSet<ActivityStatus> ActivityStatuses { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }

        // Sales - int
        public DbSet<SalesStatus> SalesStatuses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<TransactionPayment> TransactionPayments { get; set; }

        // Combos - int
        public DbSet<SalesCombo> SalesCombos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }

        // Inventory - int
        public DbSet<InventoryMovementType> InventoryMovementTypes { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        // Closures - int
        public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
        public DbSet<ActivityClosure> ActivityClosures { get; set; }

        // System Configuration - int
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        // ============================================
        // ALIAS PARA COMPATIBILIDAD CON NOMBRES EN ESPAÑOL
        // ============================================
        
        // Mapeo para controladores que usan nombres en español
        public DbSet<Organization> Organizaciones => Organizations;
        public DbSet<User> Usuarios => Users;
        public DbSet<Activity> Actividades => Activities;
        public DbSet<SalesTransaction> TransaccionesVenta => SalesTransactions;
        public DbSet<CategoryProduct> ProductosCategorias => CategoryProducts;
        public DbSet<SystemConfiguration> ConfiguracionesSistema => SystemConfigurations;

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

            // Configure foreign key relationships properly
            ConfigureRelationships(modelBuilder);

            // Seed data
            SeedData(modelBuilder);
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User relationships - Mixed Guid/int
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId) // Guid
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Activity relationships - Mixed
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.ActivityStatus)
                .WithMany(s => s.Activities)
                .HasForeignKey(a => a.ActivityStatusId) // int
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Organization)
                .WithMany(o => o.Activities)
                .HasForeignKey(a => a.OrganizationId) // Guid
                .OnDelete(DeleteBehavior.Restrict);

            // Sales relationships - int to int
            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.SalesStatus)
                .WithMany(ss => ss.SalesTransactions)
                .HasForeignKey(st => st.SalesStatusId) // int
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.CashRegister)
                .WithMany(cr => cr.SalesTransactions)
                .HasForeignKey(st => st.CashRegisterId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Activity Category relationships - int to int
            modelBuilder.Entity<ActivityCategory>()
                .HasOne(ac => ac.Activity)
                .WithMany(a => a.ActivityCategories)
                .HasForeignKey(ac => ac.ActivityId) // int
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityCategory>()
                .HasOne(ac => ac.ServiceCategory)
                .WithMany(sc => sc.ActivityCategories)
                .HasForeignKey(ac => ac.ServiceCategoryId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Category Product relationships - int to int
            modelBuilder.Entity<CategoryProduct>()
                .HasOne(cp => cp.ActivityCategory)
                .WithMany(ac => ac.CategoryProducts)
                .HasForeignKey(cp => cp.ActivityCategoryId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Inventory relationships - int to int
            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.Product)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(im => im.ProductId) // int
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.MovementType)
                .WithMany(mt => mt.InventoryMovements)
                .HasForeignKey(im => im.MovementTypeId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction Detail relationships - int to int
            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.SalesTransaction)
                .WithMany(st => st.TransactionDetails)
                .HasForeignKey(td => td.SalesTransactionId) // int
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.Product)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(td => td.ProductId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription relationships - Mixed Guid/int
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Subscriptions)
                .HasForeignKey(s => s.OrganizationId) // Guid
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Membership)
                .WithMany(m => m.Subscriptions)
                .HasForeignKey(s => s.MembershipId) // int
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionStatus)
                .WithMany(ss => ss.Subscriptions)
                .HasForeignKey(s => s.SubscriptionStatusId) // int
                .OnDelete(DeleteBehavior.Restrict);

            // Activation Key relationships - int to int
            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.Subscription)
                .WithMany(s => s.ActivationKeys)
                .HasForeignKey(ak => ak.SubscriptionId) // int
                .OnDelete(DeleteBehavior.Restrict);
        }

        // ============================================
        // SEED DATA METHOD - ACTUALIZADO CON NUEVOS TIPOS
        // ============================================
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Generate IDs
            var orgId = Guid.NewGuid(); // Organization - Guid
            var adminRoleId = 1; // Role - int
            var salesRoleId = 2; // Role - int  
            var supervisorRoleId = 3; // Role - int

            // Default Organization - Guid ID
            modelBuilder.Entity<Organization>().HasData(
                new Organization
                {
                    Id = orgId,
                    Name = "Demo Organization",
                    ContactEmail = "demo@gesco.com",
                    ContactPhone = "2222-2222",
                    Address = "San José, Costa Rica",
                    PurchaserName = "Demo Administrator",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // System Roles - int ID
            modelBuilder.Entity<Role>().HasData(
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
            );

            // Admin User - Using BCrypt hash for "admin123"
            var adminPasswordHash = "$2a$12$6nybiEVKavFp/iZhsQrSLuNIhhAnRx2STs6Fmzj.BCF4gUAwMtCV6";
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.NewGuid(), // User - Guid
                    Username = "admin",
                    Email = "admin@gesco.com",
                    Password = adminPasswordHash,
                    FullName = "System Administrator",
                    Phone = "8888-8888",
                    OrganizationId = orgId, // FK Guid
                    RoleId = adminRoleId, // FK int
                    Active = true,
                    FirstLogin = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Activity Statuses - int IDs
            modelBuilder.Entity<ActivityStatus>().HasData(
                new ActivityStatus 
                { 
                    Id = 1,
                    Name = "Not Started", 
                    Description = "Activity not started", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = 2,
                    Name = "In Progress", 
                    Description = "Activity in development", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = 3,
                    Name = "Completed", 
                    Description = "Activity completed", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = 4,
                    Name = "Cancelled", 
                    Description = "Activity cancelled", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Sales Statuses - int IDs
            modelBuilder.Entity<SalesStatus>().HasData(
                new SalesStatus 
                { 
                    Id = 1,
                    Name = "Pending", 
                    Description = "Sale pending processing", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new SalesStatus 
                { 
                    Id = 2,
                    Name = "Completed", 
                    Description = "Sale completed successfully", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new SalesStatus 
                { 
                    Id = 3,
                    Name = "Cancelled", 
                    Description = "Sale cancelled", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Payment Methods - int IDs
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod 
                { 
                    Id = 1,
                    Name = "Cash", 
                    RequiresReference = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new PaymentMethod 
                { 
                    Id = 2,
                    Name = "Card", 
                    RequiresReference = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new PaymentMethod 
                { 
                    Id = 3,
                    Name = "SINPE Mobile", 
                    RequiresReference = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Inventory Movement Types - int IDs
            modelBuilder.Entity<InventoryMovementType>().HasData(
                new InventoryMovementType 
                { 
                    Id = 1,
                    Name = "Stock In", 
                    Description = "Merchandise entry to inventory", 
                    RequiresJustification = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new InventoryMovementType 
                { 
                    Id = 2,
                    Name = "Sale", 
                    Description = "Stock out by product sale", 
                    RequiresJustification = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new InventoryMovementType 
                { 
                    Id = 3,
                    Name = "Adjustment", 
                    Description = "Inventory adjustment for differences", 
                    RequiresJustification = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // Subscription Statuses - int IDs
            modelBuilder.Entity<SubscriptionStatus>().HasData(
                new SubscriptionStatus 
                { 
                    Id = 1,
                    Name = "Active", 
                    Description = "Active subscription", 
                    AllowsSystemUsage = true, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionStatus 
                { 
                    Id = 2,
                    Name = "Suspended", 
                    Description = "Suspended subscription", 
                    AllowsSystemUsage = false, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionStatus 
                { 
                    Id = 3,
                    Name = "Cancelled", 
                    Description = "Cancelled subscription", 
                    AllowsSystemUsage = false, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // System Configuration - int IDs
            modelBuilder.Entity<SystemConfiguration>().HasData(
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
                },
                new SystemConfiguration
                {
                    Id = 3,
                    Key = "license.check_interval_days",
                    Value = "7",
                    DataType = "int",
                    Category = "license",
                    Description = "License check interval in days",
                    IsEditable = true,
                    AccessLevel = "admin",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }

        // ============================================
        // OPTIMIZATION SCRIPT EXECUTION
        // ============================================
        public async Task RunOptimizationScriptAsync()
        {
            try
            {
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), 
                    "..", "..", "src", "Gesco.Desktop.Data", "script", "sqlite_optimization_script.sql");
                
                if (File.Exists(scriptPath))
                {
                    var script = await File.ReadAllTextAsync(scriptPath);
                    
                    // Split script by semicolons and execute each statement
                    var statements = script.Split(new[] { ";\r\n", ";\n", ";" }, 
                        StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var statement in statements)
                    {
                        var cleanStatement = statement.Trim();
                        if (!string.IsNullOrEmpty(cleanStatement) && 
                            !cleanStatement.StartsWith("--") && 
                            !cleanStatement.StartsWith("/*"))
                        {
                            try
                            {
                                await Database.ExecuteSqlRawAsync(cleanStatement);
                            }
                            catch (Exception ex)
                            {
                                // Log but don't throw - some statements might not be compatible
                                Console.WriteLine($"Warning executing optimization statement: {ex.Message}");
                            }
                        }
                    }
                    
                    Console.WriteLine("✅ SQLite optimization script executed successfully");
                }
                else
                {
                    Console.WriteLine("⚠️ Optimization script not found at: " + scriptPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error executing optimization script: {ex.Message}");
            }
        }
    }
}