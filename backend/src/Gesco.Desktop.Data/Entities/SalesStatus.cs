using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Gesco.Desktop.Data.Entities
{
     [Table("sales_statuses")]
    public class SalesStatus : AuditableEntityLong
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        public virtual ICollection<SalesTransaction> SalesTransactions { get; set; } = new List<SalesTransaction>();
    }

    [Table("payment_methods")]
    public class PaymentMethod : AuditableEntityLong
    {
        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column("requires_reference")]
        public bool RequiresReference { get; set; } = false;

        [Column("active")]
        public bool Active { get; set; } = true;

        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();
    }

    [Table("cash_registers")]
    public class CashRegister : SyncableEntityLong
    {
        [Column("activity_id")]
        public long ActivityId { get; set; }

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
        [MaxLength(50)]
        public string? OperatorUserId { get; set; }

        [Column("supervisor_user_id")]
        [MaxLength(50)]
        public string? SupervisorUserId { get; set; }

        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        [ForeignKey("OperatorUserId")]
        public virtual User? OperatorUser { get; set; }

        [ForeignKey("SupervisorUserId")]
        public virtual User? SupervisorUser { get; set; }

        public virtual ICollection<SalesTransaction> SalesTransactions { get; set; } = new List<SalesTransaction>();
        public virtual ICollection<CashRegisterClosure> CashRegisterClosures { get; set; } = new List<CashRegisterClosure>();
    }

    [Table("sales_transactions")]
    public class SalesTransaction : SyncableEntityLong
    {
        [Column("cash_register_id")]
        public long CashRegisterId { get; set; }

        [Column("transaction_number")]
        [Required]
        [MaxLength(50)]
        public string TransactionNumber { get; set; } = string.Empty;

        [Column("invoice_number")]
        [MaxLength(50)]
        public string? InvoiceNumber { get; set; }

        [Column("sales_status_id")]
        public long SalesStatusId { get; set; }

        [Column("transaction_date")]
        public DateTime TransactionDate { get; set; }

        [Column("total_amount")]
        [Precision(10, 2)]
        public decimal TotalAmount { get; set; }

        [ForeignKey("CashRegisterId")]
        public virtual CashRegister CashRegister { get; set; } = null!;

        [ForeignKey("SalesStatusId")]
        public virtual SalesStatus SalesStatus { get; set; } = null!;

        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
        public virtual ICollection<TransactionPayment> TransactionPayments { get; set; } = new List<TransactionPayment>();
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }

    [Table("transaction_details")]
    public class TransactionDetail : SyncableEntityLong
    {
        [Column("sales_transaction_id")]
        public long SalesTransactionId { get; set; }

        [Column("product_id")]
        public long? ProductId { get; set; }

        [Column("combo_id")]
        public long? ComboId { get; set; }

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

        [ForeignKey("SalesTransactionId")]
        public virtual SalesTransaction SalesTransaction { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual CategoryProduct? Product { get; set; }

        [ForeignKey("ComboId")]
        public virtual SalesCombo? Combo { get; set; }
    }

    [Table("transaction_payments")]
    public class TransactionPayment : SyncableEntityLong
    {
        [Column("sales_transaction_id")]
        public long SalesTransactionId { get; set; }

        [Column("payment_method_id")]
        public long PaymentMethodId { get; set; }

        [Column("amount")]
        [Precision(10, 2)]
        public decimal Amount { get; set; }

        [Column("reference")]
        [MaxLength(100)]
        public string? Reference { get; set; }

        [Column("processed_at")]
        public DateTime ProcessedAt { get; set; }

        [Column("processed_by")]
        [MaxLength(50)]
        public string ProcessedBy { get; set; } = string.Empty;

        [ForeignKey("SalesTransactionId")]
        public virtual SalesTransaction SalesTransaction { get; set; } = null!;

        [ForeignKey("PaymentMethodId")]
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;

        [ForeignKey("ProcessedBy")]
        public virtual User ProcessedByUser { get; set; } = null!;
    }

    [Table("cash_register_closures")]
    public class CashRegisterClosure : SyncableEntityLong
    {
        [Column("cash_register_id")]
        public long CashRegisterId { get; set; }

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
        [MaxLength(50)]
        public string? ClosedBy { get; set; }

        [Column("supervised_by")]
        [MaxLength(50)]
        public string? SupervisedBy { get; set; }

        [Column("observations")]
        public string? Observations { get; set; }

        [ForeignKey("CashRegisterId")]
        public virtual CashRegister CashRegister { get; set; } = null!;

        [ForeignKey("ClosedBy")]
        public virtual User? ClosedByUser { get; set; }

        [ForeignKey("SupervisedBy")]
        public virtual User? SupervisedByUser { get; set; }
    }

    [Table("sales_combos")]
    public class SalesCombo : SyncableEntityLong
    {
        [Column("activity_id")]
        public long ActivityId { get; set; }

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

        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    }

    [Table("combo_items")]
    public class ComboItem : SyncableEntityLong
    {
        [Column("combo_id")]
        public long ComboId { get; set; }

        [Column("product_id")]
        public long ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [ForeignKey("ComboId")]
        public virtual SalesCombo Combo { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual CategoryProduct Product { get; set; } = null!;
    }

}