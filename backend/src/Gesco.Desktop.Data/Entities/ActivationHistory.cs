using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
 [Table("activation_history")]
    public class ActivationHistory : BaseEntityInt
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("activation_key_id")]
        public int ActivationKeyId { get; set; }

        [Column("activation_date")]
        public DateTime ActivationDate { get; set; }

        [Column("subscription_start_date")]
        public DateTime SubscriptionStartDate { get; set; }

        [Column("subscription_end_date")]
        public DateTime SubscriptionEndDate { get; set; }

        [Column("activated_by_user_id")]
        public Guid? ActivatedByUserId { get; set; }

        [Column("activation_ip")]
        [MaxLength(45)]
        public string? ActivationIp { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("deactivation_date")]
        public DateTime? DeactivationDate { get; set; }

        [Column("deactivation_reason")]
        public string? DeactivationReason { get; set; }

        [Column("deactivated_by")]
        public Guid? DeactivatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("ActivationKeyId")]
        public virtual ActivationKey ActivationKey { get; set; } = null!;

        [ForeignKey("ActivatedByUserId")]
        public virtual User? ActivatedByUser { get; set; }

        [ForeignKey("DeactivatedBy")]
        public virtual User? DeactivatedByUser { get; set; }
    }
}