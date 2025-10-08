using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
 
    [Table("notification_types")]
    public class NotificationType : AuditableEntityLong
    {
        [Column("code")]
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // low_stock, activity_reminder, etc.

        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("level")]
        [Required]
        [MaxLength(20)]
        public string Level { get; set; } = "info"; // info, warning, error, critical

        [Column("active")]
        public bool Active { get; set; } = true;

        // Navegación
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}