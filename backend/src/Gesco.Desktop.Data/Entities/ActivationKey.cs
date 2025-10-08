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
    public class ActivationKey : AuditableEntityLong
    {
        [Column("activation_code")]
        [Required]
        [MaxLength(255)]
        public string ActivationCode { get; set; } = string.Empty;

        [Column("subscription_id")]
        public long SubscriptionId { get; set; }

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
        public Guid? UsedByOrganizationId { get; set; }

        [Column("used_by_user_id")]
        [MaxLength(50)]
        public string? UsedByUserId { get; set; } // ✅ CORREGIDO: string para cédula

        [Column("activation_ip")]
        [MaxLength(45)]
        public string? ActivationIp { get; set; }

        [Column("generated_by")]
        [MaxLength(50)]
        public string? GeneratedBy { get; set; } // ✅ CORREGIDO: string para cédula

        [Column("revoked_by")]
        [MaxLength(50)]
        public string? RevokedBy { get; set; } // ✅ CORREGIDO: string para cédula

        [Column("revocation_reason")]
        public string? RevocationReason { get; set; }

        // Navegación
        [ForeignKey("SubscriptionId")]
        public virtual Subscription Subscription { get; set; } = null!;

        // ✅ CORREGIDO: Navegaciones a User usando string (cédula)
        [ForeignKey("UsedByUserId")]
        public virtual User? UsedByUser { get; set; }

        [ForeignKey("GeneratedBy")]
        public virtual User? GeneratedByUser { get; set; }

        [ForeignKey("RevokedBy")]
        public virtual User? RevokedByUser { get; set; }
    }
}