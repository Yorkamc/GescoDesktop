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
  // MEMBRESÍAS - CAMBIA A int
    [Table("memberships")]
    public class Membership : AuditableEntityInt
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

}