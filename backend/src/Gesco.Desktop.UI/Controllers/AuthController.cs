using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Audit;
using Gesco.Desktop.Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;

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
                _logger.LogInformation("=== AUTH CONTROLLER: LOGIN REQUEST ===");
                _logger.LogInformation("Request Usuario: {Usuario}", request.Usuario);
                
                if (string.IsNullOrEmpty(request.Usuario) || string.IsNullOrEmpty(request.Password))
                {
                    _logger.LogWarning("Login request with missing credentials");
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
                    _logger.LogInformation("=== LOGIN SUCCESSFUL ===");
                    _logger.LogInformation("User: {Username}", request.Usuario);
                    _logger.LogInformation("Token: {Token}", result.Token?.Substring(0, Math.Min(50, result.Token.Length)) + "...");
                    
                    // Decodificar y mostrar claims del token para debugging
                    if (!string.IsNullOrEmpty(result.Token))
                    {
                        try
                        {
                            var handler = new JwtSecurityTokenHandler();
                            var jwtToken = handler.ReadJwtToken(result.Token);
                            _logger.LogInformation("Token Claims:");
                            foreach (var claim in jwtToken.Claims)
                            {
                                _logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not decode token for logging");
                        }
                    }
                    
                    return Ok(result);
                }

                _logger.LogWarning("=== LOGIN FAILED ===");
                _logger.LogWarning("User: {Username}, Reason: {Message}", request.Usuario, result.Message);
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ERROR IN LOGIN CONTROLLER ===");
                _logger.LogError("User: {Username}", request.Usuario);
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
                _logger.LogInformation("Validating token from header: {Header}", 
                    string.IsNullOrEmpty(authHeader) ? "EMPTY" : authHeader.Substring(0, Math.Min(30, authHeader.Length)) + "...");
                
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    _logger.LogWarning("Token validation failed: No token provided or invalid format");
                    return Unauthorized(new { valid = false, message = "Token no proporcionado" });
                }

                var token = authHeader.Replace("Bearer ", "");
                var isValid = await _authService.ValidateTokenAsync(token);

                if (isValid)
                {
                    _logger.LogInformation("Token validation successful");
                    var currentUser = await _authService.GetCurrentUserAsync();
                    return Ok(new { valid = true, user = currentUser });
                }

                _logger.LogWarning("Token validation failed: Invalid token");
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
                _logger.LogInformation("Getting current user info");
                _logger.LogInformation("User claims:");
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
                }
                
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    _logger.LogWarning("Current user not found");
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

        [HttpGet("test-auth")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult TestAuth()
        {
            _logger.LogInformation("=== TEST AUTH ENDPOINT ===");
            _logger.LogInformation("User authenticated: {IsAuth}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("User claims:");
            
            var claims = new Dictionary<string, string>();
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
                claims[claim.Type] = claim.Value;
            }

            return Ok(new 
            { 
                message = "Authentication successful",
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                claims = claims
            });
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