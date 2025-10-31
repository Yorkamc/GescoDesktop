using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gesco.Desktop.Data.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Gesco.Desktop.Data.Entities
{
    [Table("category_products")]
    public class CategoryProduct : SyncableEntityLong
    {
        // ✅ CAMBIO: Ahora es nullable
        [Column("activity_category_id")]
        public long? ActivityCategoryId { get; set; }

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
        public virtual ActivityCategory? ActivityCategory { get; set; }

        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
        public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
        public virtual ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }

    [Table("inventory_movements")]
    public class InventoryMovement : SyncableEntityLong
    {
        [Column("product_id")]
        public long ProductId { get; set; }

        [Column("movement_type_id")]
        public long MovementTypeId { get; set; }

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
        public long? SalesTransactionId { get; set; }

        [Column("justification")]
        public string? Justification { get; set; }

        [Column("performed_by")]
        [MaxLength(50)]
        public string? PerformedBy { get; set; }

        [Column("authorized_by")]
        [MaxLength(50)]
        public string? AuthorizedBy { get; set; }

        [Column("movement_date")]
        public DateTime MovementDate { get; set; }

        // Navegación
        [ForeignKey("ProductId")]
        public virtual CategoryProduct Product { get; set; } = null!;

        [ForeignKey("MovementTypeId")]
        public virtual InventoryMovementType MovementType { get; set; } = null!;

        [ForeignKey("SalesTransactionId")]
        public virtual SalesTransaction? SalesTransaction { get; set; }

        [ForeignKey("PerformedBy")]
        public virtual User? PerformedByUser { get; set; }

        [ForeignKey("AuthorizedBy")]
        public virtual User? AuthorizedByUser { get; set; }
    }
}