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
        // DBSETS - ENTIDADES CON TIPOS DE ID CORREGIDOS
        // ============================================
        
        // Core System - GUID (Solo estos dos)
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }

        // Core System - INT (Todos los demás)
        public DbSet<Role> Roles { get; set; }

        // Memberships and Subscriptions - INT
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ActivationKey> ActivationKeys { get; set; } // CORREGIDO: int ID

        // Activities - INT (con FKs mixed)
        public DbSet<ActivityStatus> ActivityStatuses { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }

        // Sales - INT
        public DbSet<SalesStatus> SalesStatuses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<TransactionPayment> TransactionPayments { get; set; }

        // Combos - INT
        public DbSet<SalesCombo> SalesCombos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }

        // Inventory - INT
        public DbSet<InventoryMovementType> InventoryMovementTypes { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }

        // Closures - INT
        public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
        public DbSet<ActivityClosure> ActivityClosures { get; set; }

        // System Configuration - INT
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        // ============================================
        // ALIAS PARA COMPATIBILIDAD CON NOMBRES EN ESPAÑOL
        // ============================================
        
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
            // ============================================
            // USER RELATIONSHIPS - MIXED GUID/INT
            // ============================================
            
            // User (Guid) -> Organization (Guid)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId) // Guid -> Guid ✅
                .OnDelete(DeleteBehavior.Restrict);

            // User (Guid) -> Role (int)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId) // Guid -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // ACTIVITY RELATIONSHIPS - MIXED
            // ============================================
            
            // Activity (int) -> ActivityStatus (int)
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.ActivityStatus)
                .WithMany(s => s.Activities)
                .HasForeignKey(a => a.ActivityStatusId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // Activity (int) -> Organization (Guid)
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Organization)
                .WithMany(o => o.Activities)
                .HasForeignKey(a => a.OrganizationId) // int -> Guid ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // SALES RELATIONSHIPS - INT TO INT
            // ============================================
            
            // SalesTransaction (int) -> SalesStatus (int)
            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.SalesStatus)
                .WithMany(ss => ss.SalesTransactions)
                .HasForeignKey(st => st.SalesStatusId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // SalesTransaction (int) -> CashRegister (int)
            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.CashRegister)
                .WithMany(cr => cr.SalesTransactions)
                .HasForeignKey(st => st.CashRegisterId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // ACTIVITY CATEGORY RELATIONSHIPS - INT TO INT
            // ============================================
            
            // ActivityCategory (int) -> Activity (int)
            modelBuilder.Entity<ActivityCategory>()
                .HasOne(ac => ac.Activity)
                .WithMany(a => a.ActivityCategories)
                .HasForeignKey(ac => ac.ActivityId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityCategory (int) -> ServiceCategory (int)
            modelBuilder.Entity<ActivityCategory>()
                .HasOne(ac => ac.ServiceCategory)
                .WithMany(sc => sc.ActivityCategories)
                .HasForeignKey(ac => ac.ServiceCategoryId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // CATEGORY PRODUCT RELATIONSHIPS - INT TO INT
            // ============================================
            
            // CategoryProduct (int) -> ActivityCategory (int)
            modelBuilder.Entity<CategoryProduct>()
                .HasOne(cp => cp.ActivityCategory)
                .WithMany(ac => ac.CategoryProducts)
                .HasForeignKey(cp => cp.ActivityCategoryId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // INVENTORY RELATIONSHIPS - INT TO INT
            // ============================================
            
            // InventoryMovement (int) -> CategoryProduct (int)
            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.Product)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(im => im.ProductId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryMovement (int) -> InventoryMovementType (int)
            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.MovementType)
                .WithMany(mt => mt.InventoryMovements)
                .HasForeignKey(im => im.MovementTypeId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // TRANSACTION DETAIL RELATIONSHIPS - INT TO INT
            // ============================================
            
            // TransactionDetail (int) -> SalesTransaction (int)
            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.SalesTransaction)
                .WithMany(st => st.TransactionDetails)
                .HasForeignKey(td => td.SalesTransactionId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // TransactionDetail (int) -> CategoryProduct (int)
            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.Product)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(td => td.ProductId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // SUBSCRIPTION RELATIONSHIPS - MIXED GUID/INT
            // ============================================
            
            // Subscription (int) -> Organization (Guid)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Subscriptions)
                .HasForeignKey(s => s.OrganizationId) // int -> Guid ✅
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription (int) -> Membership (int)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Membership)
                .WithMany(m => m.Subscriptions)
                .HasForeignKey(s => s.MembershipId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription (int) -> SubscriptionStatus (int)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionStatus)
                .WithMany(ss => ss.Subscriptions)
                .HasForeignKey(s => s.SubscriptionStatusId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // ACTIVATION KEY RELATIONSHIPS - CORREGIDO
            // ============================================
            
            // ActivationKey (int) -> Subscription (int)
            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.Subscription)
                .WithMany(s => s.ActivationKeys)
                .HasForeignKey(ak => ak.SubscriptionId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // SERVICE CATEGORY RELATIONSHIPS
            // ============================================
            
            // ServiceCategory (int) -> Organization (Guid)
            modelBuilder.Entity<ServiceCategory>()
                .HasOne(sc => sc.Organization)
                .WithMany(o => o.ServiceCategories)
                .HasForeignKey(sc => sc.OrganizationId) // int -> Guid ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // CASH REGISTER RELATIONSHIPS
            // ============================================
            
            // CashRegister (int) -> Activity (int)
            modelBuilder.Entity<CashRegister>()
                .HasOne(cr => cr.Activity)
                .WithMany(a => a.CashRegisters)
                .HasForeignKey(cr => cr.ActivityId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // COMBO RELATIONSHIPS
            // ============================================
            
            // SalesCombo (int) -> Activity (int)
            modelBuilder.Entity<SalesCombo>()
                .HasOne(sc => sc.Activity)
                .WithMany(a => a.SalesCombos)
                .HasForeignKey(sc => sc.ActivityId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ComboItem (int) -> SalesCombo (int)
            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Combo)
                .WithMany(sc => sc.ComboItems)
                .HasForeignKey(ci => ci.ComboId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ComboItem (int) -> CategoryProduct (int)
            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Product)
                .WithMany(cp => cp.ComboItems)
                .HasForeignKey(ci => ci.ProductId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // PAYMENT RELATIONSHIPS
            // ============================================
            
            // TransactionPayment (int) -> SalesTransaction (int)
            modelBuilder.Entity<TransactionPayment>()
                .HasOne(tp => tp.SalesTransaction)
                .WithMany(st => st.TransactionPayments)
                .HasForeignKey(tp => tp.SalesTransactionId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // TransactionPayment (int) -> PaymentMethod (int)
            modelBuilder.Entity<TransactionPayment>()
                .HasOne(tp => tp.PaymentMethod)
                .WithMany(pm => pm.TransactionPayments)
                .HasForeignKey(tp => tp.PaymentMethodId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // CLOSURE RELATIONSHIPS
            // ============================================
            
            // CashRegisterClosure (int) -> CashRegister (int)
            modelBuilder.Entity<CashRegisterClosure>()
                .HasOne(crc => crc.CashRegister)
                .WithMany(cr => cr.CashRegisterClosures)
                .HasForeignKey(crc => crc.CashRegisterId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);

            // ActivityClosure (int) -> Activity (int)
            modelBuilder.Entity<ActivityClosure>()
                .HasOne(ac => ac.Activity)
                .WithMany()
                .HasForeignKey(ac => ac.ActivityId) // int -> int ✅
                .OnDelete(DeleteBehavior.Restrict);
        }

        // ============================================
        // SEED DATA METHOD - CORREGIDO CON TIPOS EXACTOS
        // ============================================
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // ============================================
            // IDS PARA SEED DATA - TIPOS CORRECTOS
            // ============================================
            
            // GUID IDs (Solo Organization y User)
            var orgId = Guid.NewGuid(); // Organization
            var adminUserId = Guid.NewGuid(); // User
            
            // INT IDs (Todos los demás)
            var adminRoleId = 1; // Role
            var salesRoleId = 2; // Role  
            var supervisorRoleId = 3; // Role

            // ============================================
            // DEFAULT ORGANIZATION - GUID ID
            // ============================================
            
            modelBuilder.Entity<Organization>().HasData(
                new Organization
                {
                    Id = orgId, // ✅ Guid
                    Name = "Demo Organization",
                    ContactEmail = "demo@gesco.com",
                    ContactPhone = "2222-2222",
                    Address = "San José, Costa Rica",
                    PurchaserName = "Demo Administrator",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // ============================================
            // SYSTEM ROLES - INT ID
            // ============================================
            
            modelBuilder.Entity<Role>().HasData(
                new Role 
                { 
                    Id = adminRoleId, // ✅ int
                    Name = "Administrator", 
                    Description = "Full system access", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Role 
                { 
                    Id = salesRoleId, // ✅ int
                    Name = "Salesperson", 
                    Description = "Sales and cash register access", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Role 
                { 
                    Id = supervisorRoleId, // ✅ int
                    Name = "Supervisor", 
                    Description = "Activity supervision", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // ============================================
            // ADMIN USER - GUID ID CON FKS MIXED
            // ============================================
            
            // BCrypt hash para "admin123"
            var adminPasswordHash = "$2a$12$6nybiEVKavFp/iZhsQrSLuNIhhAnRx2STs6Fmzj.BCF4gUAwMtCV6";
            
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId, // ✅ Guid (User ID)
                    Username = "admin",
                    Email = "admin@gesco.com",
                    Password = adminPasswordHash,
                    FullName = "System Administrator",
                    Phone = "8888-8888",
                    OrganizationId = orgId, // ✅ Guid -> Organization (Guid)
                    RoleId = adminRoleId, // ✅ int -> Role (int)
                    Active = true,
                    FirstLogin = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // ============================================
            // ACTIVITY STATUSES - INT IDS
            // ============================================
            
            modelBuilder.Entity<ActivityStatus>().HasData(
                new ActivityStatus 
                { 
                    Id = 1, // ✅ int
                    Name = "Not Started", 
                    Description = "Activity not started", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = 2, // ✅ int
                    Name = "In Progress", 
                    Description = "Activity in development", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = 3, // ✅ int
                    Name = "Completed", 
                    Description = "Activity completed", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new ActivityStatus 
                { 
                    Id = 4, // ✅ int
                    Name = "Cancelled", 
                    Description = "Activity cancelled", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // ============================================
            // SALES STATUSES - INT IDS
            // ============================================
            
            modelBuilder.Entity<SalesStatus>().HasData(
                new SalesStatus 
                { 
                    Id = 1, // ✅ int
                    Name = "Pending", 
                    Description = "Sale pending processing", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new SalesStatus 
                { 
                    Id = 2, // ✅ int
                    Name = "Completed", 
                    Description = "Sale completed successfully", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new SalesStatus 
                { 
                    Id = 3, // ✅ int
                    Name = "Cancelled", 
                    Description = "Sale cancelled", 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // ============================================
            // PAYMENT METHODS - INT IDS
            // ============================================
            
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod 
                { 
                    Id = 1, // ✅ int
                    Name = "Cash", 
                    RequiresReference = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new PaymentMethod 
                { 
                    Id = 2, // ✅ int
                    Name = "Card", 
                    RequiresReference = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new PaymentMethod 
                { 
                    Id = 3, // ✅ int
                    Name = "SINPE Mobile", 
                    RequiresReference = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // ============================================
            // INVENTORY MOVEMENT TYPES - INT IDS
            // ============================================
            
            modelBuilder.Entity<InventoryMovementType>().HasData(
                new InventoryMovementType 
                { 
                    Id = 1, // ✅ int
                    Name = "Stock In", 
                    Description = "Merchandise entry to inventory", 
                    RequiresJustification = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new InventoryMovementType 
                { 
                    Id = 2, // ✅ int
                    Name = "Sale", 
                    Description = "Stock out by product sale", 
                    RequiresJustification = false, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new InventoryMovementType 
                { 
                    Id = 3, // ✅ int
                    Name = "Adjustment", 
                    Description = "Inventory adjustment for differences", 
                    RequiresJustification = true, 
                    Active = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            );

            // ============================================
            // SUBSCRIPTION STATUSES - INT IDS
            // ============================================
            
            modelBuilder.Entity<SubscriptionStatus>().HasData(
                new SubscriptionStatus 
                { 
                    Id = 1, // ✅ int
                    Name = "Active", 
                    Description = "Active subscription", 
                    AllowsSystemUsage = true, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionStatus 
                { 
                    Id = 2, // ✅ int
                    Name = "Suspended", 
                    Description = "Suspended subscription", 
                    AllowsSystemUsage = false, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionStatus 
                { 
                    Id = 3, // ✅ int
                    Name = "Cancelled", 
                    Description = "Cancelled subscription", 
                    AllowsSystemUsage = false, 
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // ============================================
            // SYSTEM CONFIGURATION - INT IDS
            // ============================================
            
            modelBuilder.Entity<SystemConfiguration>().HasData(
                new SystemConfiguration
                {
                    Id = 1, // ✅ int
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
                    Id = 2, // ✅ int
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
                    Id = 3, // ✅ int
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

            // ============================================
            // MEMBERSHIPS - INT IDS
            // ============================================
            
            modelBuilder.Entity<Membership>().HasData(
                new Membership
                {
                    Id = 1, // ✅ int
                    Name = "Basic",
                    Description = "Basic membership with essential features",
                    MonthlyPrice = 29.99m,
                    AnnualPrice = 299.99m,
                    UserLimit = 5,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Membership
                {
                    Id = 2, // ✅ int
                    Name = "Professional",
                    Description = "Professional membership with advanced features",
                    MonthlyPrice = 59.99m,
                    AnnualPrice = 599.99m,
                    UserLimit = 25,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Membership
                {
                    Id = 3, // ✅ int
                    Name = "Enterprise",
                    Description = "Enterprise membership with unlimited features",
                    MonthlyPrice = 129.99m,
                    AnnualPrice = 1299.99m,
                    UserLimit = 100,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // ============================================
            // DEFAULT SUBSCRIPTION - INT ID CON FK MIXED
            // ============================================
            
            modelBuilder.Entity<Subscription>().HasData(
                new Subscription
                {
                    Id = 1, // ✅ int (Subscription ID)
                    OrganizationId = orgId, // ✅ Guid -> Organization
                    MembershipId = 1, // ✅ int -> Membership
                    SubscriptionStatusId = 1, // ✅ int -> SubscriptionStatus
                    StartDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddMonths(12),
                    GracePeriodEnd = DateTime.UtcNow.AddMonths(12).AddDays(30),
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