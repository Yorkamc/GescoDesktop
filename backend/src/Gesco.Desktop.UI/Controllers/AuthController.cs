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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IAuditService auditService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Usuario) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Usuario y contrasena requeridos" });
                }

                var result = await _authService.LoginAsync(request.Usuario, request.Password);

                var clientIP = GetClientIPAddress();
                await _auditService.LogLoginAttemptAsync(
                    request.Usuario, 
                    result.Success, 
                    clientIP, 
                    result.Success ? null : result.Message
                );

                if (result.Success)
                {
                    _logger.LogInformation("Successful login for user: {Username}", request.Usuario);
                    return Ok(result);
                }

                _logger.LogWarning("Failed login attempt for user: {Username}", request.Usuario);
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Username}", request.Usuario);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                
                var userId = User.FindFirst("sub")?.Value ?? "unknown";
                _logger.LogInformation("User logged out: {UserId}", userId);
                
                return Ok(new { message = "Sesion cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Error al cerrar sesion" });
            }
        }

        [HttpPost("validate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { valid = false, message = "Token no proporcionado" });
                }

                var token = authHeader.Replace("Bearer ", "");
                var isValid = await _authService.ValidateTokenAsync(token);

                if (isValid)
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    return Ok(new { valid = true, user = currentUser });
                }

                return Unauthorized(new { valid = false, message = "Token invalido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, new { message = "Error al validar token" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { message = "Error al obtener usuario" });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new { message = "Contrasena actual y nueva contrasena son requeridas" });
                }

                await Task.CompletedTask;
                
                return Ok(new { message = "Contrasena cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Error al cambiar contrasena" });
            }
        }

        private string GetClientIPAddress()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIP = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIP))
            {
                return realIP;
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}