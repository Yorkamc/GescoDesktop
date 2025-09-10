using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gesco.Desktop.Data.Entities.Base;
using Microsoft.EntityFrameworkCore;

namespace Gesco.Desktop.Data.Entities
{
    // ORGANIZACIONES
    [Table("organizations")]
    public class Organization : SyncableEntityGuid
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


}
    
