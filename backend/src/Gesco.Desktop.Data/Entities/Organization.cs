using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gesco.Desktop.Data.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Gesco.Desktop.Data.Entities
{
  // ORGANIZACIONES
    [Table("organizations")]
    public class Organization : SyncableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("contact_email")]
        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [Column("contact_phone")]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("purchaser_name")]
        [MaxLength(200)]
        public string? PurchaserName { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
    }

    // ROLES
    [Table("roles")]
    public class Role : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    // USUARIOS
    [Table("users")]
    public class User : SyncableEntity
    {
        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Column("email_verified_at")]
        public DateTime? EmailVerifiedAt { get; set; }

        [Column("password")]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Column("full_name")]
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Column("phone")]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [Column("first_login")]
        public bool FirstLogin { get; set; } = true;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("first_login_at")]
        public DateTime? FirstLoginAt { get; set; }

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }

    // MEMBRESÍAS
    [Table("memberships")]
    public class Membership : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("monthly_price")]
        [Precision(10, 2)]
        public decimal MonthlyPrice { get; set; }

        [Column("annual_price")]
        [Precision(10, 2)]
        public decimal AnnualPrice { get; set; }

        [Column("user_limit")]
        public int UserLimit { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }

    // ESTADOS DE SUSCRIPCIÓN
    [Table("subscription_statuses")]
    public class SubscriptionStatus : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("allows_system_usage")]
        public bool AllowsSystemUsage { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }

    // SUSCRIPCIONES
    [Table("subscriptions")]
    public class Subscription : SyncableEntity
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("membership_id")]
        public int MembershipId { get; set; }

        [Column("subscription_status_id")]
        public int SubscriptionStatusId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("expiration_date")]
        public DateTime ExpirationDate { get; set; }

        [Column("grace_period_end")]
        public DateTime GracePeriodEnd { get; set; }

        [Column("cancellation_date")]
        public DateTime? CancellationDate { get; set; }

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("MembershipId")]
        public virtual Membership Membership { get; set; } = null!;

        [ForeignKey("SubscriptionStatusId")]
        public virtual SubscriptionStatus SubscriptionStatus { get; set; } = null!;

        public virtual ICollection<ActivationKey> ActivationKeys { get; set; } = new List<ActivationKey>();
    }

    // CLAVES DE ACTIVACIÓN
    [Table("activation_keys")]
    public class ActivationKey : AuditableEntity
    {
        [Column("activation_code")]
        [Required]
        [MaxLength(80)]
        public string ActivationCode { get; set; } = string.Empty;

        [Column("subscription_id")]
        public int SubscriptionId { get; set; }

        [Column("is_generated")]
        public bool IsGenerated { get; set; } = true;

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("is_expired")]
        public bool IsExpired { get; set; } = false;

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        [Column("generated_date")]
        public DateTime GeneratedDate { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

        [Column("used_date")]
        public DateTime? UsedDate { get; set; }

        [Column("revoked_date")]
        public DateTime? RevokedDate { get; set; }

        [Column("max_uses")]
        public int MaxUses { get; set; } = 1;

        [Column("current_uses")]
        public int CurrentUses { get; set; } = 0;

        [Column("generation_batch")]
        [MaxLength(100)]
        public string? GenerationBatch { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("used_by_organization_id")]
        public Guid? UsedByOrganizationId { get; set; }

        [Column("used_by_user_id")]
        public Guid? UsedByUserId { get; set; }

        [Column("activation_ip")]
        [MaxLength(45)]
        public string? ActivationIp { get; set; }

        [Column("generated_by")]
        public Guid? GeneratedBy { get; set; }

        [Column("revoked_by")]
        public Guid? RevokedBy { get; set; }

        [Column("revocation_reason")]
        public string? RevocationReason { get; set; }

        // Navegación
        [ForeignKey("SubscriptionId")]
        public virtual Subscription Subscription { get; set; } = null!;
    }

    // ESTADOS DE ACTIVIDAD
    [Table("activity_statuses")]
    public class ActivityStatus : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }

    // ACTIVIDADES
    [Table("activities")]
    public class Activity : SyncableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("start_date")]
        public DateOnly StartDate { get; set; }

        [Column("start_time")]
        public TimeOnly? StartTime { get; set; }

        [Column("end_date")]
        public DateOnly? EndDate { get; set; }

        [Column("end_time")]
        public TimeOnly? EndTime { get; set; }

        [Column("location")]
        [MaxLength(200)]
        public string? Location { get; set; }

        [Column("activity_status_id")]
        public int ActivityStatusId { get; set; }

        [Column("manager_user_id")]
        public Guid? ManagerUserId { get; set; }

        [Column("organization_id")]
        public Guid? OrganizationId { get; set; }

        // Navegación
        [ForeignKey("ActivityStatusId")]
        public virtual ActivityStatus ActivityStatus { get; set; } = null!;

        [ForeignKey("OrganizationId")]
        public virtual Organization? Organization { get; set; }

        public virtual ICollection<ActivityCategory> ActivityCategories { get; set; } = new List<ActivityCategory>();
        public virtual ICollection<CashRegister> CashRegisters { get; set; } = new List<CashRegister>();
        public virtual ICollection<SalesCombo> SalesCombos { get; set; } = new List<SalesCombo>();
    }

    // CATEGORÍAS DE SERVICIO
    [Table("service_categories")]
    public class ServiceCategory : SyncableEntity
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        public virtual ICollection<ActivityCategory> ActivityCategories { get; set; } = new List<ActivityCategory>();
    }

    // ACTIVIDAD-CATEGORÍAS (Tabla pivote)
    [Table("activity_categories")]
    public class ActivityCategory : SyncableEntity
    {
        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Column("service_category_id")]
        public int ServiceCategoryId { get; set; }

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; } = null!;

        public virtual ICollection<CategoryProduct> CategoryProducts { get; set; } = new List<CategoryProduct>();
    }

    // PRODUCTOS DE CATEGORÍA
    [Table("category_products")]
    public class CategoryProduct : SyncableEntity
    {
        [Column("activity_category_id")]
        public int ActivityCategoryId { get; set; }

        [Column("code")]
        [MaxLength(50)]
        public string? Code { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("unit_price")]
        [Precision(10, 2)]
        public decimal UnitPrice { get; set; }

        [Column("initial_quantity")]
        public int InitialQuantity { get; set; } = 0;

        [Column("current_quantity")]
        public int CurrentQuantity { get; set; } = 0;

        [Column("alert_quantity")]
        public int AlertQuantity { get; set; } = 10;

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        [ForeignKey("ActivityCategoryId")]
        public virtual ActivityCategory ActivityCategory { get; set; } = null!;

        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
        public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }

    // ESTADOS DE VENTA
    [Table("sales_statuses")]
    public class SalesStatus : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<SalesTransaction> SalesTransactions { get; set; } = new List<SalesTransaction>();
    }

    // MÉTODOS DE PAGO
    [Table("payment_methods")]
    public class PaymentMethod : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("requires_reference")]
        public bool RequiresReference { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();
    }

    // CAJAS REGISTRADORAS
    [Table("cash_registers")]
    public class CashRegister : SyncableEntity
    {
        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Column("register_number")]
        public int RegisterNumber { get; set; }

        [Column("name")]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Column("location")]
        [MaxLength(200)]
        public string? Location { get; set; }

        [Column("is_open")]
        public bool IsOpen { get; set; } = false;

        [Column("opened_at")]
        public DateTime? OpenedAt { get; set; }

        [Column("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [Column("operator_user_id")]
        public Guid? OperatorUserId { get; set; }

        [Column("supervisor_user_id")]
        public Guid? SupervisorUserId { get; set; }

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        public virtual ICollection<SalesTransaction> SalesTransactions { get; set; } = new List<SalesTransaction>();
        public virtual ICollection<CashRegisterClosure> CashRegisterClosures { get; set; } = new List<CashRegisterClosure>();
    }

    // TRANSACCIONES DE VENTA
    [Table("sales_transactions")]
    public class SalesTransaction : SyncableEntity
    {
        [Column("cash_register_id")]
        public int CashRegisterId { get; set; }

        [Column("transaction_number")]
        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [Column("invoice_number")]
        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        [Column("sales_status_id")]
        public int SalesStatusId { get; set; }

        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; }

        [Column("total_amount")]
        [Precision(10, 2)]
        public decimal TotalAmount { get; set; }

        // Navegación
        [ForeignKey("CashRegisterId")]
        public virtual CashRegister CashRegister { get; set; } = null!;

        [ForeignKey("SalesStatusId")]
        public virtual SalesStatus SalesStatus { get; set; } = null!;

        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }

    // DETALLES DE TRANSACCIÓN
    [Table("transaction_details")]
    public class TransactionDetail : SyncableEntity
    {
        [Column("sales_transaction_id")]
        public int SalesTransactionId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [Column("combo_id")]
        public int? ComboId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        [Precision(10, 2)]
        public decimal UnitPrice { get; set; }

        [Column("total_amount")]
        [Precision(10, 2)]
        public decimal TotalAmount { get; set; }

        [Column("is_combo")]
        public bool IsCombo { get; set; } = false;

        // Navegación
        [ForeignKey("SalesTransactionId")]
        public virtual SalesTransaction SalesTransaction { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual CategoryProduct? Product { get; set; }

        [ForeignKey("ComboId")]
        public virtual SalesCombo? Combo { get; set; }
    }

    // PAGOS DE TRANSACCIÓN
    [Table("transaction_payments")]
    public class TransactionPayment : SyncableEntity
    {
        [Column("sales_transaction_id")]
        public int SalesTransactionId { get; set; }

        [Column("payment_method_id")]
        public int PaymentMethodId { get; set; }

        [Column("amount")]
        [Precision(10, 2)]
        public decimal Amount { get; set; }

        [Column("reference")]
        [MaxLength(100)]
        public string? Reference { get; set; }

        [Column("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [Column("processed_by")]
        public Guid ProcessedBy { get; set; }

        // Navegación
        [ForeignKey("SalesTransactionId")]
        public virtual SalesTransaction SalesTransaction { get; set; } = null!;

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    }

    // COMBOS DE VENTA
    [Table("sales_combos")]
    public class SalesCombo : SyncableEntity
    {
        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("combo_price")]
        [Precision(10, 2)]
        public decimal ComboPrice { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    }

    // ITEMS DE COMBO
    [Table("combo_items")]
    public class ComboItem : SyncableEntity
    {
        [Column("combo_id")]
        public int ComboId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        // Navegación
        [ForeignKey("ComboId")]
        public virtual SalesCombo Combo { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual CategoryProduct Product { get; set; } = null!;
    }

    // TIPOS DE MOVIMIENTO DE INVENTARIO
    [Table("inventory_movement_types")]
    public class InventoryMovementType : AuditableEntity
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("requires_justification")]
        public bool RequiresJustification { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }

    // MOVIMIENTOS DE INVENTARIO
    [Table("inventory_movements")]
    public class InventoryMovement : SyncableEntity
    {
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("movement_type_id")]
        public int MovementTypeId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("previous_quantity")]
        public int PreviousQuantity { get; set; }

        [Column("new_quantity")]
        public int NewQuantity { get; set; }

        [Column("unit_cost")]
        [Precision(10, 2)]
        public decimal? UnitCost { get; set; }

        [Column("total_value")]
        [Precision(10, 2)]
        public decimal? TotalValue { get; set; }

        [Column("sales_transaction_id")]
        public int? SalesTransactionId { get; set; }

        [Column("justification")]
        public string? Justification { get; set; }

        [Column("performed_by")]
        public Guid? PerformedBy { get; set; }

        [Column("authorized_by")]
        public Guid? AuthorizedBy { get; set; }

        [Column("movement_date")]
        public DateTime MovementDate { get; set; }

        // Navegación
        [ForeignKey("ProductId")]
        public virtual CategoryProduct Product { get; set; } = null!;

        [ForeignKey("MovementTypeId")]
        public virtual InventoryMovementType MovementType { get; set; } = null!;

        [ForeignKey("SalesTransactionId")]
        public virtual SalesTransaction? SalesTransaction { get; set; }
    }

    // CIERRES DE CAJA
    [Table("cash_register_closures")]
    public class CashRegisterClosure : SyncableEntity
    {
        [Column("cash_register_id")]
        public int CashRegisterId { get; set; }

        [Column("opening_date")]
        public DateTime OpeningDate { get; set; }

        [Column("closing_date")]
        public DateTime ClosingDate { get; set; }

        [Column("total_transactions")]
        public int TotalTransactions { get; set; }

        [Column("total_items_sold")]
        public int TotalItemsSold { get; set; }

        [Column("total_sales_amount")]
        [Precision(10, 2)]
        public decimal TotalSalesAmount { get; set; }

        [Column("cash_calculated")]
        [Precision(10, 2)]
        public decimal CashCalculated { get; set; }

        [Column("cards_calculated")]
        [Precision(10, 2)]
        public decimal CardsCalculated { get; set; }

        [Column("sinpe_calculated")]
        [Precision(10, 2)]
        public decimal SinpeCalculated { get; set; }

        [Column("cash_declared")]
        [Precision(10, 2)]
        public decimal? CashDeclared { get; set; }

        [Column("cash_difference")]
        [Precision(10, 2)]
        public decimal? CashDifference { get; set; }

        [Column("closed_by")]
        public Guid? ClosedBy { get; set; }

        [Column("supervised_by")]
        public Guid? SupervisedBy { get; set; }

        [Column("observations")]
        public string? Observations { get; set; }

        // Navegación
        [ForeignKey("CashRegisterId")]
        public virtual CashRegister CashRegister { get; set; } = null!;
    }

    // CIERRES DE ACTIVIDAD
    [Table("activity_closures")]
    public class ActivityClosure : SyncableEntity
    {
        [Column("activity_id")]
        public int ActivityId { get; set; }

        [Column("closure_date")]
        public DateTime ClosureDate { get; set; }

        [Column("duration_hours")]
        [Precision(5, 2)]
        public decimal DurationHours { get; set; }

        [Column("total_registers")]
        public int TotalRegisters { get; set; }

        [Column("registers_with_differences")]
        public int RegistersWithDifferences { get; set; }

        [Column("total_sales")]
        [Precision(10, 2)]
        public decimal TotalSales { get; set; }

        [Column("total_transactions")]
        public int TotalTransactions { get; set; }

        [Column("total_items_sold")]
        public int TotalItemsSold { get; set; }

        [Column("out_of_stock_items")]
        public int OutOfStockItems { get; set; }

        [Column("items_with_stock")]
        public int ItemsWithStock { get; set; }

        [Column("total_remaining_units")]
        public int TotalRemainingUnits { get; set; }

        [Column("final_inventory_value")]
        [Precision(10, 2)]
        public decimal FinalInventoryValue { get; set; }

        [Column("shrinkage_value")]
        [Precision(10, 2)]
        public decimal ShrinkageValue { get; set; }

        [Column("closed_by")]
        public Guid? ClosedBy { get; set; }

        [Column("supervised_by")]
        public Guid? SupervisedBy { get; set; }

        [Column("observations")]
        public string? Observations { get; set; }

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;
    }
}