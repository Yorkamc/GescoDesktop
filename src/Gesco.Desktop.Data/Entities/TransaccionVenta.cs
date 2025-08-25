using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class TransaccionVenta
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }
        public int CajaId { get; set; }


        [Required]
        [MaxLength(50)]
        public string NumeroTransaccion { get; set; }


        public DateTime FechaTransaccion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuentos { get; set; }
        public decimal Total { get; set; }
        public int EstadoId { get; set; }
        public int VendedorUsuarioId { get; set; }

        // Cliente

        [MaxLength(200)]
        public string ClienteNombre { get; set; }

        // Sincronización

        public string Uuid { get; set; }
        public bool PendienteSincronizar { get; set; }

        // Navegación

        public virtual Actividad Actividad { get; set; }
        public virtual Caja Caja { get; set; }
        public virtual Usuario VendedorUsuario { get; set; }
        public virtual ICollection<DetalleTransaccionVenta> Detalles { get; set; }
    }
}
