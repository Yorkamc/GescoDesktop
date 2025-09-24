using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
[Table("notifications")]
    public class Notification : AuditableEntityInt
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("user_id")]
        [MaxLength(50)]
        public string? UserId { get; set; } // ✅ CORREGIDO: string para cédula

        [Column("notification_type_id")]
        public int NotificationTypeId { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        [Required]
        public string Message { get; set; } = string.Empty;

        [Column("additional_data")]
        public string? AdditionalData { get; set; } // JSON

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("important")]
        public bool Important { get; set; } = false;

        [Column("scheduled_date")]
        public DateTime? ScheduledDate { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

        [Column("delivery_channels")]
        [MaxLength(100)]
        public string? DeliveryChannels { get; set; } // email,sms,push

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("NotificationTypeId")]
        public virtual NotificationType NotificationType { get; set; } = null!;
    }
}