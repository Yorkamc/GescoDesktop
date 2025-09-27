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

                // ✅ CORREGIDO: Buscar por username O email (usando nueva estructura de cédula)
                var user = await _context.Users
                    .Include(u => u.Organization)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == usuario || u.Email == usuario);

                if (user == null)
                {
                    _logger.LogWarning("User not found: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                if (!user.Active)
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

                // ✅ VERIFICAR PASSWORD CON BCRYPT
                bool passwordValid = false;
                try
                {
                    passwordValid = PasswordHelper.VerifyPassword(password, user.Password);
                    _logger.LogDebug("Password verification result: {Valid} for user: {Usuario}", 
                        passwordValid, usuario);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying password for user: {Usuario}", usuario);
                    
                    // Fallback verification
                    try
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
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
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Contraseña incorrecta"
                    };
                }

                // ✅ GENERAR JWT TOKEN CON CÉDULA
                var token = GenerateJwtToken(user);

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successful login for user: {Usuario}", usuario);

                return new LoginResultDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddHours(24),
                    IsOffline = false,
                    Usuario = new UsuarioDto
                    {
                        Id = user.Id, // ✅ CORREGIDO: Ya es string (cédula)
                        NombreUsuario = user.Username,
                        Correo = user.Email,
                        NombreCompleto = user.FullName ?? user.Username,
                        OrganizacionId = user.OrganizationId.ToString(), // Convert Guid to string
                        RolId = user.RoleId.ToString(), // Convert int to string
                        NombreRol = user.Role?.Name ?? "Unknown"
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

        private string GenerateJwtToken(Gesco.Desktop.Data.Entities.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // ✅ CÉDULA en lugar de Guid
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("organization_id", user.OrganizationId.ToString()),
                    new Claim("role_id", user.RoleId.ToString()),
                    new Claim("cedula", user.Id) // ✅ Claim adicional específico para cédula
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}