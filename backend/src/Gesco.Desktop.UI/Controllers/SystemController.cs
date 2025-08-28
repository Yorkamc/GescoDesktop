using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Context;
using System.Diagnostics;
using Gesco.Desktop.Data.Entities;

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
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
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
        [ProducesResponseType(typeof(SystemStatsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var stats = new SystemStatsDto
                {
                    // Actividades
                    TotalActividades = await _context.Actividades.CountAsync(),
                    ActividadesActivas = await _context.Actividades
                        .Where(a => a.EstadoId == 2) // En curso
                        .CountAsync(),
                    
                    // Ventas del día
                    VentasHoy = await _context.TransaccionesVenta
                        .Where(t => t.FechaTransaccion.Date == today && t.EstadoId == 2) // Completadas
                        .SumAsync(t => (decimal?)t.Total) ?? 0m,
                    
                    TransaccionesHoy = await _context.TransaccionesVenta
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
                    PeriodoReporte = "Día actual y mes actual"
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
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    ServerTime = DateTime.UtcNow,
                    Platform = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    GCMemory = GC.GetTotalMemory(false),
                    Runtime = Environment.Version.ToString(),
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
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
        [ProducesResponseType(typeof(List<ConfiguracionSistema>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetSystemConfig()
        {
            try
            {
                // TODO: Verificar que el usuario sea administrador
                
                var configs = await _context.ConfiguracionesSistema
                    .Where(c => c.NivelAcceso == "admin" || c.NivelAcceso == "user")
                    .Select(c => new
                    {
                        c.Id,
                        c.Clave,
                        c.Valor,
                        c.TipoValor,
                        c.Categoria,
                        c.Descripcion,
                        c.EsEditable,
                        c.ActualizadoEn
                    })
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
        public async Task<IActionResult> UpdateSystemConfig([FromBody] UpdateConfigRequest request)
        {
            try
            {
                var config = await _context.ConfiguracionesSistema
                    .FirstOrDefaultAsync(c => c.Clave == request.Clave);

                if (config == null)
                {
                    return NotFound(new { message = "Configuración no encontrada" });
                }

                if (!config.EsEditable)
                {
                    return BadRequest(new { message = "Esta configuración no es editable" });
                }

                config.Valor = request.Valor;
                config.ActualizadoEn = DateTime.UtcNow;
                // TODO: Obtener usuario actual y asignar a ActualizadoPor

                await _context.SaveChangesAsync();

                _logger.LogInformation("System configuration updated: {Key} = {Value}", request.Clave, request.Valor);

                return Ok(new { message = "Configuración actualizada exitosamente" });
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
            var workingSet = Environment.WorkingSet / 1024 / 1024; // MB
            var gcMemory = GC.GetTotalMemory(false) / 1024 / 1024; // MB
            return $"Working Set: {workingSet} MB, GC Memory: {gcMemory} MB";
        }
    }

    // DTOs
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

    public class SystemStatsDto
    {
        public int TotalActividades { get; set; }
        public int ActividadesActivas { get; set; }
        public decimal VentasHoy { get; set; }
        public int TransaccionesHoy { get; set; }
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

    public class UpdateConfigRequest
    {
        public string Clave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }
}