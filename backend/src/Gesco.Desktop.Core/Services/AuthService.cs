using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Utils;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly string _jwtSecret;

        public AuthService(LocalDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                        "TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345";
        }

        public async Task<LoginResultDto> LoginAsync(string usuario, string password)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Usuario}", usuario);

                // Buscar usuario por nombre de usuario o email
                var user = await _context.Usuarios
                    .Include(u => u.Organizacion)
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuario || u.Correo == usuario);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                if (!user.Activo)
                {
                    _logger.LogWarning("User inactive: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    };
                }

                // Debug logging
                _logger.LogDebug("Found user: {UserId}, checking password...", user.Id);
                _logger.LogDebug("Stored hash starts with: {HashPrefix}...", 
                    user.Contrasena?.Substring(0, Math.Min(10, user.Contrasena != null ? user.Contrasena.Length : 0)));

                // Verificar contraseña con BCrypt usando PasswordHelper
                bool passwordValid = false;
                try
                {
                    passwordValid = PasswordHelper.VerifyPassword(password, user.Contrasena);
                    _logger.LogDebug("Password verification result: {Valid} for user: {Usuario}", 
                        passwordValid, usuario);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying password for user: {Usuario}", usuario);
                    
                    // En caso de error, intentar verificación directa como fallback
                    try
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Contrasena);
                        _logger.LogDebug("Fallback BCrypt verification result: {Valid}", passwordValid);
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Fallback password verification also failed");
                        return new LoginResultDto
                        {
                            Success = false,
                            Message = "Error al verificar credenciales"
                        };
                    }
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password for user: {Usuario}", usuario);
                    
                    // Log adicional para debugging
                    _logger.LogDebug("Password check failed - Input: '{Password}', Hash: '{Hash}'", 
                        password, user.Contrasena);
                    
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Contraseña incorrecta"
                    };
                }

                // Generar JWT token
                var token = GenerateJwtToken(user);

                // Actualizar último login
                user.UltimoLogin = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successful login for user: {Usuario}", usuario);

                return new LoginResultDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    TokenExpiration = DateTime.Now.AddHours(24),
                    IsOffline = false,
                    Usuario = new UsuarioDto
                    {
                        Id = user.Id,
                        NombreUsuario = user.NombreUsuario,
                        Correo = user.Correo,
                        NombreCompleto = user.NombreCompleto,
                        OrganizacionId = user.OrganizacionId,
                        RolId = user.RolId,
                        NombreRol = user.Rol?.Nombre ?? GetRoleName(user.RolId)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for user: {Usuario}", usuario);
                return new LoginResultDto
                {
                    Success = false,
                    Message = $"Error en autenticación: {ex.Message}"
                };
            }
        }

        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UsuarioDto?> GetCurrentUserAsync()
        {
            await Task.CompletedTask;
            return null;
        }

        private string GenerateJwtToken(Data.Entities.Usuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.NombreUsuario),
                    new Claim(ClaimTypes.Email, user.Correo),
                    new Claim("organizacion_id", user.OrganizacionId?.ToString() ?? ""),
                    new Claim("rol_id", user.RolId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GetRoleName(int rolId)
        {
            return rolId switch
            {
                1 => "Administrador",
                2 => "Vendedor", 
                3 => "Cajero",
                _ => "Usuario"
            };
        }
    }
}