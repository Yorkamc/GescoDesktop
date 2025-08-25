using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreUsuario { get; set; }

        [Required]
        [MaxLength(200)]
        public string Correo { get; set; }

        [MaxLength(200)]
        public string NombreCompleto { get; set; }

        [Required]
        public string Password { get; set; }

        public int? OrganizacionId { get; set; }
        public int RolId { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? UltimoLogin { get; set; }
        public DateTime? UltimaSincronizacion { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
    }
}
