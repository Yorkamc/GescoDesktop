using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
 /// <summary>
    /// Gestión de sesiones activas de usuarios
    /// Permite: renovar tokens, cerrar sesiones remotamente, ver dispositivos activos
    /// </summary>
    [Table("user_sessions")]
    public class UserSession : AuditableEntityString
    {
        [Column("user_id")]
        [Required]
        [MaxLength(12)]
        public string UserId { get; set; } = string.Empty;

        [Column("access_token")]
        [Required]
        [MaxLength(255)]
        public string AccessToken { get; set; } = string.Empty;

        [Column("refresh_token")]
        [Required]
        [MaxLength(255)]
        public string RefreshToken { get; set; } = string.Empty;

        [Column("token_uuid")]
        [Required]
        [MaxLength(36)]
        public string TokenUuid { get; set; } = string.Empty;

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Column("last_access_date")]
        public DateTime LastAccessDate { get; set; } = DateTime.UtcNow;

        [Column("expiration_date")]
        public DateTime ExpirationDate { get; set; }

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("end_reason")]
        [MaxLength(100)]
        public string? EndReason { get; set; } // logout, expired, force_close, etc.

        // Navegación
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Métodos de utilidad
        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpirationDate;
        }

        public bool IsValid()
        {
            return Active && !IsExpired();
        }

        public void Close(string reason)
        {
            Active = false;
            EndDate = DateTime.UtcNow;
            EndReason = reason;
        }

        public void UpdateLastAccess()
        {
            LastAccessDate = DateTime.UtcNow;
        }
    }

}