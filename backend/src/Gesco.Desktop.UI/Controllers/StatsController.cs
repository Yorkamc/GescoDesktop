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

                var inProgressActivityStatus = await _context.ActivityStatuses
                    .FirstOrDefaultAsync(s => s.Name == "In Progress");

                var completedSalesStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                _logger.LogInformation("Found activity status: {StatusFound}", inProgressActivityStatus != null);
                _logger.LogInformation("Found sales status: {StatusFound}", completedSalesStatus != null);

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
                    if (inProgressActivityStatus != null)
                    {
                        activeActivities = await _context.Activities
                            .CountAsync(a => a.ActivityStatusId == inProgressActivityStatus.Id);
                    }
                    _logger.LogInformation("Active activities: {Count}", activeActivities);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting active activities");
                }

                try
                {
                    if (completedSalesStatus != null)
                    {
                        todaySales = await _context.SalesTransactions
                            .Where(t => t.TransactionDate.Date == today && t.SalesStatusId == completedSalesStatus.Id)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m;
                    }
                    else
                    {
                        todaySales = await _context.SalesTransactions
                            .Where(t => t.TransactionDate.Date == today)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m;
                    }
                    _logger.LogInformation("Today sales: {Amount}", todaySales);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting today sales");
                }

                try
                {
                    todayTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate.Date == today);
                    _logger.LogInformation("Today transactions: {Count}", todayTransactions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting today transactions");
                }

                try
                {
                    if (completedSalesStatus != null)
                    {
                        monthSales = await _context.SalesTransactions
                            .Where(t => t.TransactionDate >= thisMonth && t.SalesStatusId == completedSalesStatus.Id)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m;
                    }
                    else
                    {
                        monthSales = await _context.SalesTransactions
                            .Where(t => t.TransactionDate >= thisMonth)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m;
                    }
                    _logger.LogInformation("Month sales: {Amount}", monthSales);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting month sales");
                }

                try
                {
                    monthTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate >= thisMonth);
                    _logger.LogInformation("Month transactions: {Count}", monthTransactions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting month transactions");
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

                _logger.LogInformation("Stats retrieved successfully: Activities: {Activities}, Sales today: {SalesToday:C}", 
                    stats.TotalActivities, stats.TodaySales);

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
                        Date = g.Key,
                        TotalSales = g.Sum(t => t.TotalAmount),
                        TotalTransactions = g.Count(),
                        CompletedTransactions = g.Count(),
                        AverageTransaction = g.Average(t => t.TotalAmount),
                        TotalItemsSold = 0 // Se puede calcular con un join adicional si es necesario
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

    // ============================================
    // ✅ DTOs LOCALES - CON NOMBRES ÚNICOS
    // ============================================
    
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