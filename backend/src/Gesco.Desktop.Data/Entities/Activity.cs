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
   [Table("activity_statuses")]
    public class ActivityStatus : AuditableEntityLong
    {
        [Column("name")]
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }

    [Table("activities")]
    public class Activity : SyncableEntityLong
    {
        [Column("name")]
        [Required]
        [MaxLength(255)]
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
        [MaxLength(255)]
        public string? Location { get; set; }

        [Column("activity_status_id")]
        public long ActivityStatusId { get; set; }

        [Column("manager_user_id")]
        [MaxLength(50)]
        public string? ManagerUserId { get; set; }

        [Column("organization_id")]
        public Guid? OrganizationId { get; set; }

        // Navegación
        [ForeignKey("ActivityStatusId")]
        public virtual ActivityStatus ActivityStatus { get; set; } = null!;

        [ForeignKey("OrganizationId")]
        public virtual Organization? Organization { get; set; }

        [ForeignKey("ManagerUserId")]
        public virtual User? ManagerUser { get; set; }

        public virtual ICollection<ActivityCategory> ActivityCategories { get; set; } = new List<ActivityCategory>();
        public virtual ICollection<CashRegister> CashRegisters { get; set; } = new List<CashRegister>();
        public virtual ICollection<SalesCombo> SalesCombos { get; set; } = new List<SalesCombo>();
    }

    [Table("activity_categories")]
    public class ActivityCategory : SyncableEntityLong
    {
        [Column("activity_id")]
        public long ActivityId { get; set; }

        [Column("service_category_id")]
        public long ServiceCategoryId { get; set; }

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; } = null!;

        public virtual ICollection<CategoryProduct> CategoryProducts { get; set; } = new List<CategoryProduct>();
    }
    
    [Table("activity_closures")]
    public class ActivityClosure : SyncableEntityLong
    {
        [Column("activity_id")]
        public long ActivityId { get; set; }

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
        [MaxLength(50)]
        public string? ClosedBy { get; set; }

        [Column("supervised_by")]
        [MaxLength(50)]
        public string? SupervisedBy { get; set; }

        [Column("observations")]
        public string? Observations { get; set; }

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        [ForeignKey("ClosedBy")]
        public virtual User? ClosedByUser { get; set; }

        [ForeignKey("SupervisedBy")]
        public virtual User? SupervisedByUser { get; set; }
    }

    [Table("inventory_movement_types")]
    public class InventoryMovementType : AuditableEntityLong
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
}