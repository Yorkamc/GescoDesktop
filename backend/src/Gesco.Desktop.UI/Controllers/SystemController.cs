using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs; // AGREGADO: Usar DTOs compartidos
using System.Diagnostics;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SystemController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<SystemController> _logger;

        public SystemController(LocalDbContext context, ILogger<SystemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Health check del sistema
        /// </summary>
        /// <returns>Estado de salud del sistema</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(HealthCheckDto), 200)]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Verificar conexión a base de datos
                var canConnectToDb = await _context.Database.CanConnectAsync();
                
                var health = new HealthCheckDto
                {
                    Status = canConnectToDb ? "healthy" : "unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    DatabaseConnection = canConnectToDb,
                    Uptime = GetUptime(),
                    MemoryUsage = GetMemoryUsage()
                };

                if (!canConnectToDb)
                {
                    _logger.LogWarning("Health check failed: Cannot connect to database");
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check");
                return StatusCode(500, new HealthCheckDto
                {
                    Status = "error",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener estadísticas generales del sistema
        /// </summary>
        /// <returns>Estadísticas del sistema</returns>
        [HttpGet("stats")]
        [Authorize]
        [ProducesResponseType(typeof(DashboardStatsDto), 200)] // CORREGIDO: Usar DTO compartido
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                // Get statuses by name to avoid hardcoded IDs
                var inProgressActivityStatus = await _context.ActivityStatuses
                    .FirstOrDefaultAsync(s => s.Name == "In Progress");

                var completedSalesStatus = await _context.SalesStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                var stats = new DashboardStatsDto // CORREGIDO: Usar DTO compartido
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
                    ReportPeriod = "Día actual y mes actual"
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system stats");
                return StatusCode(500, new { message = "Error al obtener estadísticas del sistema" });
            }
        }

        /// <summary>
        /// Obtener información del sistema
        /// </summary>
        /// <returns>Información detallada del sistema</returns>
        [HttpGet("info")]
        [Authorize]
        [ProducesResponseType(typeof(SystemInfoDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetSystemInfo()
        {
            try
            {
                var info = new SystemInfoDto
                {
                    Version = "1.0.0",
                    BuildDate = System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    ServerTime = DateTime.UtcNow,
                    Platform = System.Environment.OSVersion.ToString(),
                    ProcessorCount = System.Environment.ProcessorCount,
                    WorkingSet = System.Environment.WorkingSet,
                    GCMemory = GC.GetTotalMemory(false),
                    Runtime = System.Environment.Version.ToString(),
                    MachineName = System.Environment.MachineName,
                    UserName = System.Environment.UserName,
                    Uptime = GetUptime()
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system info");
                return StatusCode(500, new { message = "Error al obtener información del sistema" });
            }
        }

        /// <summary>
        /// Obtener configuraciones del sistema (solo administradores)
        /// </summary>
        /// <returns>Configuraciones del sistema</returns>
        [HttpGet("config")]
        [Authorize] // TODO: Agregar rol de administrador
        [ProducesResponseType(typeof(List<SystemConfigurationDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetSystemConfig()
        {
            try
            {
                // TODO: Verificar que el usuario sea administrador
                
                var configs = await _context.SystemConfigurations
                    .Where(c => c.AccessLevel == "admin" || c.AccessLevel == "user")
                    .Select(c => new SystemConfigurationDto
                    {
                        Id = c.Id,
                        Key = c.Key,
                        Value = c.IsSensitive ? "***" : c.Value, // Hide sensitive values
                        DataType = c.DataType,
                        Category = c.Category,
                        Description = c.Description,
                        IsEditable = c.IsEditable,
                        AccessLevel = c.AccessLevel,
                        DisplayOrder = c.DisplayOrder,
                        ValidationPattern = c.ValidationPattern,
                        MinValue = c.MinValue,
                        MaxValue = c.MaxValue,
                        AllowedValues = c.AllowedValues,
                        IsSensitive = c.IsSensitive,
                        Environment = c.Environment,
                        RestartRequired = c.RestartRequired,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .OrderBy(c => c.Category)
                    .ThenBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Key)
                    .ToListAsync();

                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system config");
                return StatusCode(500, new { message = "Error al obtener configuración del sistema" });
            }
        }

        /// <summary>
        /// Actualizar configuración del sistema
        /// </summary>
        /// <param name="request">Datos de configuración a actualizar</param>
        /// <returns>Resultado de la actualización</returns>
        [HttpPut("config")]
        [Authorize] // TODO: Agregar rol de administrador
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSystemConfig([FromBody] UpdateSystemConfigurationRequest request)
        {
            try
            {
                var config = await _context.SystemConfigurations
                    .FirstOrDefaultAsync(c => c.Key == request.Key);

                if (config == null)
                {
                    return NotFound(new { message = "Configuración no encontrada" });
                }

                if (!config.IsEditable)
                {
                    return BadRequest(new { message = "Esta configuración no es editable" });
                }

                config.Value = request.Value;
                config.UpdatedAt = DateTime.UtcNow;
                // TODO: Obtener usuario actual y asignar a UpdatedBy

                await _context.SaveChangesAsync();

                _logger.LogInformation("System configuration updated: {Key} = {Value}", 
                    request.Key, config.IsSensitive ? "***" : request.Value);

                return Ok(new { 
                    message = "Configuración actualizada exitosamente",
                    restartRequired = config.RestartRequired
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system config");
                return StatusCode(500, new { message = "Error al actualizar configuración" });
            }
        }

        private string GetUptime()
        {
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }

        private string GetMemoryUsage()
        {
            var workingSet = System.Environment.WorkingSet / 1024 / 1024; // MB
            var gcMemory = GC.GetTotalMemory(false) / 1024 / 1024; // MB
            return $"Working Set: {workingSet} MB, GC Memory: {gcMemory} MB";
        }
    }

    // DTOs locales específicos del SystemController (diferentes nombres para evitar conflictos)
    public class HealthCheckDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public bool DatabaseConnection { get; set; }
        public string Uptime { get; set; } = string.Empty;
        public string MemoryUsage { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public class SystemInfoDto
    {
        public string Version { get; set; } = string.Empty;
        public DateTime BuildDate { get; set; }
        public string Environment { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public string Platform { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public long WorkingSet { get; set; }
        public long GCMemory { get; set; }
        public string Runtime { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
    }

    // DTO para SystemConfiguration - ya existe en Shared/DTOs, pero Swagger necesita ver la referencia aquí
    public class SystemConfigurationDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEditable { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? ValidationPattern { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? AllowedValues { get; set; }
        public bool IsSensitive { get; set; }
        public string Environment { get; set; } = string.Empty;
        public bool RestartRequired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Request para actualizar configuración
    public class UpdateSystemConfigurationRequest
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}