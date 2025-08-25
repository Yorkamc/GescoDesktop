using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class ArticuloActividad
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }
        public int CategoriaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal? CostoActividad { get; set; }
        public int CantidadDisponible { get; set; }
        public int CantidadVendida { get; set; }
        public bool Activo { get; set; } = true;

        // Sincronización
        public string Uuid { get; set; }
        public bool PendienteSincronizar { get; set; }

        // Navegación
        public virtual Actividad Actividad { get; set; }
        public virtual CategoriaArticulo Categoria { get; set; }
    }
}
