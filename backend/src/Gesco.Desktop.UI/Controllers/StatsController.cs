using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;

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

                // Get status entities by name to avoid hardcoded IDs
                var inProgressActivityStatus = await _context.ActivityStatuses
                    .FirstOrDefaultAsync(s => s.Name == "In Progress");

                var completedSalesStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                var stats = new DashboardStatsDto
                {
                    // Actividades
                    TotalActivities = await _context.Activities.CountAsync(),
                    ActiveActivities = inProgressActivityStatus != null 
                        ? await _context.Activities.CountAsync(a => a.ActivityStatusId == inProgressActivityStatus.Id)
                        : 0,
                    
                    // Ventas del día
                    TodaySales = completedSalesStatus != null
                        ? await _context.SalesTransactions
                            .Where(t => t.TransactionDate.Date == today && t.SalesStatusId == completedSalesStatus.Id)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m
                        : await _context.SalesTransactions
                            .Where(t => t.TransactionDate.Date == today)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m,
                    
                    TodayTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate.Date == today),
                    
                    // Ventas del mes
                    MonthSales = completedSalesStatus != null
                        ? await _context.SalesTransactions
                            .Where(t => t.TransactionDate >= thisMonth && t.SalesStatusId == completedSalesStatus.Id)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m
                        : await _context.SalesTransactions
                            .Where(t => t.TransactionDate >= thisMonth)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m,
                    
                    MonthTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate >= thisMonth),
                    
                    // Usuarios
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users
                        .CountAsync(u => u.Active),
                    
                    // Productos/Artículos
                    TotalProducts = await _context.CategoryProducts.CountAsync(),
                    ActiveProducts = await _context.CategoryProducts
                        .CountAsync(p => p.Active),
                    
                    LowStockProducts = await _context.CategoryProducts
                        .CountAsync(p => p.CurrentQuantity <= p.AlertQuantity && p.Active),
                    
                    // Timestamps
                    QueryDate = DateTime.UtcNow,
                    ReportPeriod = $"Día {today:dd/MM/yyyy} y mes {thisMonth:MM/yyyy}"
                };

                _logger.LogInformation("Stats retrieved: Activities: {Activities}, Sales today: {SalesToday:C}", 
                    stats.TotalActivities, stats.TodaySales);

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
                var completedStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");
                
                var salesData = await _context.SalesTransactions
                    .Where(t => t.TransactionDate >= startDate && 
                               (completedStatus == null || t.SalesStatusId == completedStatus.Id))
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new SalesSummaryDto
                    {
                        Fecha = g.Key,
                        TotalVentas = g.Sum(t => t.TotalAmount),
                        Transacciones = g.Count(),
                        PromedioTransaccion = g.Average(t => t.TotalAmount)
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
                var activities = await _context.Activities
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(limite)
                    .Select(a => new ActivitySummaryDto
                    {
                        Id = a.Id,
                        Nombre = a.Name,
                        Estado = a.ActivityStatus.Name,
                        FechaInicio = a.StartDate.ToDateTime(a.StartTime ?? TimeOnly.MinValue),
                        Organizacion = a.Organization != null ? a.Organization.Name : "Sin organización",
                        CreadoEn = a.CreatedAt
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

    // DTOs locales específicos del StatsController
    public class SalesSummaryDto
    {
        public DateTime Fecha { get; set; }
        public decimal TotalVentas { get; set; }
        public int Transacciones { get; set; }
        public decimal PromedioTransaccion { get; set; }
    }

    public class ActivitySummaryDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string Organizacion { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
    }
}