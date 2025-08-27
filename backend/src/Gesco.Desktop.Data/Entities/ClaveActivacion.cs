using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
 public class ClaveActivacion
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CodigoActivacion { get; set; }

        public int SuscripcionesId { get; set; }
        public bool Generada { get; set; } = true;
        public bool Utilizada { get; set; } = false;
        public bool Expirada { get; set; } = false;
        public bool Revocada { get; set; } = false;

        [Required]
        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaExpiracion { get; set; }
        public DateTime? FechaUtilizacion { get; set; }
        public DateTime? FechaRevocacion { get; set; }

        public int UsosMaximos { get; set; } = 1;
        public int UsosActuales { get; set; } = 0;

        [MaxLength(100)]
        public string LoteGeneracion { get; set; }

        public string Notas { get; set; }
        public int? UtilizadaPorOrganizacionId { get; set; }
        public int? UtilizadaPorUsuarioId { get; set; }

        [MaxLength(45)]
        public string IpActivacion { get; set; }

        public int? GeneradaPor { get; set; }
        public int? RevocadaPor { get; set; }
        public string RazonRevocacion { get; set; }

        // Navegación
        public virtual Suscripcion Suscripcion { get; set; }
        public virtual Organizacion UtilizadaPorOrganizacion { get; set; }
        public virtual Usuario UtilizadaPorUsuario { get; set; }
        public virtual Usuario GeneradaPorUsuario { get; set; }
        public virtual Usuario RevocadaPorUsuario { get; set; }
    }
}