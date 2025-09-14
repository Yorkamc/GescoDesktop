using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
 [Table("desktop_clients")]
    public class DesktopClient : AuditableEntityString
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("client_name")]
        [Required]
        [MaxLength(100)]
        public string ClientName { get; set; } = string.Empty;

        [Column("app_version")]
        [MaxLength(20)]
        public string? AppVersion { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("last_sync_version")]
        public long LastSyncVersion { get; set; } = 0;

        [Column("last_connection")]
        public DateTime? LastConnection { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "active"; // active, inactive, blocked

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("sync_interval_seconds")]
        public int SyncIntervalSeconds { get; set; } = 30;

        [Column("read_only")]
        public bool ReadOnly { get; set; } = false;

        [Column("receive_notifications")]
        public bool ReceiveNotifications { get; set; } = true;

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<SyncQueueItem> SyncQueueItems { get; set; } = new List<SyncQueueItem>();
    }
}