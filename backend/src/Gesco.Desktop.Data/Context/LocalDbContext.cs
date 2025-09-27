using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;
using System.Linq;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        public DbSet<Organization> Organizations { get; set; }  // Guid
        public DbSet<User> Users { get; set; }                 // String (cédula) ✅
        public DbSet<Role> Roles { get; set; }                 // Int

        // Memberships and Subscriptions - INT
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ActivationKey> ActivationKeys { get; set; }

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
        // SYNC SYSTEM - STRING Y INT IDs
        // ============================================
        
        public DbSet<DesktopClient> DesktopClients { get; set; }        // String (UUID)
        public DbSet<SyncQueueItem> SyncQueue { get; set; }             // Int
        public DbSet<SyncVersion> SyncVersions { get; set; }            // Int

        // Notifications - INT
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // OAuth - STRING
        public DbSet<OAuthAccessToken> OAuthAccessTokens { get; set; }
        public DbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; }

        // Auditing - INT
        public DbSet<ApiActivityLog> ApiActivityLogs { get; set; }
        public DbSet<ActivationHistory> ActivationHistories { get; set; }

        // ============================================
        // ALIAS PARA COMPATIBILIDAD CON NOMBRES EN ESPAÑOL
        // ============================================
        
        public DbSet<Organization> Organizaciones => Organizations;
        public DbSet<User> Usuarios => Users;
        public DbSet<Activity> Actividades => Activities;
        public DbSet<SalesTransaction> TransaccionesVenta => SalesTransactions;
        public DbSet<CategoryProduct> ProductosCategorias => CategoryProducts;
        public DbSet<SystemConfiguration> ConfiguracionesSistema => SystemConfigurations;
        public DbSet<DesktopClient> ClientesEscritorio => DesktopClients;
        public DbSet<SyncQueueItem> ColaSincronizacion => SyncQueue;

        // ============================================
        // CONSTRUCTORS
        // ============================================
        public LocalDbContext() { }
        
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        // ============================================
        // DATABASE CONFIGURATION - CORREGIDO CS8604
        // ============================================
    private static void ConfigureRelationships(ModelBuilder modelBuilder)
{
    // Set default delete behavior to Restrict
    foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
    {
        relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }

    // ============================================
    // USER RELATIONSHIPS - STRING (CÉDULA) COMO PK
    // ============================================

    // User (string/cédula) -> Organization (Guid)
    modelBuilder.Entity<User>()
        .HasOne(u => u.Organization)
        .WithMany(o => o.Users)
        .HasForeignKey(u => u.OrganizationId) // string -> Guid ✅
        .OnDelete(DeleteBehavior.Restrict);

    // User (string/cédula) -> Role (int)
    modelBuilder.Entity<User>()
        .HasOne(u => u.Role)
        .WithMany(r => r.Users)
        .HasForeignKey(u => u.RoleId) // string -> int ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ============================================
    // ACTIVITY RELATIONSHIPS - MIXED CON CÉDULAS
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

    // ✅ CORREGIDO: Activity (int) -> User (string/cédula) para Manager
    modelBuilder.Entity<Activity>()
        .HasOne(a => a.ManagerUser)
        .WithMany()
        .HasForeignKey(a => a.ManagerUserId) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // CASH REGISTER RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // CashRegister (int) -> Activity (int)
    modelBuilder.Entity<CashRegister>()
        .HasOne(cr => cr.Activity)
        .WithMany(a => a.CashRegisters)
        .HasForeignKey(cr => cr.ActivityId) // int -> int ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: CashRegister -> Users (cédulas)
    modelBuilder.Entity<CashRegister>()
        .HasOne(cr => cr.OperatorUser)
        .WithMany()
        .HasForeignKey(cr => cr.OperatorUserId) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<CashRegister>()
        .HasOne(cr => cr.SupervisorUser)
        .WithMany()
        .HasForeignKey(cr => cr.SupervisorUserId) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // CASH REGISTER CLOSURE RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // CashRegisterClosure (int) -> CashRegister (int)
    modelBuilder.Entity<CashRegisterClosure>()
        .HasOne(crc => crc.CashRegister)
        .WithMany(cr => cr.CashRegisterClosures)
        .HasForeignKey(crc => crc.CashRegisterId) // int -> int ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: CashRegisterClosure -> Users (cédulas)
    modelBuilder.Entity<CashRegisterClosure>()
        .HasOne(crc => crc.ClosedByUser)
        .WithMany()
        .HasForeignKey(crc => crc.ClosedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<CashRegisterClosure>()
        .HasOne(crc => crc.SupervisedByUser)
        .WithMany()
        .HasForeignKey(crc => crc.SupervisedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // ACTIVITY CLOSURE RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // ActivityClosure (int) -> Activity (int)
    modelBuilder.Entity<ActivityClosure>()
        .HasOne(ac => ac.Activity)
        .WithMany()
        .HasForeignKey(ac => ac.ActivityId) // int -> int ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: ActivityClosure -> Users (cédulas)
    modelBuilder.Entity<ActivityClosure>()
        .HasOne(ac => ac.ClosedByUser)
        .WithMany()
        .HasForeignKey(ac => ac.ClosedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<ActivityClosure>()
        .HasOne(ac => ac.SupervisedByUser)
        .WithMany()
        .HasForeignKey(ac => ac.SupervisedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // TRANSACTION PAYMENT RELATIONSHIPS - CON CÉDULAS
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

    // ✅ CORREGIDO: TransactionPayment -> User (cédula)
    modelBuilder.Entity<TransactionPayment>()
        .HasOne(tp => tp.ProcessedByUser)
        .WithMany()
        .HasForeignKey(tp => tp.ProcessedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ============================================
    // INVENTORY MOVEMENT RELATIONSHIPS - CON CÉDULAS
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

    // ✅ CORREGIDO: InventoryMovement -> Users (cédulas)
    modelBuilder.Entity<InventoryMovement>()
        .HasOne(im => im.PerformedByUser)
        .WithMany()
        .HasForeignKey(im => im.PerformedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<InventoryMovement>()
        .HasOne(im => im.AuthorizedByUser)
        .WithMany()
        .HasForeignKey(im => im.AuthorizedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // SYNC SYSTEM RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // DesktopClient (string) -> Organization (Guid)
    modelBuilder.Entity<DesktopClient>()
        .HasOne(dc => dc.Organization)
        .WithMany()
        .HasForeignKey(dc => dc.OrganizationId)
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: DesktopClient (string) -> User (string/cédula)
    modelBuilder.Entity<DesktopClient>()
        .HasOne(dc => dc.User)
        .WithMany()
        .HasForeignKey(dc => dc.UserId) // string -> string ✅
        .OnDelete(DeleteBehavior.Restrict);

    // SyncQueueItem (int) -> Organization (Guid)
    modelBuilder.Entity<SyncQueueItem>()
        .HasOne(sq => sq.Organization)
        .WithMany()
        .HasForeignKey(sq => sq.OrganizationId)
        .OnDelete(DeleteBehavior.Restrict);

    // SyncQueueItem (int) -> DesktopClient (string)
    modelBuilder.Entity<SyncQueueItem>()
        .HasOne(sq => sq.DesktopClient)
        .WithMany(dc => dc.SyncQueueItems)
        .HasForeignKey(sq => sq.ClientId) // int -> string ✅
        .OnDelete(DeleteBehavior.Restrict);

    // SyncVersion (int) -> Organization (Guid)
    modelBuilder.Entity<SyncVersion>()
        .HasOne(sv => sv.Organization)
        .WithMany()
        .HasForeignKey(sv => sv.OrganizationId)
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: SyncVersion (int) -> User (string/cédula)
    modelBuilder.Entity<SyncVersion>()
        .HasOne(sv => sv.ChangedByUserNavigation)
        .WithMany()
        .HasForeignKey(sv => sv.ChangedByUser) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // SyncVersion (int) -> DesktopClient (string)
    modelBuilder.Entity<SyncVersion>()
        .HasOne(sv => sv.OriginClient)
        .WithMany()
        .HasForeignKey(sv => sv.OriginClientId)
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // NOTIFICATION RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // Notification (int) -> Organization (Guid)
    modelBuilder.Entity<Notification>()
        .HasOne(n => n.Organization)
        .WithMany()
        .HasForeignKey(n => n.OrganizationId)
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: Notification (int) -> User (string/cédula)
    modelBuilder.Entity<Notification>()
        .HasOne(n => n.User)
        .WithMany()
        .HasForeignKey(n => n.UserId) // int -> string ✅
        .OnDelete(DeleteBehavior.Cascade);

    // Notification (int) -> NotificationType (int)
    modelBuilder.Entity<Notification>()
        .HasOne(n => n.NotificationType)
        .WithMany(nt => nt.Notifications)
        .HasForeignKey(n => n.NotificationTypeId)
        .OnDelete(DeleteBehavior.Restrict);

    // ============================================
    // OAUTH RELATIONSHIPS - CON CÉDULAS
    // ============================================

    modelBuilder.Entity<OAuthAccessToken>()
    .HasOne(oat => oat.User)
    .WithMany()
    .HasForeignKey(oat => oat.UserId) // string -> string ✅ AHORA ES COMPATIBLE
    .OnDelete(DeleteBehavior.SetNull);

// OAuthRefreshToken (string) -> OAuthAccessToken (string)
modelBuilder.Entity<OAuthRefreshToken>()
    .HasOne(ort => ort.AccessToken)
    .WithMany()
    .HasForeignKey(ort => ort.AccessTokenId)
    .OnDelete(DeleteBehavior.Restrict);
    // ============================================
    // API ACTIVITY LOG RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // ✅ CORREGIDO: ApiActivityLog (int) -> User (string/cédula)
    modelBuilder.Entity<ApiActivityLog>()
        .HasOne(aal => aal.User)
        .WithMany()
        .HasForeignKey(aal => aal.UserId) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ApiActivityLog (int) -> Organization (Guid)
    modelBuilder.Entity<ApiActivityLog>()
        .HasOne(aal => aal.Organization)
        .WithMany()
        .HasForeignKey(aal => aal.OrganizationId)
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // ACTIVATION HISTORY RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // ActivationHistory (int) -> Organization (Guid)
    modelBuilder.Entity<ActivationHistory>()
        .HasOne(ah => ah.Organization)
        .WithMany()
        .HasForeignKey(ah => ah.OrganizationId)
        .OnDelete(DeleteBehavior.Restrict);

    // ActivationHistory (int) -> ActivationKey (int)
    modelBuilder.Entity<ActivationHistory>()
        .HasOne(ah => ah.ActivationKey)
        .WithMany()
        .HasForeignKey(ah => ah.ActivationKeyId)
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: ActivationHistory (int) -> User (string/cédula) - ActivatedBy
    modelBuilder.Entity<ActivationHistory>()
        .HasOne(ah => ah.ActivatedByUser)
        .WithMany()
        .HasForeignKey(ah => ah.ActivatedByUserId) // int -> string ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: ActivationHistory (int) -> User (string/cédula) - DeactivatedBy
    modelBuilder.Entity<ActivationHistory>()
        .HasOne(ah => ah.DeactivatedByUser)
        .WithMany()
        .HasForeignKey(ah => ah.DeactivatedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // ACTIVATION KEY RELATIONSHIPS - CON CÉDULAS
    // ============================================

    // ActivationKey (int) -> Subscription (int)
    modelBuilder.Entity<ActivationKey>()
        .HasOne(ak => ak.Subscription)
        .WithMany(s => s.ActivationKeys)
        .HasForeignKey(ak => ak.SubscriptionId) // int -> int ✅
        .OnDelete(DeleteBehavior.Restrict);

    // ✅ CORREGIDO: ActivationKey -> Users (cédulas)
    modelBuilder.Entity<ActivationKey>()
        .HasOne(ak => ak.UsedByUser)
        .WithMany()
        .HasForeignKey(ak => ak.UsedByUserId) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<ActivationKey>()
        .HasOne(ak => ak.GeneratedByUser)
        .WithMany()
        .HasForeignKey(ak => ak.GeneratedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    modelBuilder.Entity<ActivationKey>()
        .HasOne(ak => ak.RevokedByUser)
        .WithMany()
        .HasForeignKey(ak => ak.RevokedBy) // int -> string ✅
        .OnDelete(DeleteBehavior.SetNull);

    // ============================================
    // SALES RELATIONSHIPS - MANTENIDAS COMO ESTABAN
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
    // SERVICE CATEGORY RELATIONSHIPS
    // ============================================

    // ServiceCategory (int) -> Organization (Guid)
    modelBuilder.Entity<ServiceCategory>()
        .HasOne(sc => sc.Organization)
        .WithMany(o => o.ServiceCategories)
        .HasForeignKey(sc => sc.OrganizationId) // int -> Guid ✅
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
    // ÍNDICES ÚNICOS IMPORTANTES
    // ============================================
    
    // User - cédula única
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Id) // Cédula única
        .IsUnique()
        .HasDatabaseName("idx_users_cedula_unique");

    // User - username único
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Username)
        .IsUnique()
        .HasDatabaseName("idx_users_username_unique");

    // User - email único
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique()
        .HasDatabaseName("idx_users_email_unique");

    // DesktopClient - cliente único
    modelBuilder.Entity<DesktopClient>()
        .HasIndex(dc => dc.Id)
        .IsUnique();

    // SyncQueue - evitar duplicados
    modelBuilder.Entity<SyncQueueItem>()
        .HasIndex(sq => new { sq.ClientId, sq.AffectedTable, sq.RecordId, sq.SyncVersion })
        .HasDatabaseName("idx_sync_queue_unique");

    // Notification - índices para queries frecuentes
    modelBuilder.Entity<Notification>()
        .HasIndex(n => new { n.OrganizationId, n.IsRead, n.Important })
        .HasDatabaseName("idx_notifications_org_status");

    // ApiActivityLog - índices para auditoría
    modelBuilder.Entity<ApiActivityLog>()
        .HasIndex(aal => new { aal.OrganizationId, aal.CreatedAt })
        .HasDatabaseName("idx_api_logs_org_date");

    // ============================================
    // ÍNDICES PARA SINCRONIZACIÓN
    // ============================================
    
    // Users - índices de sync
    modelBuilder.Entity<User>()
        .HasIndex(u => new { u.SyncVersion, u.LastSync })
        .HasDatabaseName("idx_users_sync_tracking");

    // Activities - índices de sync
    modelBuilder.Entity<Activity>()
        .HasIndex(a => new { a.SyncVersion, a.LastSync })
        .HasDatabaseName("idx_activities_sync_tracking");

    // Organizations - índices de sync
    modelBuilder.Entity<Organization>()
        .HasIndex(o => new { o.SyncVersion, o.LastSync })
        .HasDatabaseName("idx_organizations_sync_tracking");
}
        // ============================================
        // SEED DATA METHOD - CORREGIDO CON TIPOS EXACTOS
        // ============================================
// FRAGMENTO CORREGIDO DEL MÉTODO SeedData en LocalDbContext.cs

private static void SeedData(ModelBuilder modelBuilder)
{
    // ============================================
    // IDS PARA SEED DATA - TIPOS CORRECTOS
    // ============================================

    // GUID IDs (Solo Organization)
    var orgId = Guid.NewGuid(); // Organization

    // STRING IDs (User con cédula)
    var adminUserId = "118640123"; // Cédula de administrador

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
    // ADMIN USER - STRING ID (CÉDULA) CON FKS MIXED
    // ============================================

    // BCrypt hash para "admin123"
    var adminPasswordHash = "$2a$12$LQV.K4/OOOgwdEXCfC7jC.QLwpZ9HkqhXfOr9p6mTyYFEYGHZcP/a";

    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = adminUserId, // ✅ string (Cédula)
            Username = "admin",
            Email = "admin@gesco.com",
            Password = adminPasswordHash,
            FullName = "System Administrator",
            Phone = "8888-8888",
            OrganizationId = orgId, // ✅ string -> Organization (Guid)
            RoleId = adminRoleId, // ✅ string -> Role (int)
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

    // ============================================
    // NOTIFICATION TYPES - INT IDS
    // ============================================

    modelBuilder.Entity<NotificationType>().HasData(
        new NotificationType
        {
            Id = 1,
            Code = "low_stock",
            Name = "Low Stock Alert",
            Description = "Product inventory is running low",
            Level = "warning",
            Active = true,
            CreatedAt = DateTime.UtcNow
        },
        new NotificationType
        {
            Id = 2,
            Code = "activity_reminder",
            Name = "Activity Reminder",
            Description = "Upcoming activity notification",
            Level = "info",
            Active = true,
            CreatedAt = DateTime.UtcNow
        },
        new NotificationType
        {
            Id = 3,
            Code = "system_alert",
            Name = "System Alert",
            Description = "Critical system notification",
            Level = "critical",
            Active = true,
            CreatedAt = DateTime.UtcNow
        },
        new NotificationType
        {
            Id = 4,
            Code = "sync_error",
            Name = "Sync Error",
            Description = "Data synchronization failed",
            Level = "error",
            Active = true,
            CreatedAt = DateTime.UtcNow
        }
    );

    // ============================================
    // SAMPLE ACTIVITY WITH ADMIN AS MANAGER - ✅ CON CÉDULA
    // ============================================

    modelBuilder.Entity<Activity>().HasData(
        new Activity
        {
            Id = 1, // ✅ int
            Name = "Demo Activity",
            Description = "Sample activity for demonstration",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(8)),
            Location = "Demo Location",
            ActivityStatusId = 1, // Not Started
            ManagerUserId = adminUserId, // ✅ CORREGIDO: string (cédula)
            OrganizationId = orgId, // Guid
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminUserId // ✅ CORREGIDO: string (cédula)
        }
    );
}
        // ============================================
        // MÉTODOS HELPER PARA SYNC - MEJORADOS
        // ============================================

        /// <summary>
        /// Registra un nuevo cliente desktop
        /// </summary>
        public async Task<DesktopClient> RegisterDesktopClientAsync(
            Guid organizationId, 
            string userId, 
            string clientName, 
            string? appVersion = null)
        {
            var client = new DesktopClient
            {
                OrganizationId = organizationId,
                UserId = userId,
                ClientName = clientName,
                AppVersion = appVersion,
                Status = "active",
                RegisteredAt = DateTime.UtcNow
            };

            DesktopClients.Add(client);
            await SaveChangesAsync();
            return client;
        }

        /// <summary>
        /// Encola cambios para sincronización
        /// </summary>
        public async Task<int> QueueSyncChangesAsync(
            IEnumerable<SyncQueueItem> changes)
        {
            SyncQueue.AddRange(changes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene cambios pendientes para un cliente específico
        /// </summary>
        public async Task<List<SyncQueueItem>> GetPendingChangesAsync(
            string clientId, 
            long fromSyncVersion = 0,
            int limit = 100)
        {
            return await SyncQueue
                .Where(sq => sq.ClientId == clientId 
                    && sq.Status == "pending" 
                    && sq.SyncVersion > fromSyncVersion
                    && sq.ExpiresAt > DateTime.UtcNow)
                .OrderBy(sq => sq.Priority)
                .ThenBy(sq => sq.SyncVersion)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Marca cambios como confirmados
        /// </summary>
        public async Task ConfirmSyncChangesAsync(IEnumerable<int> syncQueueIds)
        {
            var items = await SyncQueue
                .Where(sq => syncQueueIds.Contains(sq.Id))
                .ToListAsync();

            foreach (var item in items)
            {
                item.Status = "confirmed";
                item.ConfirmedAt = DateTime.UtcNow;
            }

            await SaveChangesAsync();
        }

        /// <summary>
        /// Limpia elementos expirados de la cola de sync
        /// </summary>
        public async Task<int> CleanupExpiredSyncItemsAsync()
        {
            var expiredItems = await SyncQueue
                .Where(sq => sq.ExpiresAt <= DateTime.UtcNow || 
                    (sq.Status == "error" && sq.Attempts >= sq.MaxAttempts))
                .ToListAsync();

            SyncQueue.RemoveRange(expiredItems);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Optimiza la base de datos
        /// </summary>
        public async Task OptimizeDatabaseAsync()
        {
            try
            {
                await Database.ExecuteSqlRawAsync("VACUUM;");
                await Database.ExecuteSqlRawAsync("ANALYZE;");
                Console.WriteLine("✅ SQLite database optimized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Database optimization warning: {ex.Message}");
            }
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