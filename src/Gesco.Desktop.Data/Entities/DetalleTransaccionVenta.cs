using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class DetalleTransaccionVenta
    {
        public int Id { get; set; }
        public int TransaccionId { get; set; }
        public int? ArticuloId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; }
        
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        
        // Navegación
        public virtual TransaccionVenta Transaccion { get; set; }
        public virtual ArticuloActividad Articulo { get; set; }
    }
}
