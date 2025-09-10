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
    public class ActivityStatus : AuditableEntityInt
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

    // ACTIVIDADES - CAMBIA A int
    [Table("activities")]
    public class Activity : SyncableEntityInt
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
        public int ActivityStatusId { get; set; } // CAMBIA a int

        [Column("manager_user_id")]
        public Guid? ManagerUserId { get; set; } // MANTIENE Guid

        [Column("organization_id")]
        public Guid? OrganizationId { get; set; } // MANTIENE Guid

        // Navegación
        [ForeignKey("ActivityStatusId")]
        public virtual ActivityStatus ActivityStatus { get; set; } = null!;

        [ForeignKey("OrganizationId")]
        public virtual Organization? Organization { get; set; }

        public virtual ICollection<ActivityCategory> ActivityCategories { get; set; } = new List<ActivityCategory>();
        public virtual ICollection<CashRegister> CashRegisters { get; set; } = new List<CashRegister>();
        public virtual ICollection<SalesCombo> SalesCombos { get; set; } = new List<SalesCombo>();
    }
    // ACTIVIDAD-CATEGORÍAS (Tabla pivote) - CAMBIA A int
    [Table("activity_categories")]
    public class ActivityCategory : SyncableEntityInt
    {
        [Column("activity_id")]
        public int ActivityId { get; set; } // CAMBIA a int

        [Column("service_category_id")]
        public int ServiceCategoryId { get; set; } // CAMBIA a int

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;

        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; } = null!;

        public virtual ICollection<CategoryProduct> CategoryProducts { get; set; } = new List<CategoryProduct>();
    }
    
    // CIERRES DE ACTIVIDAD - CAMBIA A int
    [Table("activity_closures")]
    public class ActivityClosure : SyncableEntityInt
    {
        [Column("activity_id")]
        public int ActivityId { get; set; } // CAMBIA a int

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
        public Guid? ClosedBy { get; set; } // MANTIENE Guid

        [Column("supervised_by")]
        public Guid? SupervisedBy { get; set; } // MANTIENE Guid

        [Column("observations")]
        public string? Observations { get; set; }

        // Navegación
        [ForeignKey("ActivityId")]
        public virtual Activity Activity { get; set; } = null!;
    }

    

    // TIPOS DE MOVIMIENTO DE INVENTARIO - CAMBIA A int
    [Table("inventory_movement_types")]
    public class InventoryMovementType : AuditableEntityInt
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