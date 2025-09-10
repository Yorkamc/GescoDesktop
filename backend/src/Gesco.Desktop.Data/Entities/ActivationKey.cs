using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
    [Table("activation_keys")]
    public class ActivationKey : AuditableEntityInt
    {
        [Column("activation_code")]
        [Required]
        [MaxLength(80)]
        public string ActivationCode { get; set; } = string.Empty;

        [Column("subscription_id")]
        public int SubscriptionId { get; set; } // CAMBIA a int

        [Column("is_generated")]
        public bool IsGenerated { get; set; } = true;

        [Column("is_used")]
        public bool IsUsed { get; set; } = false;

        [Column("is_expired")]
        public bool IsExpired { get; set; } = false;

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        [Column("generated_date")]
        public DateTime GeneratedDate { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

        [Column("used_date")]
        public DateTime? UsedDate { get; set; }

        [Column("revoked_date")]
        public DateTime? RevokedDate { get; set; }

        [Column("max_uses")]
        public int MaxUses { get; set; } = 1;

        [Column("current_uses")]
        public int CurrentUses { get; set; } = 0;

        [Column("generation_batch")]
        [MaxLength(100)]
        public string? GenerationBatch { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("used_by_organization_id")]
        public Guid? UsedByOrganizationId { get; set; } // MANTIENE Guid

        [Column("used_by_user_id")]
        public Guid? UsedByUserId { get; set; } // MANTIENE Guid

        [Column("activation_ip")]
        [MaxLength(45)]
        public string? ActivationIp { get; set; }

        [Column("generated_by")]
        public Guid? GeneratedBy { get; set; } // MANTIENE Guid

        [Column("revoked_by")]
        public Guid? RevokedBy { get; set; } // MANTIENE Guid

        [Column("revocation_reason")]
        public string? RevocationReason { get; set; }

        // Navegación
        [ForeignKey("SubscriptionId")]
        public virtual Subscription Subscription { get; set; } = null!;
    }
}