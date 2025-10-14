using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
[Table("sync_queue")]
    public class SyncQueueItem : BaseEntityLong
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("client_id")]
        [Required]
        [MaxLength(36)]
        public string ClientId { get; set; } = string.Empty;

        [Column("affected_table")]
        [Required]
        [MaxLength(50)]
        public string AffectedTable { get; set; } = string.Empty;

        [Column("record_id")]
        [MaxLength(50)]
        public string RecordId { get; set; } = string.Empty;

        [Column("operation")]
        [Required]
        [MaxLength(10)]
        public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

        [Column("change_data")]
        [Required]
        public string ChangeData { get; set; } = string.Empty; // JSON

        [Column("sync_version")]
        public long SyncVersion { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "pending"; // pending, sent, confirmed, error

        [Column("attempts")]
        public int Attempts { get; set; } = 0;

        [Column("max_attempts")]
        public int MaxAttempts { get; set; } = 3;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("confirmed_at")]
        public DateTime? ConfirmedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        [Column("priority")]
        public int Priority { get; set; } = 2; // 1=high, 2=medium, 3=low

        [Column("batch_id")]
        [MaxLength(36)]
        public string? BatchId { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("error_code")]
        [MaxLength(50)]
        public string? ErrorCode { get; set; }

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("ClientId")]
        public virtual DesktopClient DesktopClient { get; set; } = null!;
    }
}