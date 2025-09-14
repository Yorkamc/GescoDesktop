using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
[Table("oauth_refresh_tokens")]
    public class OAuthRefreshToken : BaseEntityString
    {
        [Column("access_token_id")]
        [Required]
        [MaxLength(80)]
        public string AccessTokenId { get; set; } = string.Empty;

        [Column("revoked")]
        public bool Revoked { get; set; } = false;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        // Navegación
        [ForeignKey("AccessTokenId")]
        public virtual OAuthAccessToken AccessToken { get; set; } = null!;
    }
}