using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }
        public int? UsuarioId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; }

        [Required]
        public string Mensaje { get; set; }

        public string DatosAdicionales { get; set; }
        public bool Leida { get; set; } = false;
        public DateTime? FechaLeida { get; set; }
        public bool Importante { get; set; } = false;
        public DateTime? FechaProgramada { get; set; }
        public DateTime? FechaExpiracion { get; set; }

        [MaxLength(100)]
        public string CanalesEntrega { get; set; }

        [Required]
        public DateTime CreadaEn { get; set; }

        public int? CreadaPor { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual Usuario CreadaPorUsuario { get; set; }
    }
}