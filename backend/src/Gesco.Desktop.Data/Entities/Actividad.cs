using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
 public class Actividad
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        // Fechas y ubicación
        [Required]
        public DateTime FechaInicio { get; set; }

        public TimeSpan? HoraInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public TimeSpan? HoraFin { get; set; }

        [MaxLength(200)]
        public string Ubicacion { get; set; }

        // Estados y responsables
        public int EstadoId { get; set; }
        public int? EncargadoUsuarioId { get; set; }
        public int OrganizacionId { get; set; }

        // Auditoría
        public int ?CreadoPor { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public int? ActualizadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        [MaxLength(32)]
        public string HashSync { get; set; }

        // Navegación
        public virtual EstadoActividad Estado { get; set; }
        public virtual Usuario EncargadoUsuario { get; set; }
        public virtual Organizacion Organizacion { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }

        public virtual ICollection<ActividadCategoria> ActividadCategorias { get; set; }
        public virtual ICollection<ComboVenta> CombosVenta { get; set; }
        public virtual ICollection<Caja> Cajas { get; set; }
        public virtual ICollection<CierreActividad> CierresActividad { get; set; }
    }
}