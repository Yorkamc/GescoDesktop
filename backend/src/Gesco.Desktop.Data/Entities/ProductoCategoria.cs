using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
public class ProductoCategoria
    {
        public int Id { get; set; }
        public int ActividadCategoriaId { get; set; }

        // Información del producto
        [MaxLength(50)]
        public string Codigo { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        // Control de inventario
        public int CantidadInicial { get; set; } = 0;
        public int CantidadActual { get; set; } = 0;
        public int CantidadAlerta { get; set; } = 10;

        // Configuración de venta
        public bool Activo { get; set; } = true;

        // Auditoría
        public int CreadoPor { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public int? ActualizadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        [MaxLength(32)]
        public string HashSync { get; set; }

        // Navegación
        public virtual ActividadCategoria ActividadCategoria { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<DetalleTransaccionVenta> DetallesTransaccionVenta { get; set; }
        public virtual ICollection<ComboArticulo> ComboArticulos { get; set; }
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; }
    }
}