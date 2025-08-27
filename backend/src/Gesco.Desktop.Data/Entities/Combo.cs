using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
   // ----------------------------
    // Combo de Venta
    // ----------------------------
    public class ComboVenta
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public decimal PrecioCombo { get; set; }

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
        public virtual Actividad Actividad { get; set; }
        public virtual Usuario CreadoPorUsuario { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
        public virtual ICollection<ComboArticulo> ComboArticulos { get; set; }
        public virtual ICollection<DetalleTransaccionVenta> DetallesTransaccionVenta { get; set; }
    }

    // ----------------------------
    // Artículos que componen cada combo
    // ----------------------------
    public class ComboArticulo
    {
        public int Id { get; set; }
        public int ComboId { get; set; }
        public int ArticuloId { get; set; }

        [Required]
        public int Cantidad { get; set; } = 1;

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual ComboVenta Combo { get; set; }
        public virtual ProductoCategoria Articulo { get; set; }
    }

    // ----------------------------
    // Tipo de Movimiento de Inventario
    // ----------------------------
    public class TipoMovimientoInventario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public bool AfectaStock { get; set; } = true;
        public bool RequiereJustificacion { get; set; } = false;
        public bool Activo { get; set; } = true;
        public DateTime? CreadoEn { get; set; }

        // Navegación
        public virtual ICollection<MovimientoInventario> MovimientosInventario { get; set; }
    }

    // ----------------------------
    // Movimientos de Inventario
    // ----------------------------
    public class MovimientoInventario
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public int TipoMovimientoId { get; set; }

        // Información del movimiento
        [Required]
        public int Cantidad { get; set; } // Positivo=entrada, Negativo=salida

        [Required]
        public int CantidadAnterior { get; set; }

        [Required]
        public int CantidadPosterior { get; set; }

        public decimal? CostoUnitario { get; set; }
        public decimal? ValorTotal { get; set; }

        // Referencias
        public int? TransaccionVentaId { get; set; }
        public string Motivo { get; set; }
        public string Justificacion { get; set; }

        // Auditoría
        public int RealizadoPor { get; set; }
        public int? AutorizadoPor { get; set; }
        public DateTime FechaMovimiento { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual ProductoCategoria Articulo { get; set; }
        public virtual TipoMovimientoInventario TipoMovimiento { get; set; }
        public virtual TransaccionVenta TransaccionVenta { get; set; }
        public virtual Usuario RealizadoPorUsuario { get; set; }
        public virtual Usuario AutorizadoPorUsuario { get; set; }
    }

    // ----------------------------
    // Secuencias de Numeración por Organización
    // ----------------------------
    public class SecuenciaNumeracion
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } // 'factura', 'transaccion'

        [MaxLength(10)]
        public string Prefijo { get; set; }

        [Required]
        public int SiguienteNumero { get; set; } = 1;

        public DateTime? ActualizadoEn { get; set; }
        public int? ActualizadoPor { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
    }

    // ----------------------------
    // Cierre de Caja
    // ----------------------------
    public class CierreCaja
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime FechaCierre { get; set; }

        // Totales calculados
        public int TotalTransacciones { get; set; }
        public int TotalItemsVendidos { get; set; }
        public decimal MontoVentas { get; set; }

        // Totales por método de pago (calculados)
        public decimal EfectivoCalculado { get; set; }
        public decimal TarjetasCalculado { get; set; }
        public decimal SinpesCalculado { get; set; }

        // Arqueo físico (declarado)
        public decimal? EfectivoDeclarado { get; set; }
        public decimal? DiferenciaEfectivo { get; set; }

        // Responsables
        public int CerradaPor { get; set; }
        public int? SupervisadaPor { get; set; }

        // Observaciones
        public string Observaciones { get; set; }
        public string ProblemasReportados { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual Caja Caja { get; set; }
        public virtual Usuario CerradaPorUsuario { get; set; }
        public virtual Usuario SupervisadaPorUsuario { get; set; }
    }

    // ----------------------------
    // Cierre de Actividad
    // ----------------------------
    public class CierreActividad
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }
        public DateTime FechaCierre { get; set; }
        public decimal DuracionHoras { get; set; }

        // Resumen de cajas
        public int TotalCajas { get; set; }
        public int CajasConDiferencias { get; set; }

        // Resumen financiero
        public decimal TotalVentas { get; set; }
        public int TotalTransacciones { get; set; }
        public int TotalArticulosVendidos { get; set; }

        // Resumen de inventario
        public int ArticulosAgotados { get; set; }
        public int ArticulosConStock { get; set; }
        public int TotalUnidadesRestantes { get; set; }
        public decimal ValorInventarioFinal { get; set; }
        public decimal ValorMerma { get; set; }

        // Responsables y control
        public int CerradaPor { get; set; }
        public int? SupervisadaPor { get; set; }
        public string Observaciones { get; set; }
        public string ProblemasReportados { get; set; }

        // Sincronización Simplificada
        public bool Sincronizado { get; set; } = false;
        public DateTime? FechaUltimaSync { get; set; }

        // Navegación
        public virtual Actividad Actividad { get; set; }
        public virtual Usuario CerradaPorUsuario { get; set; }
        public virtual Usuario SupervisadaPorUsuario { get; set; }
    }
}