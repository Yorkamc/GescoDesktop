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
        /// DEBUG: Ver todas las ventas y sus estados
        /// </summary>
        [HttpGet("debug/sales")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DebugSales()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var allSales = await _context.SalesTransactions
                    .Select(t => new
                    {
                        t.Id,
                        t.TransactionNumber,
                        t.SalesStatusId,
                        t.TotalAmount,
                        t.TransactionDate
                    })
                    .ToListAsync();

                var todaySales = allSales.Where(s => s.TransactionDate.Date == today).ToList();
                var monthSales = allSales.Where(s => s.TransactionDate >= thisMonth).ToList();

                var statuses = await _context.SalesStatuses
                    .Select(s => new { s.Id, s.Name })
                    .ToListAsync();

                return Ok(new
                {
                    Today = today.ToString("yyyy-MM-dd"),
                    ThisMonth = thisMonth.ToString("yyyy-MM-dd"),
                    AvailableStatuses = statuses,
                    TotalSalesInDB = allSales.Count,
                    TodaySalesCount = todaySales.Count,
                    TodaySalesTotal = todaySales.Sum(s => s.TotalAmount),
                    MonthSalesCount = monthSales.Count,
                    MonthSalesTotal = monthSales.Sum(s => s.TotalAmount),
                    TodaySales = todaySales,
                    MonthSales = monthSales
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug endpoint");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Obtener estadísticas generales del sistema
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(DashboardStatsDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                _logger.LogInformation("Getting dashboard stats...");

                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                // ✅ Usar ID directo: 2 = Completed
                var completedStatusId = 2;

                _logger.LogInformation("Using completed status ID: {StatusId}", completedStatusId);
                _logger.LogInformation("Today: {Today}, This month: {Month}", today, thisMonth);

                var totalActivities = 0;
                var activeActivities = 0;
                var todaySales = 0m;
                var todayTransactions = 0;
                var monthSales = 0m;
                var monthTransactions = 0;
                var totalUsers = 0;
                var activeUsers = 0;
                var totalProducts = 0;
                var activeProducts = 0;
                var lowStockProducts = 0;

                try
                {
                    totalActivities = await _context.Activities.CountAsync();
                    _logger.LogInformation("Total activities: {Count}", totalActivities);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting total activities");
                }

                try
                {
                    // ActivityStatusId 2 = "In Progress"
                    activeActivities = await _context.Activities
                        .CountAsync(a => a.ActivityStatusId == 2);
                    _logger.LogInformation("Active activities: {Count}", activeActivities);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting active activities");
                }

                try
                {
                    // ✅ Ventas de HOY con estado Completed (ID = 2)
                    var todaySalesData = await _context.SalesTransactions
                        .Where(t => t.TransactionDate.Date == today && t.SalesStatusId == completedStatusId)
                        .ToListAsync();

                    todaySales = todaySalesData.Sum(t => t.TotalAmount);
                    todayTransactions = todaySalesData.Count;

                    _logger.LogInformation("Today: Found {Count} completed sales totaling {Amount:C}", 
                        todayTransactions, todaySales);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting today sales");
                }

                try
                {
                    // ✅ Ventas del MES con estado Completed (ID = 2)
                    var monthSalesData = await _context.SalesTransactions
                        .Where(t => t.TransactionDate >= thisMonth && t.SalesStatusId == completedStatusId)
                        .ToListAsync();

                    monthSales = monthSalesData.Sum(t => t.TotalAmount);
                    monthTransactions = monthSalesData.Count;

                    _logger.LogInformation("Month: Found {Count} completed sales totaling {Amount:C}", 
                        monthTransactions, monthSales);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting month sales");
                }

                try
                {
                    totalUsers = await _context.Users.CountAsync();
                    activeUsers = await _context.Users.CountAsync(u => u.Active);
                    _logger.LogInformation("Users: {Total} total, {Active} active", totalUsers, activeUsers);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting user stats");
                }

                try
                {
                    totalProducts = await _context.CategoryProducts.CountAsync();
                    activeProducts = await _context.CategoryProducts.CountAsync(p => p.Active);
                    lowStockProducts = await _context.CategoryProducts
                        .CountAsync(p => p.CurrentQuantity <= p.AlertQuantity && p.Active);
                    _logger.LogInformation("Products: {Total} total, {Active} active, {LowStock} low stock", 
                        totalProducts, activeProducts, lowStockProducts);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting product stats");
                }

                var stats = new DashboardStatsDto
                {
                    TotalActivities = totalActivities,
                    ActiveActivities = activeActivities,
                    TodaySales = todaySales,
                    TodayTransactions = todayTransactions,
                    MonthSales = monthSales,
                    MonthTransactions = monthTransactions,
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    LowStockProducts = lowStockProducts,
                    QueryDate = DateTime.UtcNow,
                    ReportPeriod = $"Día {today:dd/MM/yyyy} y mes {thisMonth:MM/yyyy}"
                };

                _logger.LogInformation(
                    "✅ Stats retrieved: Activities: {Activities}, Today sales: {TodaySales:C} ({TodayTrans} trans), Month sales: {MonthSales:C} ({MonthTrans} trans)", 
                    stats.TotalActivities, 
                    stats.TodaySales, 
                    stats.TodayTransactions,
                    stats.MonthSales,
                    stats.MonthTransactions
                );

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                
                var defaultStats = new DashboardStatsDto
                {
                    TotalActivities = 0,
                    ActiveActivities = 0,
                    TodaySales = 0,
                    TodayTransactions = 0,
                    MonthSales = 0,
                    MonthTransactions = 0,
                    TotalUsers = 0,
                    ActiveUsers = 0,
                    TotalProducts = 0,
                    ActiveProducts = 0,
                    LowStockProducts = 0,
                    QueryDate = DateTime.UtcNow,
                    ReportPeriod = "Error al cargar datos"
                };

                return Ok(defaultStats);
            }
        }

        /// <summary>
        /// Obtener resumen de ventas por período
        /// </summary>
        [HttpGet("sales-summary")]
        [ProducesResponseType(typeof(List<SalesSummaryDto>), 200)]
        public async Task<IActionResult> GetSalesSummary([FromQuery] int dias = 7)
        {
            try {
                var startDate = DateTime.Today.AddDays(-dias);
                var completedStatusId = 2;
                
                var salesData = await _context.SalesTransactions
                    .Where(t => t.TransactionDate >= startDate && t.SalesStatusId == completedStatusId)
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new SalesSummaryDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(t => t.TotalAmount),
                        TotalTransactions = g.Count(),
                        CompletedTransactions = g.Count(),
                        AverageTransaction = g.Average(t => t.TotalAmount),
                        TotalItemsSold = 0
                    })
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                return Ok(salesData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales summary");
                return Ok(new List<SalesSummaryDto>());
            }
        }

        /// <summary>
        /// Obtener actividades recientes
        /// </summary>
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
                return Ok(new List<ActivitySummaryDto>());
            }
        }
    }

    public class ActivitySummaryDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public string Organizacion { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
    }
}