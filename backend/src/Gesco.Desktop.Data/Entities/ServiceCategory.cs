using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
[Table("service_categories")]
    public class ServiceCategory : SyncableEntityInt
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; } // MANTIENE Guid

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
}