using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
 public class ActividadCategoria
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }
        public int CategoriaServicioId { get; set; }

        // Auditoría
        public int CreadoPor { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public int? ActualizadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual Actividad Actividad { get; set; }
        public virtual CategoriaServicio CategoriaServicio { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<ProductoCategoria> ProductosCategorias { get; set; }
    }
}