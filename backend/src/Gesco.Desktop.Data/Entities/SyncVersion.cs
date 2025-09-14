using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
[Table("sync_versions")]
    public class SyncVersion : BaseEntityInt
    {
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Column("table_name")]
        [Required]
        [MaxLength(50)]
        public string TableName { get; set; } = string.Empty;

        [Column("record_id")]
        public long RecordId { get; set; }

        [Column("version")]
        public long Version { get; set; } = 1;

        [Column("operation")]
        [Required]
        [MaxLength(10)]
        public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

        [Column("change_date")]
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        [Column("change_data")]
        public string? ChangeData { get; set; }

        [Column("changed_by_user")]
        public Guid? ChangedByUser { get; set; }

        [Column("origin_client_id")]
        [MaxLength(36)]
        public string? OriginClientId { get; set; }

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("ChangedByUser")]
        public virtual User? ChangedByUser { get; set; }

        [ForeignKey("OriginClientId")]
        public virtual DesktopClient? OriginClient { get; set; }
    }

}