using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
  public class EstadoVenta
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? CreadoEn { get; set; }

        // Navegación
        public virtual ICollection<TransaccionVenta> TransaccionesVenta { get; set; }
    }

    // ----------------------------
    // Método de Pago
    // ----------------------------
    public class MetodoPago
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public bool RequiereReferencia { get; set; } = false;
        public bool Activo { get; set; } = true;
        public DateTime? CreadoEn { get; set; }

        // Navegación
        public virtual ICollection<PagoTransaccion> PagosTransacciones { get; set; }
    }

    // ----------------------------
    // Caja de Venta (Tabla Crítica)
    // ----------------------------
    public class Caja
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }
        public int NumeroCaja { get; set; }

        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(200)]
        public string Ubicacion { get; set; }

        public bool Abierta { get; set; } = false;
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? OperadorUsuarioId { get; set; }
        public int? SupervisorUsuarioId { get; set; }

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
        public virtual Actividad Actividad { get; set; }
        public virtual Usuario OperadorUsuario { get; set; }
        public virtual Usuario SupervisorUsuario { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<TransaccionVenta> TransaccionesVenta { get; set; }
        public virtual ICollection<CierreCaja> CierresCaja { get; set; }
    }

    // ----------------------------
    // Transacciones de Venta (Tabla Crítica)
    // ----------------------------
    public class TransaccionVenta
    {
        public int Id { get; set; }
        public int CajaId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroTransaccion { get; set; }

        [MaxLength(50)]
        public string NumeroFactura { get; set; }

        // Estado y fechas
        public int EstadoId { get; set; }
        public DateTime FechaTransaccion { get; set; }

        // Totales financieros
        [Required]
        public decimal Total { get; set; }

        // Vendedor
        public int VendedorUsuarioId { get; set; }

        // Auditoría
        public DateTime CreadoEn { get; set; } = DateTime.Now;
        public int? ActualizadoPor { get; set; }
        public DateTime? ActualizadoEn { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        [MaxLength(32)]
        public string HashSync { get; set; }

        // Navegación
        public virtual Caja Caja { get; set; }
        public virtual EstadoVenta Estado { get; set; }
        public virtual Usuario VendedorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<DetalleTransaccionVenta> DetallesTransaccionVenta { get; set; }
        public virtual ICollection<PagoTransaccion> PagosTransacciones { get; set; }
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; }
    }

    // ----------------------------
    // Detalle de Transacciones (Tabla Crítica)
    // ----------------------------
    public class DetalleTransaccionVenta
    {
        public int Id { get; set; }
        public int TransaccionId { get; set; }
        public int? ArticuloId { get; set; }
        public int? ComboId { get; set; }

        // Información al momento de la venta
        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public decimal Total { get; set; }

        // Metadatos
        public bool EsCombo { get; set; } = false;

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual TransaccionVenta Transaccion { get; set; }
        public virtual ProductoCategoria Articulo { get; set; }
        public virtual ComboVenta Combo { get; set; }
    }

    // ----------------------------
    // Pagos de Transacciones
    // ----------------------------
    public class PagoTransaccion
    {
        public int Id { get; set; }
        public int TransaccionId { get; set; }
        public int MetodoPagoId { get; set; }

        // Información del pago
        [Required]
        public decimal Monto { get; set; }

        [MaxLength(100)]
        public string Referencia { get; set; }

        // Control y estado
        public DateTime ProcesadoEn { get; set; }
        public int ProcesadoPor { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual TransaccionVenta Transaccion { get; set; }
        public virtual MetodoPago MetodoPago { get; set; }
        public virtual Usuario ProcesadoPorUsuario { get; set; }
    }
}