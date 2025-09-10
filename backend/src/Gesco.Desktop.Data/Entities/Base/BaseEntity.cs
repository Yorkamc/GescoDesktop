using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gesco.Desktop.Data.Entities.Base
{
    // Base abstracta - permite diferentes tipos de ID
    public abstract class BaseEntity
    {
        public abstract object GetId();
    }

    // Para entidades con ID Guid (Organization, User)
    public abstract class BaseEntityGuid : BaseEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        public override object GetId() => Id;
    }

    // Para entidades con ID int (todas las demás)
    public abstract class BaseEntityInt : BaseEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public override object GetId() => Id;
    }

    // Auditable con Guid
    public abstract class AuditableEntityGuid : BaseEntityGuid
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

    // Auditable con int
    public abstract class AuditableEntityInt : BaseEntityInt
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

    // Syncable con Guid (para Organization y User)
    public abstract class SyncableEntityGuid : AuditableEntityGuid
    {
        [Column("sync_version")]
        public long SyncVersion { get; set; } = 1;

        [Column("last_sync")]
        public DateTime? LastSync { get; set; }

        [Column("integrity_hash")]
        [MaxLength(64)]
        public string? IntegrityHash { get; set; }
    }

    // Syncable con int (para todas las demás entidades de negocio)
    public abstract class SyncableEntityInt : AuditableEntityInt
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