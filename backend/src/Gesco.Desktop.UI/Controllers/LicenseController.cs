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
                    return BadRequest(new { message = "Codigo de activacion requerido" });
                }

                if (request.OrganizacionId <= 0)
                {
                    return BadRequest(new { message = "ID de organizacion valido requerido" });
                }

                var maskedCode = request.CodigoActivacion.Length > 5 
                    ? request.CodigoActivacion.Substring(0, 5) + "***"
                    : "***";
                
                _logger.LogInformation("Attempting license activation with code: {Code}", maskedCode);

                var result = await _activationService.ActivateAsync(request.CodigoActivacion, request.OrganizacionId);

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
                    message = isValid ? "Licencia valida" : "Licencia invalida o expirada",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license");
                return StatusCode(500, new { message = "Error al validar licencia" });
            }
        }

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
                return StatusCode(500, new { message = "Error al obtener informacion de licencia" });
            }
        }

        [HttpPost("renew")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(501)]
        public async Task<IActionResult> RenewLicense([FromBody] RenewLicenseRequest request)
        {
            await Task.CompletedTask;
            return StatusCode(501, new { message = "Funcionalidad de renovacion no implementada" });
        }

        private static string DetermineLicenseType(LicenseStatusDto status)
        {
            if (!status.IsActive) return "Inactiva";
            if (status.DiasRestantes < 30) return "Proximo a vencer";
            if (status.MaxUsuarios <= 5) return "Basica";
            if (status.MaxUsuarios <= 20) return "Estandar";
            return "Premium";
        }

        private static List<string> GetLicenseFeatures(LicenseStatusDto status)
        {
            var features = new List<string>();

            if (status.IsActive)
            {
                features.Add("Gestion de Actividades");
                features.Add("Sistema de Ventas");
                features.Add("Reportes Basicos");

                if (status.MaxUsuarios > 5)
                {
                    features.Add("Multiples Usuarios");
                    features.Add("Roles y Permisos");
                }

                if (status.MaxUsuarios > 20)
                {
                    features.Add("Reportes Avanzados");
                    features.Add("Sincronizacion en Linea");
                    features.Add("Soporte Prioritario");
                }
            }

            return features;
        }
    }

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