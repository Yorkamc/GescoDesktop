using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
  // ESTADOS DE SUSCRIPCIÓN - CAMBIA A int
    [Table("subscription_statuses")]
    public class SubscriptionStatus : AuditableEntityInt
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

    // SUSCRIPCIONES - CAMBIA A int
    [Table("subscriptions")]
    public class Subscription : SyncableEntityInt
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; } // MANTIENE Guid

        [Column("membership_id")]
        public int MembershipId { get; set; } // CAMBIA a int

        [Column("subscription_status_id")]
        public int SubscriptionStatusId { get; set; } // CAMBIA a int

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

}