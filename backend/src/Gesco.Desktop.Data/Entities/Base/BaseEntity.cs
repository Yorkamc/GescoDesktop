using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gesco.Desktop.Data.Entities.Base
{
      
    public abstract class BaseEntity
    {
        public abstract object GetId();
    }

    // ============================================
    // ENTIDADES BASE POR TIPO DE ID
    // ============================================

    /// <summary>
    /// Para entidades con ID Guid (Organization)
    /// </summary>
    public abstract class BaseEntityGuid : BaseEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        public override object GetId() => Id;
    }

    /// <summary>
    /// Para entidades con ID int (mayoría de entidades de negocio)
    /// </summary>
    public abstract class BaseEntityInt : BaseEntity
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public override object GetId() => Id;
    }

    /// <summary>
    /// Para entidades con ID string (User con cédula, DesktopClient, etc.)
    /// </summary>
    public abstract class BaseEntityString : BaseEntity
    {
        [Key]
        [Column("id")]
        [MaxLength(50)] // Suficiente para cédulas, UUIDs como string, etc.
        public virtual string Id { get; set; } = string.Empty;

        public override object GetId() => Id;
    }

    // ============================================
    // ENTIDADES AUDITABLES
    // ============================================

    /// <summary>
    /// Auditable con Guid ID (Organization)
    /// </summary>
    public abstract class AuditableEntityGuid : BaseEntityGuid
    {
        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; } // ✅ Cédula del creador

        [Column("updated_by")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; } // ✅ Cédula del actualizador

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Auditable con int ID (mayoría de entidades)
    /// </summary>
    public abstract class AuditableEntityInt : BaseEntityInt
    {
        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; } // ✅ Cédula del creador

        [Column("updated_by")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; } // ✅ Cédula del actualizador

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Auditable con string ID (User, DesktopClient)
    /// </summary>
    public abstract class AuditableEntityString : BaseEntityString
    {
        [Column("created_by")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; } // ✅ Cédula del creador

        [Column("updated_by")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; } // ✅ Cédula del actualizador

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    // ============================================
    // ENTIDADES SINCRONIZABLES
    // ============================================

    /// <summary>
    /// Syncable con Guid ID (Organization)
    /// </summary>
    public abstract class SyncableEntityGuid : AuditableEntityGuid
    {
        [Column("sync_version")]
        public long SyncVersion { get; set; } = 1;

        [Column("last_sync")]
        public DateTime? LastSync { get; set; }

        [Column("integrity_hash")]
        [MaxLength(64)]
        public string? IntegrityHash { get; set; }

        [Column("sync_status")]
        [MaxLength(20)]
        public string SyncStatus { get; set; } = "pending"; // pending, synced, conflict, error

        [Column("conflict_resolution")]
        [MaxLength(20)]
        public string? ConflictResolution { get; set; } // server_wins, client_wins, manual

        [Column("last_sync_error")]
        public string? LastSyncError { get; set; }
    }

    /// <summary>
    /// Syncable con int ID (Activities, Products, Sales, etc.)
    /// </summary>
    public abstract class SyncableEntityInt : AuditableEntityInt
    {
        [Column("sync_version")]
        public long SyncVersion { get; set; } = 1;

        [Column("last_sync")]
        public DateTime? LastSync { get; set; }

        [Column("integrity_hash")]
        [MaxLength(64)]
        public string? IntegrityHash { get; set; }

        [Column("sync_status")]
        [MaxLength(20)]
        public string SyncStatus { get; set; } = "pending"; // pending, synced, conflict, error

        [Column("conflict_resolution")]
        [MaxLength(20)]
        public string? ConflictResolution { get; set; } // server_wins, client_wins, manual

        [Column("last_sync_error")]
        public string? LastSyncError { get; set; }
    }

    /// <summary>
    /// ✅ SYNCABLE CON STRING ID (User con cédula, DesktopClient)
    /// Esta es la clase que necesitabas!
    /// </summary>
    public abstract class SyncableEntityString : AuditableEntityString
    {
        [Column("sync_version")]
        public long SyncVersion { get; set; } = 1;

        [Column("last_sync")]
        public DateTime? LastSync { get; set; }

        [Column("integrity_hash")]
        [MaxLength(64)]
        public string? IntegrityHash { get; set; }

        [Column("sync_status")]
        [MaxLength(20)]
        public string SyncStatus { get; set; } = "pending"; // pending, synced, conflict, error

        [Column("conflict_resolution")]
        [MaxLength(20)]
        public string? ConflictResolution { get; set; } // server_wins, client_wins, manual

        [Column("last_sync_error")]
        public string? LastSyncError { get; set; }

        // ============================================
        // MÉTODOS HELPER PARA SINCRONIZACIÓN
        // ============================================

        /// <summary>
        /// Marca la entidad como pendiente de sincronización
        /// </summary>
        public virtual void MarkForSync()
        {
            SyncVersion++;
            LastSync = null;
            SyncStatus = "pending";
            LastSyncError = null;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marca la entidad como sincronizada
        /// </summary>
        public virtual void MarkAsSynced()
        {
            LastSync = DateTime.UtcNow;
            SyncStatus = "synced";
            LastSyncError = null;
        }

        /// <summary>
        /// Marca la entidad con error de sincronización
        /// </summary>
        public virtual void MarkSyncError(string error)
        {
            SyncStatus = "error";
            LastSyncError = error;
            LastSync = DateTime.UtcNow; // Para evitar reintentos inmediatos
        }

        /// <summary>
        /// Marca la entidad en conflicto
        /// </summary>
        public virtual void MarkSyncConflict(string resolution = "manual")
        {
            SyncStatus = "conflict";
            ConflictResolution = resolution;
            LastSync = DateTime.UtcNow;
        }

        /// <summary>
        /// Verifica si la entidad necesita sincronización
        /// </summary>
        public virtual bool NeedsSync()
        {
            return SyncStatus == "pending" || 
                   SyncStatus == "error" || 
                   SyncStatus == "conflict" ||
                   LastSync == null ||
                   (UpdatedAt.HasValue && UpdatedAt > LastSync);
        }

        /// <summary>
        /// Calcula hash de integridad
        /// </summary>
        public virtual void UpdateIntegrityHash()
        {
            // Implementación básica - puedes sobrescribir en clases derivadas
            var data = $"{Id}:{SyncVersion}:{UpdatedAt?.Ticks}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            IntegrityHash = Convert.ToBase64String(hash);
        }
    }

    // ============================================
    // INTERFACES PARA SINCRONIZACIÓN
    // ============================================

    /// <summary>
    /// Interface para entidades que se pueden sincronizar
    /// </summary>
    public interface ISyncableEntity
    {
        long SyncVersion { get; set; }
        DateTime? LastSync { get; set; }
        string? IntegrityHash { get; set; }
        string SyncStatus { get; set; }
        string? ConflictResolution { get; set; }
        string? LastSyncError { get; set; }
        
        void MarkForSync();
        void MarkAsSynced();
        void MarkSyncError(string error);
        void MarkSyncConflict(string resolution = "manual");
        bool NeedsSync();
        void UpdateIntegrityHash();
    }

    /// <summary>
    /// Interface para entidades auditables
    /// </summary>
    public interface IAuditableEntity
    {
        string? CreatedBy { get; set; }
        string? UpdatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }

 
}
