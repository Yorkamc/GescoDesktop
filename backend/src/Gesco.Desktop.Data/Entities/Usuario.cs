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

        [MaxLength(50)]
        public string Telefono { get; set; }

        [Required]
        public string Contrasena { get; set; }

        public bool PrimerLogin { get; set; } = true;
        public bool Activo { get; set; } = true;

        // Foreign Keys
        public int? OrganizacionId { get; set; }
        public int RolId { get; set; }
        public int? CreadoPor { get; set; }
        public int? ActualizadoPor { get; set; }

        // Timestamps
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public DateTime? PrimerLoginEn { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual Rol Rol { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
    }
}