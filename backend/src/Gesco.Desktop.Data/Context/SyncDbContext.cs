// ============================================
// ARCHIVO: backend/src/Gesco.Desktop.Data/Context/SyncDbContext.cs
// ============================================

using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Gesco.Desktop.Data.Context
{
    /// <summary>
    /// DbContext para sincronización con PostgreSQL
    /// Usa las mismas entidades que LocalDbContext pero optimizado para PostgreSQL
    /// </summary>
    public class SyncDbContext : DbContext
    {
        // ============================================
        // DBSETS - EXACTAMENTE IGUALES que LocalDbContext
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

        // System Configuration
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        // ============================================
        // CONSTRUCTORS
        // ============================================
        public SyncDbContext() { }
        
        public SyncDbContext(DbContextOptions<SyncDbContext> options) : base(options) { }

        // ============================================
        // DATABASE CONFIGURATION
        // ============================================
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback connection string para desarrollo
                var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING") ??
                                     "Host=localhost;Database=gesco_sync;Username=postgres;Password=password";
                
                optionsBuilder.UseNpgsql(connectionString);
                
                // Solo en desarrollo
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    optionsBuilder.EnableSensitiveDataLogging();
                    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
                }
            }
        }

        // ============================================
        // MODEL CONFIGURATION
        // ============================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configurar PostgreSQL específicos
            ConfigurePostgreSQLSpecifics(modelBuilder);
            
            // 2. Configurar relaciones (iguales que LocalDbContext)
            ConfigureRelationships(modelBuilder);
            
            // 3. Configurar índices optimizados para sync
            ConfigureSyncIndexes(modelBuilder);
            
            // 4. NO incluir SeedData (se puebla via sync desde SQLite)
        }

        // ============================================
        // CONFIGURACIÓN ESPECÍFICA PARA POSTGRESQL
        // ============================================
        private static void ConfigurePostgreSQLSpecifics(ModelBuilder modelBuilder)
        {
            // ============================================
            // 1. GUIDS COMO UUID
            // ============================================
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
                    {
                        // Especificar explícitamente como UUID
                        property.SetColumnType("uuid");
                        
                        // Para primary keys, usar gen_random_uuid() como default
                        if (property.IsPrimaryKey())
                        {
                            property.SetDefaultValueSql("gen_random_uuid()");
                        }
                    }
                }
            }

            // ============================================
            // 2. DECIMALES CON PRECISIÓN
            // ============================================
            
            // Products
            modelBuilder.Entity<CategoryProduct>()
                .Property(e => e.UnitPrice)
                .HasColumnType("decimal(10,2)");

            // Sales
            modelBuilder.Entity<SalesTransaction>()
                .Property(e => e.TotalAmount)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<TransactionDetail>()
                .Property(e => e.UnitPrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<TransactionDetail>()
                .Property(e => e.TotalAmount)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<TransactionPayment>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(10,2)");

            // Inventory
            modelBuilder.Entity<InventoryMovement>()
                .Property(e => e.UnitCost)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<InventoryMovement>()
                .Property(e => e.TotalValue)
                .HasColumnType("decimal(10,2)");

            // Memberships
            modelBuilder.Entity<Membership>()
                .Property(e => e.MonthlyPrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Membership>()
                .Property(e => e.AnnualPrice)
                .HasColumnType("decimal(10,2)");

            // Combos
            modelBuilder.Entity<SalesCombo>()
                .Property(e => e.ComboPrice)
                .HasColumnType("decimal(10,2)");

            // Closures
            modelBuilder.Entity<CashRegisterClosure>()
                .Property(e => e.TotalSalesAmount)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<CashRegisterClosure>()
                .Property(e => e.CashCalculated)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<ActivityClosure>()
                .Property(e => e.TotalSales)
                .HasColumnType("decimal(10,2)");

            // ============================================
            // 3. FECHAS Y TIEMPOS
            // ============================================
            
            // Activities - DateOnly y TimeOnly específicos
            modelBuilder.Entity<Activity>()
                .Property(e => e.StartDate)
                .HasColumnType("date");

            modelBuilder.Entity<Activity>()
                .Property(e => e.StartTime)
                .HasColumnType("time");

            modelBuilder.Entity<Activity>()
                .Property(e => e.EndDate)
                .HasColumnType("date");

            modelBuilder.Entity<Activity>()
                .Property(e => e.EndTime)
                .HasColumnType("time");

            // ============================================
            // 4. TIMESTAMPS CON TIMEZONE
            // ============================================
            
            // Timestamps automáticos para entidades principales
            var timestampEntities = new[]
            {
                typeof(Organization), typeof(User), typeof(Role),
                typeof(Activity), typeof(ActivityStatus),
                typeof(CategoryProduct), typeof(SalesTransaction),
                typeof(Membership), typeof(Subscription)
            };

            foreach (var entityType in timestampEntities)
            {
                var entity = modelBuilder.Entity(entityType);
                
                // CreatedAt con default NOW()
                entity.Property("CreatedAt")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW()");

                // UpdatedAt nullable
                entity.Property("UpdatedAt")
                    .HasColumnType("timestamp with time zone");
            }

            // ============================================
            // 5. CONFIGURACIONES ESPECÍFICAS POR ENTIDAD
            // ============================================

            // SystemConfiguration - JSON para AllowedValues
            modelBuilder.Entity<SystemConfiguration>()
                .Property(e => e.AllowedValues)
                .HasColumnType("jsonb");

            // Activation Keys - timestamps específicos
            modelBuilder.Entity<ActivationKey>()
                .Property(e => e.GeneratedDate)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<ActivationKey>()
                .Property(e => e.ExpirationDate)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<ActivationKey>()
                .Property(e => e.UsedDate)
                .HasColumnType("timestamp with time zone");

            // Sales Transaction dates
            modelBuilder.Entity<SalesTransaction>()
                .Property(e => e.TransactionDate)
                .HasColumnType("timestamp with time zone");

            // Cash Register timing
            modelBuilder.Entity<CashRegister>()
                .Property(e => e.OpenedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<CashRegister>()
                .Property(e => e.ClosedAt)
                .HasColumnType("timestamp with time zone");
        }

        // ============================================
        // RELACIONES - EXACTAMENTE IGUALES QUE LocalDbContext
        // ============================================
        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Set default delete behavior to Restrict
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // User relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Activity relationships
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.ActivityStatus)
                .WithMany(s => s.Activities)
                .HasForeignKey(a => a.ActivityStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Organization)
                .WithMany(o => o.Activities)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sales relationships
            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.SalesStatus)
                .WithMany(ss => ss.SalesTransactions)
                .HasForeignKey(st => st.SalesStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesTransaction>()
                .HasOne(st => st.CashRegister)
                .WithMany(cr => cr.SalesTransactions)
                .HasForeignKey(st => st.CashRegisterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Activity Category relationships (tabla pivote)
            modelBuilder.Entity<ActivityCategory>()
                .HasOne(ac => ac.Activity)
                .WithMany(a => a.ActivityCategories)
                .HasForeignKey(ac => ac.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityCategory>()
                .HasOne(ac => ac.ServiceCategory)
                .WithMany(sc => sc.ActivityCategories)
                .HasForeignKey(ac => ac.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category Product relationships
            modelBuilder.Entity<CategoryProduct>()
                .HasOne(cp => cp.ActivityCategory)
                .WithMany(ac => ac.CategoryProducts)
                .HasForeignKey(cp => cp.ActivityCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Inventory relationships
            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.Product)
                .WithMany(p => p.InventoryMovements)
                .HasForeignKey(im => im.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.MovementType)
                .WithMany(mt => mt.InventoryMovements)
                .HasForeignKey(im => im.MovementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction Detail relationships
            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.SalesTransaction)
                .WithMany(st => st.TransactionDetails)
                .HasForeignKey(td => td.SalesTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.Product)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(td => td.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Subscription relationships
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Organization)
                .WithMany(o => o.Subscriptions)
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Membership)
                .WithMany(m => m.Subscriptions)
                .HasForeignKey(s => s.MembershipId)
                .OnDelete(DeleteBehavior.Restrict);

            // Activation Key relationships
            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.Subscription)
                .WithMany(s => s.ActivationKeys)
                .HasForeignKey(ak => ak.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // ============================================
        // ÍNDICES OPTIMIZADOS PARA SINCRONIZACIÓN
        // ============================================
        private static void ConfigureSyncIndexes(ModelBuilder modelBuilder)
        {
            // ============================================
            // ÍNDICES CRÍTICOS PARA SYNC
            // ============================================
            
            // Sync version tracking - FUNDAMENTAL para sync eficiente
            modelBuilder.Entity<Organization>()
                .HasIndex(e => new { e.SyncVersion, e.LastSync })
                .HasDatabaseName("idx_organizations_sync_tracking");

            modelBuilder.Entity<Activity>()
                .HasIndex(e => new { e.SyncVersion, e.LastSync })
                .HasDatabaseName("idx_activities_sync_tracking");

            modelBuilder.Entity<User>()
                .HasIndex(e => new { e.SyncVersion, e.LastSync })
                .HasDatabaseName("idx_users_sync_tracking");

            modelBuilder.Entity<CategoryProduct>()
                .HasIndex(e => new { e.SyncVersion, e.LastSync })
                .HasDatabaseName("idx_products_sync_tracking");

            modelBuilder.Entity<SalesTransaction>()
                .HasIndex(e => new { e.SyncVersion, e.LastSync })
                .HasDatabaseName("idx_sales_sync_tracking");

            // ============================================
            // ÍNDICES PARA CONSULTAS FRECUENTES
            // ============================================
            
            // Autenticación y usuarios
            modelBuilder.Entity<User>()
                .HasIndex(e => new { e.Username, e.Email })
                .HasDatabaseName("idx_users_credentials");

            modelBuilder.Entity<User>()
                .HasIndex(e => new { e.OrganizationId, e.Active })
                .HasDatabaseName("idx_users_org_active");

            // Actividades por organización y estado
            modelBuilder.Entity<Activity>()
                .HasIndex(e => new { e.OrganizationId, e.ActivityStatusId })
                .HasDatabaseName("idx_activities_org_status");

            modelBuilder.Entity<Activity>()
                .HasIndex(e => new { e.StartDate, e.EndDate })
                .HasDatabaseName("idx_activities_date_range");

            // Productos por categoría y stock
            modelBuilder.Entity<CategoryProduct>()
                .HasIndex(e => new { e.ActivityCategoryId, e.Active })
                .HasDatabaseName("idx_products_category_active");

            modelBuilder.Entity<CategoryProduct>()
                .HasIndex(e => new { e.CurrentQuantity, e.AlertQuantity, e.Active })
                .HasDatabaseName("idx_products_stock_alert");

            // Ventas por fecha y estado
            modelBuilder.Entity<SalesTransaction>()
                .HasIndex(e => new { e.TransactionDate, e.SalesStatusId })
                .HasDatabaseName("idx_sales_date_status");

            modelBuilder.Entity<SalesTransaction>()
                .HasIndex(e => new { e.CashRegisterId, e.TransactionDate })
                .HasDatabaseName("idx_sales_register_date");

            // Movimientos de inventario
            modelBuilder.Entity<InventoryMovement>()
                .HasIndex(e => new { e.ProductId, e.MovementDate })
                .HasDatabaseName("idx_inventory_product_date");

            modelBuilder.Entity<InventoryMovement>()
                .HasIndex(e => new { e.MovementTypeId, e.MovementDate })
                .HasDatabaseName("idx_inventory_type_date");

            // ============================================
            // ÍNDICES PARA REPORTES Y ANALYTICS
            // ============================================
            
            // Ventas por período
            modelBuilder.Entity<SalesTransaction>()
                .HasIndex(e => e.TransactionDate)
                .HasDatabaseName("idx_sales_date_reporting");

            // Actividades por fecha
            modelBuilder.Entity<Activity>()
                .HasIndex(e => e.StartDate)
                .HasDatabaseName("idx_activities_start_date");

            // Productos por precio (para reportes de inventario)
            modelBuilder.Entity<CategoryProduct>()
                .HasIndex(e => e.UnitPrice)
                .HasDatabaseName("idx_products_price");

            // ============================================
            // ÍNDICES ÚNICOS PARA INTEGRIDAD
            // ============================================
            
            // Username y email únicos
            modelBuilder.Entity<User>()
                .HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("idx_users_username_unique");

            modelBuilder.Entity<User>()
                .HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("idx_users_email_unique");

            // Códigos de activación únicos
            modelBuilder.Entity<ActivationKey>()
                .HasIndex(e => e.ActivationCode)
                .IsUnique()
                .HasDatabaseName("idx_activation_code_unique");

            // Nombres de organización únicos
            modelBuilder.Entity<Organization>()
                .HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("idx_organizations_name_unique");
        }

        // ============================================
        // MÉTODOS HELPER PARA SYNC
        // ============================================

        /// <summary>
        /// Obtiene entidades que necesitan sincronización (modificadas desde la última sync)
        /// </summary>
        public IQueryable<T> GetPendingSyncEntities<T>() where T : class
        {
            var dbSet = Set<T>();
            
            // Si la entidad tiene SyncVersion y LastSync, filtrar por pendientes
            var entityType = typeof(T);
            if (entityType.GetProperty("SyncVersion") != null && entityType.GetProperty("LastSync") != null)
            {
                return dbSet.Where(e => 
                    EF.Property<DateTime?>(e, "LastSync") == null || 
                    EF.Property<DateTime?>(e, "UpdatedAt") > EF.Property<DateTime?>(e, "LastSync"));
            }
            
            return dbSet;
        }

        /// <summary>
        /// Marca entidades como sincronizadas
        /// </summary>
        public async Task MarkAsSyncedAsync<T>(IEnumerable<T> entities) where T : class
        {
            var now = DateTime.UtcNow;
            
            foreach (var entity in entities)
            {
                var entityType = typeof(T);
                var lastSyncProperty = entityType.GetProperty("LastSync");
                if (lastSyncProperty != null)
                {
                    lastSyncProperty.SetValue(entity, now);
                }
            }
            
            await SaveChangesAsync();
        }

        /// <summary>
        /// Optimiza la base de datos ejecutando VACUUM y ANALYZE
        /// </summary>
        public async Task OptimizeDatabaseAsync()
        {
            try
            {
                // PostgreSQL optimization
                await Database.ExecuteSqlRawAsync("VACUUM ANALYZE;");
                await Database.ExecuteSqlRawAsync("REINDEX DATABASE CONCURRENTLY;");
                Console.WriteLine("✅ PostgreSQL database optimized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Database optimization warning: {ex.Message}");
            }
        }
    }
}