using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
    // USUARIOS - CAMBIADO DE GUID A STRING (CÉDULA)
    [Table("users")]
    public class User : SyncableEntityString  
    {
        [Column("cedula")] 
        [Required]
        [MaxLength(20)]
        public override string Id { get; set; } = string.Empty; 

        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Column("email_verified_at")]
        public DateTime? EmailVerifiedAt { get; set; }

        [Column("password")]
        [Required]
        public string Password { get; set; } = string.Empty;

        [Column("full_name")]
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Column("phone")]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [Column("first_login")]
        public bool FirstLogin { get; set; } = true;

        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("organization_id")]
        public Guid OrganizationId { get; set; } // MANTIENE Guid para Organization

        [Column("role_id")]
        public int RoleId { get; set; } // MANTIENE int para Role

        [Column("first_login_at")]
        public DateTime? FirstLoginAt { get; set; }

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        // Navegación
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }
}