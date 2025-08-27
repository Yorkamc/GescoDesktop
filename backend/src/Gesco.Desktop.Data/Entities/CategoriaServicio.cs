using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
  public class CategoriaServicio
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Auditoría
        public int CreadoPor { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public int? ActualizadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<ActividadCategoria> ActividadCategorias { get; set; }
    }
}