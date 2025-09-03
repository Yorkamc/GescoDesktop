using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gesco.Desktop.Data.Entities.Base
{
    public abstract class BaseEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public abstract class AuditableEntity : BaseEntity
    {
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public abstract class SyncableEntity : AuditableEntity
    {
        [Column("sync_version")]
        public long SyncVersion { get; set; } = 1;

        [Column("last_sync")]
        public DateTime? LastSync { get; set; }

        [Column("integrity_hash")]
        [MaxLength(64)]
        public string? IntegrityHash { get; set; }
    }
}
