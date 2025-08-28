using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LicenseController : ControllerBase
    {
        private readonly IActivationService _activationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<LicenseController> _logger;

        public LicenseController(
            IActivationService activationService,
            IAuditService auditService,
            ILogger<LicenseController> logger)
        {
            _activationService = activationService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Activar licencia del sistema
        /// </summary>
        /// <param name="request">Datos de activación</param>
        /// <returns>Resultado de la activación</returns>
        [HttpPost("activate")]
        [ProducesResponseType(typeof(ActivationResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Activate([FromBody] ActivationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CodigoActivacion))
                {
                    return BadRequest(new { message = "Código de activación requerido" });
                }

                if (request.OrganizacionId <= 0)
                {
                    return BadRequest(new { message = "ID de organización válido requerido" });
                }

                _logger.LogInformation("Attempting license activation with code: {Code}", 
                    request.CodigoActivacion?.Substring(0, Math.Min(5, request.CodigoActivacion.Length)) + "***");

                var result = await _activationService.ActivateAsync(request.CodigoActivacion, request.OrganizacionId);

                // Log de auditoría
                var userId = User.FindFirst("sub")?.Value ?? "system";
                await _auditService.LogLicenseActivationAsync(request.CodigoActivacion, result.Success, userId);

                if (result.Success)
                {
                    _logger.LogInformation("License activated successfully");
                    return Ok(result);
                }

                _logger.LogWarning("License activation failed: {Message}", result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during license activation");
                return StatusCode(500, new { message = "Error interno al activar licencia" });
            }
        }

        /// <summary>
        /// Obtener estado actual de la licencia
        /// </summary>
        /// <returns>Estado detallado de la licencia</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(LicenseStatusDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _activationService.GetLicenseStatusAsync();
                
                _logger.LogInformation("License status checked - IsActive: {IsActive}, DaysRemaining: {Days}", 
                    status.IsActive, status.DiasRestantes);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting license status");
                return StatusCode(500, new { message = "Error al obtener estado de licencia" });
            }
        }

        /// <summary>
        /// Validar si la licencia actual está activa
        /// </summary>
        /// <returns>Estado de validez de la licencia</returns>
        [HttpGet("validate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Validate()
        {
            try
            {
                var isValid = await _activationService.ValidateLicenseAsync();
                
                return Ok(new 
                { 
                    isValid = isValid,
                    message = isValid ? "Licencia válida" : "Licencia inválida o expirada",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license");
                return StatusCode(500, new { message = "Error al validar licencia" });
            }
        }

        /// <summary>
        /// Obtener información detallada de la licencia (requiere autenticación)
        /// </summary>
        /// <returns>Información completa de la licencia</returns>
        [HttpGet("info")]
        [Authorize]
        [ProducesResponseType(typeof(LicenseInfoDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetLicenseInfo()
        {
            try
            {
                var status = await _activationService.GetLicenseStatusAsync();
                
                var info = new LicenseInfoDto
                {
                    IsActive = status.IsActive,
                    FechaActivacion = status.FechaActivacion,
                    FechaExpiracion = status.FechaExpiracion,
                    DiasRestantes = status.DiasRestantes,
                    MaxUsuarios = status.MaxUsuarios,
                    OrganizacionId = status.OrganizacionId,
                    TipoLicencia = DetermineLicenseType(status),
                    Caracteristicas = GetLicenseFeatures(status)
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting license info");
                return StatusCode(500, new { message = "Error al obtener información de licencia" });
            }
        }

        /// <summary>
        /// Renovar licencia (funcionalidad futura)
        /// </summary>
        /// <param name="request">Datos de renovación</param>
        /// <returns>Resultado de la renovación</returns>
        [HttpPost("renew")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(501)]
        public async Task<IActionResult> RenewLicense([FromBody] RenewLicenseRequest request)
        {
            // TODO: Implementar renovación de licencia
            await Task.CompletedTask;
            return StatusCode(501, new { message = "Funcionalidad de renovación no implementada" });
        }

        private string DetermineLicenseType(LicenseStatusDto status)
        {
            if (!status.IsActive) return "Inactiva";
            if (status.DiasRestantes < 30) return "Próximo a vencer";
            if (status.MaxUsuarios <= 5) return "Básica";
            if (status.MaxUsuarios <= 20) return "Estándar";
            return "Premium";
        }

        private List<string> GetLicenseFeatures(LicenseStatusDto status)
        {
            var features = new List<string>();

            if (status.IsActive)
            {
                features.Add("Gestión de Actividades");
                features.Add("Sistema de Ventas");
                features.Add("Reportes Básicos");

                if (status.MaxUsuarios > 5)
                {
                    features.Add("Múltiples Usuarios");
                    features.Add("Roles y Permisos");
                }

                if (status.MaxUsuarios > 20)
                {
                    features.Add("Reportes Avanzados");
                    features.Add("Sincronización en Línea");
                    features.Add("Soporte Prioritario");
                }
            }

            return features;
        }
    }

    // DTOs adicionales
    public class LicenseInfoDto : LicenseStatusDto
    {
        public string TipoLicencia { get; set; } = string.Empty;
        public List<string> Caracteristicas { get; set; } = new();
    }

    public class RenewLicenseRequest
    {
        public string CodigoRenovacion { get; set; } = string.Empty;
        public int PeriodoMeses { get; set; }
    }
}