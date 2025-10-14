using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;
using System.Linq;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ActivationKey> ActivationKeys { get; set; }
        public DbSet<ActivityStatus> ActivityStatuses { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ActivityCategory> ActivityCategories { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<SalesStatus> SalesStatuses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<SalesTransaction> SalesTransactions { get; set; }
        public DbSet<TransactionDetail> TransactionDetails { get; set; }
        public DbSet<TransactionPayment> TransactionPayments { get; set; }
        public DbSet<SalesCombo> SalesCombos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }
        public DbSet<InventoryMovementType> InventoryMovementTypes { get; set; }
        public DbSet<InventoryMovement> InventoryMovements { get; set; }
        public DbSet<CashRegisterClosure> CashRegisterClosures { get; set; }
        public DbSet<ActivityClosure> ActivityClosures { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<DesktopClient> DesktopClients { get; set; }
        public DbSet<SyncQueueItem> SyncQueue { get; set; }
        public DbSet<SyncVersion> SyncVersions { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OAuthAccessToken> OAuthAccessTokens { get; set; }
        public DbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; }
        public DbSet<ApiActivityLog> ApiActivityLogs { get; set; }
        public DbSet<ActivationHistory> ActivationHistories { get; set; }

        public DbSet<Organization> Organizaciones => Organizations;
        public DbSet<User> Usuarios => Users;
        public DbSet<Activity> Actividades => Activities;
        public DbSet<SalesTransaction> TransaccionesVenta => SalesTransactions;
        public DbSet<CategoryProduct> ProductosCategorias => CategoryProducts;
        public DbSet<SystemConfiguration> ConfiguracionesSistema => SystemConfigurations;
        public DbSet<DesktopClient> ClientesEscritorio => DesktopClients;
        public DbSet<SyncQueueItem> ColaSincronizacion => SyncQueue;

        public LocalDbContext() { }
        
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureRelationships(modelBuilder);
            ConfigureIndexes(modelBuilder);
            SeedData(modelBuilder);
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // User
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

            // Activity
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

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.ManagerUser)
                .WithMany()
                .HasForeignKey(a => a.ManagerUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // CashRegister
            modelBuilder.Entity<CashRegister>()
                .HasOne(cr => cr.Activity)
                .WithMany(a => a.CashRegisters)
                .HasForeignKey(cr => cr.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CashRegister>()
                .HasOne(cr => cr.OperatorUser)
                .WithMany()
                .HasForeignKey(cr => cr.OperatorUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CashRegister>()
                .HasOne(cr => cr.SupervisorUser)
                .WithMany()
                .HasForeignKey(cr => cr.SupervisorUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // CashRegisterClosure
            modelBuilder.Entity<CashRegisterClosure>()
                .HasOne(crc => crc.CashRegister)
                .WithMany(cr => cr.CashRegisterClosures)
                .HasForeignKey(crc => crc.CashRegisterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CashRegisterClosure>()
                .HasOne(crc => crc.ClosedByUser)
                .WithMany()
                .HasForeignKey(crc => crc.ClosedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CashRegisterClosure>()
                .HasOne(crc => crc.SupervisedByUser)
                .WithMany()
                .HasForeignKey(crc => crc.SupervisedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ActivityClosure
            modelBuilder.Entity<ActivityClosure>()
                .HasOne(ac => ac.Activity)
                .WithMany()
                .HasForeignKey(ac => ac.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivityClosure>()
                .HasOne(ac => ac.ClosedByUser)
                .WithMany()
                .HasForeignKey(ac => ac.ClosedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ActivityClosure>()
                .HasOne(ac => ac.SupervisedByUser)
                .WithMany()
                .HasForeignKey(ac => ac.SupervisedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // TransactionPayment
            modelBuilder.Entity<TransactionPayment>()
                .HasOne(tp => tp.SalesTransaction)
                .WithMany(st => st.TransactionPayments)
                .HasForeignKey(tp => tp.SalesTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionPayment>()
                .HasOne(tp => tp.PaymentMethod)
                .WithMany(pm => pm.TransactionPayments)
                .HasForeignKey(tp => tp.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionPayment>()
                .HasOne(tp => tp.ProcessedByUser)
                .WithMany()
                .HasForeignKey(tp => tp.ProcessedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryMovement
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

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.PerformedByUser)
                .WithMany()
                .HasForeignKey(im => im.PerformedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InventoryMovement>()
                .HasOne(im => im.AuthorizedByUser)
                .WithMany()
                .HasForeignKey(im => im.AuthorizedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // DesktopClient
            modelBuilder.Entity<DesktopClient>()
                .HasOne(dc => dc.Organization)
                .WithMany()
                .HasForeignKey(dc => dc.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DesktopClient>()
                .HasOne(dc => dc.User)
                .WithMany()
                .HasForeignKey(dc => dc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // SyncQueueItem
            modelBuilder.Entity<SyncQueueItem>()
                .HasOne(sq => sq.Organization)
                .WithMany()
                .HasForeignKey(sq => sq.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SyncQueueItem>()
                .HasOne(sq => sq.DesktopClient)
                .WithMany(dc => dc.SyncQueueItems)
                .HasForeignKey(sq => sq.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // SyncVersion
            modelBuilder.Entity<SyncVersion>()
                .HasOne(sv => sv.Organization)
                .WithMany()
                .HasForeignKey(sv => sv.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SyncVersion>()
                .HasOne(sv => sv.ChangedByUserNavigation)
                .WithMany()
                .HasForeignKey(sv => sv.ChangedByUser)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SyncVersion>()
                .HasOne(sv => sv.OriginClient)
                .WithMany()
                .HasForeignKey(sv => sv.OriginClientId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Organization)
                .WithMany()
                .HasForeignKey(n => n.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.NotificationType)
                .WithMany(nt => nt.Notifications)
                .HasForeignKey(n => n.NotificationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // OAuth
            modelBuilder.Entity<OAuthAccessToken>()
                .HasOne(oat => oat.User)
                .WithMany()
                .HasForeignKey(oat => oat.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OAuthRefreshToken>()
                .HasOne(ort => ort.AccessToken)
                .WithMany()
                .HasForeignKey(ort => ort.AccessTokenId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApiActivityLog
            modelBuilder.Entity<ApiActivityLog>()
                .HasOne(aal => aal.User)
                .WithMany()
                .HasForeignKey(aal => aal.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApiActivityLog>()
                .HasOne(aal => aal.Organization)
                .WithMany()
                .HasForeignKey(aal => aal.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            // ActivationHistory
            modelBuilder.Entity<ActivationHistory>()
                .HasOne(ah => ah.Organization)
                .WithMany()
                .HasForeignKey(ah => ah.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivationHistory>()
                .HasOne(ah => ah.ActivationKey)
                .WithMany()
                .HasForeignKey(ah => ah.ActivationKeyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivationHistory>()
                .HasOne(ah => ah.ActivatedByUser)
                .WithMany()
                .HasForeignKey(ah => ah.ActivatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivationHistory>()
                .HasOne(ah => ah.DeactivatedByUser)
                .WithMany()
                .HasForeignKey(ah => ah.DeactivatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ActivationKey
            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.Subscription)
                .WithMany(s => s.ActivationKeys)
                .HasForeignKey(ak => ak.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.UsedByUser)
                .WithMany()
                .HasForeignKey(ak => ak.UsedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.GeneratedByUser)
                .WithMany()
                .HasForeignKey(ak => ak.GeneratedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ActivationKey>()
                .HasOne(ak => ak.RevokedByUser)
                .WithMany()
                .HasForeignKey(ak => ak.RevokedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // SalesTransaction
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

            // ActivityCategory
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

            // CategoryProduct
            modelBuilder.Entity<CategoryProduct>()
                .HasOne(cp => cp.ActivityCategory)
                .WithMany(ac => ac.CategoryProducts)
                .HasForeignKey(cp => cp.ActivityCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // TransactionDetail
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

            // Subscription
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

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscriptionStatus)
                .WithMany(ss => ss.Subscriptions)
                .HasForeignKey(s => s.SubscriptionStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // ServiceCategory
            modelBuilder.Entity<ServiceCategory>()
                .HasOne(sc => sc.Organization)
                .WithMany(o => o.ServiceCategories)
                .HasForeignKey(sc => sc.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // SalesCombo
            modelBuilder.Entity<SalesCombo>()
                .HasOne(sc => sc.Activity)
                .WithMany(a => a.SalesCombos)
                .HasForeignKey(sc => sc.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            // ComboItem
            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Combo)
                .WithMany(sc => sc.ComboItems)
                .HasForeignKey(ci => ci.ComboId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Product)
                .WithMany(cp => cp.ComboItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Id)
                .IsUnique()
                .HasDatabaseName("idx_users_cedula_unique");

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.OrganizationId, u.Username })
                .IsUnique()
                .HasDatabaseName("idx_users_org_username_unique");
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("idx_users_email_unique");

            modelBuilder.Entity<DesktopClient>()
                .HasIndex(dc => dc.Id)
                .IsUnique();

            modelBuilder.Entity<SyncQueueItem>()
                .HasIndex(sq => new { sq.ClientId, sq.AffectedTable, sq.RecordId, sq.SyncVersion })
                .HasDatabaseName("idx_sync_queue_unique");

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.OrganizationId, n.IsRead, n.Important })
                .HasDatabaseName("idx_notifications_org_status");

            modelBuilder.Entity<ApiActivityLog>()
                .HasIndex(aal => new { aal.OrganizationId, aal.CreatedAt })
                .HasDatabaseName("idx_api_logs_org_date");

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.SyncVersion, u.LastSync })
                .HasDatabaseName("idx_users_sync_tracking");

            modelBuilder.Entity<Activity>()
                .HasIndex(a => new { a.SyncVersion, a.LastSync })
                .HasDatabaseName("idx_activities_sync_tracking");

            modelBuilder.Entity<Organization>()
                .HasIndex(o => new { o.SyncVersion, o.LastSync })
                .HasDatabaseName("idx_organizations_sync_tracking");
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var orgId = Guid.NewGuid();
            var adminUserId = "118640123";
            var adminRoleId = 1L;
            var salesRoleId = 2L;
            var supervisorRoleId = 3L;

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

            var adminPasswordHash = "$2a$12$LQV.K4/OOOgwdEXCfC7jC.QLwpZ9HkqhXfOr9p6mTyYFEYGHZcP/a";

            modelBuilder.Entity<User>().HasData(
                new User
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
                }
            );

            modelBuilder.Entity<ActivityStatus>().HasData(
                new ActivityStatus { Id = 1L, Name = "Not Started", Description = "Activity not started", Active = true, CreatedAt = DateTime.UtcNow },
                new ActivityStatus { Id = 2L, Name = "In Progress", Description = "Activity in development", Active = true, CreatedAt = DateTime.UtcNow },
                new ActivityStatus { Id = 3L, Name = "Completed", Description = "Activity completed", Active = true, CreatedAt = DateTime.UtcNow },
                new ActivityStatus { Id = 4L, Name = "Cancelled", Description = "Activity cancelled", Active = true, CreatedAt = DateTime.UtcNow }
            );

            modelBuilder.Entity<SalesStatus>().HasData(
                new SalesStatus { Id = 1L, Name = "Pending", Description = "Sale pending processing", Active = true, CreatedAt = DateTime.UtcNow },
                new SalesStatus { Id = 2L, Name = "Completed", Description = "Sale completed successfully", Active = true, CreatedAt = DateTime.UtcNow },
                new SalesStatus { Id = 3L, Name = "Cancelled", Description = "Sale cancelled", Active = true, CreatedAt = DateTime.UtcNow }
            );

            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1L, Name = "Cash", RequiresReference = false, Active = true, CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 2L, Name = "Card", RequiresReference = true, Active = true, CreatedAt = DateTime.UtcNow },
                new PaymentMethod { Id = 3L, Name = "SINPE Mobile", RequiresReference = true, Active = true, CreatedAt = DateTime.UtcNow }
            );

            modelBuilder.Entity<InventoryMovementType>().HasData(
                new InventoryMovementType { Id = 1L, Name = "Stock In", Description = "Merchandise entry to inventory", RequiresJustification = false, Active = true, CreatedAt = DateTime.UtcNow },
                new InventoryMovementType { Id = 2L, Name = "Sale", Description = "Stock out by product sale", RequiresJustification = false, Active = true, CreatedAt = DateTime.UtcNow },
                new InventoryMovementType { Id = 3L, Name = "Adjustment", Description = "Inventory adjustment for differences", RequiresJustification = true, Active = true, CreatedAt = DateTime.UtcNow }
            );

            modelBuilder.Entity<SubscriptionStatus>().HasData(
                new SubscriptionStatus { Id = 1L, Name = "Active", Description = "Active subscription", AllowsSystemUsage = true, Active = true, CreatedAt = DateTime.UtcNow },
                new SubscriptionStatus { Id = 2L, Name = "Suspended", Description = "Suspended subscription", AllowsSystemUsage = false, Active = true, CreatedAt = DateTime.UtcNow },
                new SubscriptionStatus { Id = 3L, Name = "Cancelled", Description = "Cancelled subscription", AllowsSystemUsage = false, Active = true, CreatedAt = DateTime.UtcNow }
            );

            modelBuilder.Entity<SystemConfiguration>().HasData(
                new SystemConfiguration
                {
                    Id = 1L,
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
                    Id = 2L,
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
                    Id = 3L,
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

            modelBuilder.Entity<Membership>().HasData(
                new Membership
                {
                    Id = 1L,
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
                    Id = 2L,
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
                    Id = 3L,
                    Name = "Enterprise",
                    Description = "Enterprise membership with unlimited features",
                    MonthlyPrice = 129.99m,
                    AnnualPrice = 1299.99m,
                    UserLimit = 100,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Subscription>().HasData(
                new Subscription
                {
                    Id = 1L,
                    OrganizationId = orgId,
                    MembershipId = 1L,
                    SubscriptionStatusId = 1L,
                    StartDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddMonths(12),
                    GracePeriodEnd = DateTime.UtcNow.AddMonths(12).AddDays(30),
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<NotificationType>().HasData(
                new NotificationType
                {
                    Id = 1L,
                    Code = "low_stock",
                    Name = "Low Stock Alert",
                    Description = "Product inventory is running low",
                    Level = "warning",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new NotificationType
                {
                    Id = 2L,
                    Code = "activity_reminder",
                    Name = "Activity Reminder",
                    Description = "Upcoming activity notification",
                    Level = "info",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new NotificationType
                {
                    Id = 3L,
                    Code = "system_alert",
                    Name = "System Alert",
                    Description = "Critical system notification",
                    Level = "critical",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                },
                new NotificationType
                {
                    Id = 4L,
                    Code = "sync_error",
                    Name = "Sync Error",
                    Description = "Data synchronization failed",
                    Level = "error",
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Activity>().HasData(
                new Activity
                {
                    Id = 1L,
                    Name = "Demo Activity",
                    Description = "Sample activity for demonstration",
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    StartTime = TimeOnly.FromDateTime(DateTime.Now),
                    EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(8)),
                    Location = "Demo Location",
                    ActivityStatusId = 1L,
                    ManagerUserId = adminUserId,
                    OrganizationId = orgId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUserId
                }
            );
        }

        public async Task<DesktopClient> RegisterDesktopClientAsync(Guid organizationId, string userId, string clientName, string? appVersion = null)
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

        public async Task<int> QueueSyncChangesAsync(IEnumerable<SyncQueueItem> changes)
        {
            SyncQueue.AddRange(changes);
            return await SaveChangesAsync();
        }

        public async Task<List<SyncQueueItem>> GetPendingChangesAsync(string clientId, long fromSyncVersion = 0, int limit = 100)
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

        public async Task ConfirmSyncChangesAsync(IEnumerable<long> syncQueueIds)
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

        public async Task<int> CleanupExpiredSyncItemsAsync()
        {
            var expiredItems = await SyncQueue
                .Where(sq => sq.ExpiresAt <= DateTime.UtcNow || 
                    (sq.Status == "error" && sq.Attempts >= sq.MaxAttempts))
                .ToListAsync();

            SyncQueue.RemoveRange(expiredItems);
            return await SaveChangesAsync();
        }

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

        public async Task RunOptimizationScriptAsync()
        {
            try
            {
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), 
                    "..", "..", "src", "Gesco.Desktop.Data", "script", "sqlite_optimization_script.sql");
                
                if (File.Exists(scriptPath))
                {
                    var script = await File.ReadAllTextAsync(scriptPath);
                    var statements = script.Split(new[] { ";\r\n", ";\n", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    
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