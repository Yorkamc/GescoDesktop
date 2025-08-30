using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Context;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StatsController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<StatsController> _logger;

        public StatsController(LocalDbContext context, ILogger<StatsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtener estadísticas generales del sistema
        /// </summary>
        /// <returns>Estadísticas del dashboard</returns>
        [HttpGet]
        [ProducesResponseType(typeof(DashboardStatsDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new DashboardStatsDto
                {
                    // Actividades
                    Actividades = await _context.Actividades.CountAsync(),
                    ActividadesActivas = await _context.Actividades
                        .Where(a => a.EstadoId == 2) // En curso
                        .CountAsync(),
                    
                    // Ventas del día (usar nombre correcto del campo)
                    VentasHoy = await _context.TransaccionesVenta
                        .Where(t => t.FechaTransaccion.Date == today && t.EstadoId == 2) // Completadas
                        .SumAsync(t => (decimal?)t.Total) ?? 0m,
                    
                    Transacciones = await _context.TransaccionesVenta
                        .Where(t => t.FechaTransaccion.Date == today)
                        .CountAsync(),
                    
                    // Ventas del mes
                    VentasMes = await _context.TransaccionesVenta
                        .Where(t => t.FechaTransaccion >= thisMonth && t.EstadoId == 2)
                        .SumAsync(t => (decimal?)t.Total) ?? 0m,
                    
                    TransaccionesMes = await _context.TransaccionesVenta
                        .Where(t => t.FechaTransaccion >= thisMonth)
                        .CountAsync(),
                    
                    // Usuarios
                    TotalUsuarios = await _context.Usuarios.CountAsync(),
                    UsuariosActivos = await _context.Usuarios
                        .Where(u => u.Activo)
                        .CountAsync(),
                    
                    // Productos/Artículos
                    TotalProductos = await _context.ProductosCategorias.CountAsync(),
                    ProductosActivos = await _context.ProductosCategorias
                        .Where(p => p.Activo)
                        .CountAsync(),
                    
                    ProductosAgotados = await _context.ProductosCategorias
                        .Where(p => p.CantidadActual <= p.CantidadAlerta)
                        .CountAsync(),
                    
                    // Timestamps
                    FechaConsulta = DateTime.UtcNow,
                    PeriodoReporte = $"Día {today:dd/MM/yyyy} y mes {thisMonth:MM/yyyy}"
                };

                _logger.LogInformation("Stats retrieved: {Stats}", 
                    $"Activities: {stats.Actividades}, Sales today: {stats.VentasHoy:C}");

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { 
                    message = "Error al obtener estadísticas del sistema",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener resumen de ventas por período
        /// </summary>
        /// <param name="dias">Número de días hacia atrás (por defecto 7)</param>
        /// <returns>Datos de ventas por día</returns>
        [HttpGet("sales-summary")]
        [ProducesResponseType(typeof(List<SalesSummaryDto>), 200)]
        public async Task<IActionResult> GetSalesSummary([FromQuery] int dias = 7)
        {
            try
            {
                var startDate = DateTime.Today.AddDays(-dias);
                
                var salesData = await _context.TransaccionesVenta
                    .Where(t => t.FechaTransaccion >= startDate && t.EstadoId == 2)
                    .GroupBy(t => t.FechaTransaccion.Date)
                    .Select(g => new SalesSummaryDto
                    {
                        Fecha = g.Key,
                        TotalVentas = g.Sum(t => t.Total),
                        Transacciones = g.Count(),
                        PromedioTransaccion = g.Average(t => t.Total)
                    })
                    .OrderBy(s => s.Fecha)
                    .ToListAsync();

                return Ok(salesData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales summary");
                return StatusCode(500, new { message = "Error al obtener resumen de ventas" });
            }
        }

        /// <summary>
        /// Obtener actividades recientes
        /// </summary>
        /// <param name="limite">Número máximo de actividades (por defecto 10)</param>
        /// <returns>Lista de actividades recientes</returns>
        [HttpGet("recent-activities")]
        [Authorize]
        [ProducesResponseType(typeof(List<ActivitySummaryDto>), 200)]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limite = 10)
        {
            try
            {
                var activities = await _context.Actividades
                    .Include(a => a.Estado)
                    .Include(a => a.Organizacion)
                    .OrderByDescending(a => a.CreadoEn)
                    .Take(limite)
                    .Select(a => new ActivitySummaryDto
                    {
                        Id = a.Id,
                        Nombre = a.Nombre,
                        Estado = a.Estado.Nombre,
                        FechaInicio = a.FechaInicio,
                        Organizacion = a.Organizacion.Nombre,
                        CreadoEn = a.CreadoEn
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return StatusCode(500, new { message = "Error al obtener actividades recientes" });
            }
        }
    }

    // DTOs
    public class DashboardStatsDto
    {
        public int Actividades { get; set; }
        public int ActividadesActivas { get; set; }
        public decimal VentasHoy { get; set; }
        public int Transacciones { get; set; }
        public decimal VentasMes { get; set; }
        public int TransaccionesMes { get; set; }
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int ProductosAgotados { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string PeriodoReporte { get; set; } = string.Empty;
    }

    public class SalesSummaryDto
    {
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int Transacciones { get; set; }
        public decimal PromedioTransaccion { get; set; }
    }

    public class ActivitySummaryDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string Organizacion { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
    }
}