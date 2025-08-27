using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
   public class Suscripcion
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }
        public int MembresiaId { get; set; }
        public int EstadoId { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaVencimiento { get; set; }

        [Required]
        public DateTime FechaFinGracia { get; set; }

        public DateTime? FechaCancelacion { get; set; }
        public int? CreadoPor { get; set; }
        public DateTime? CreadoEn { get; set; }
        public int? ActualizadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual Membresia Membresia { get; set; }
        public virtual EstadoSuscripcion Estado { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<ClaveActivacion> ClavesActivacion { get; set; }
    }
}