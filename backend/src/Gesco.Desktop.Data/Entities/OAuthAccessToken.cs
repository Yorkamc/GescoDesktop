using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
    [Table("oauth_access_tokens")]
    public class OAuthAccessToken : BaseEntityString
    {
        // ✅ CORREGIDO: UserId debe ser string para coincidir con User.Id (cédula)
        [Column("user_id")]
        [MaxLength(50)]
        public string? UserId { get; set; } // CAMBIADO DE Guid? A string?

        [Column("client_id")]
        public Guid ClientId { get; set; }

        [Column("name")]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Column("scopes")]
        public string? Scopes { get; set; }

        [Column("revoked")]
        public bool Revoked { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        // ✅ CORREGIDO: Navegación debe usar string (cédula)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}