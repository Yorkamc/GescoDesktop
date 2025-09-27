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
                _logger.LogInformation("🔐 Attempting login for user: {Usuario}", usuario);

                // Buscar por username O email
                var user = await _context.Users
                    .Include(u => u.Organization)
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == usuario || u.Email == usuario);

                if (user == null)
                {
                    _logger.LogWarning("❌ User not found: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                if (!user.Active)
                {
                    _logger.LogWarning("❌ User inactive: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    };
                }

                _logger.LogInformation("✅ User found: {UserId}, verifying password...", user.Id);
                _logger.LogInformation("📝 Stored hash: {Hash}", user.Password);
                _logger.LogInformation("📝 Password to verify: {Password}", password);

                // VERIFICACIÓN MEJORADA DE CONTRASEÑA
                bool passwordValid = false;
                string verificationMethod = "";

                try
                {
                    // Verificar si el hash tiene formato válido
                    if (!PasswordHelper.IsValidBCryptHash(user.Password))
                    {
                        _logger.LogWarning("⚠️ Invalid BCrypt hash format in database for user: {Usuario}", usuario);
                        _logger.LogWarning("⚠️ Hash: {Hash} (Length: {Length})", user.Password, user.Password?.Length ?? 0);
                        
                        // Si el hash es inválido, generar uno nuevo y actualizar
                        await RegenerateUserPasswordHash(user, password);
                        
                        return new LoginResultDto
                        {
                            Success = false,
                            Message = "Hash de contraseña corrupto. Se ha regenerado un nuevo hash. Intente nuevamente."
                        };
                    }

                    // Verificación BCrypt normal
                    passwordValid = PasswordHelper.VerifyPassword(password, user.Password);
                    verificationMethod = "BCrypt";
                    
                    _logger.LogInformation("🔍 BCrypt verification result: {Valid} for user: {Usuario}", 
                        passwordValid, usuario);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error verifying password for user: {Usuario}", usuario);
                    
                    // Fallback verification usando BCrypt directo
                    try
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                        verificationMethod = "BCrypt Direct";
                        _logger.LogInformation("🔍 Fallback BCrypt verification result: {Valid}", passwordValid);
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "❌ Fallback password verification also failed");
                        
                        // Último intento: regenerar hash si sabemos que la contraseña debería ser correcta
                        if (usuario == "admin" && password == "admin123")
                        {
                            _logger.LogWarning("🔧 Regenerating admin password hash due to verification failure");
                            await RegenerateUserPasswordHash(user, password);
                            
                            return new LoginResultDto
                            {
                                Success = false,
                                Message = "Hash de contraseña regenerado. Intente login nuevamente."
                            };
                        }
                        
                        return new LoginResultDto
                        {
                            Success = false,
                            Message = "Error al verificar credenciales"
                        };
                    }
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("❌ Invalid password for user: {Usuario} (Method: {Method})", 
                        usuario, verificationMethod);
                    
                    // Log detallado para debug
                    _logger.LogDebug("Debug info - Password: '{Password}', Hash: '{Hash}'", 
                        password, user.Password);
                    
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Contraseña incorrecta"
                    };
                }

                // Generar JWT Token
                var token = GenerateJwtToken(user);

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Successful login for user: {Usuario} using {Method}", 
                    usuario, verificationMethod);

                return new LoginResultDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddHours(24),
                    IsOffline = false,
                    Usuario = new UsuarioDto
                    {
                        Id = user.Id, // Cédula como string
                        NombreUsuario = user.Username,
                        Correo = user.Email,
                        NombreCompleto = user.FullName ?? user.Username,
                        OrganizacionId = user.OrganizationId.ToString(),
                        RolId = user.RoleId.ToString(),
                        NombreRol = user.Role?.Name ?? "Unknown"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error during login for user: {Usuario}", usuario);
                return new LoginResultDto
                {
                    Success = false,
                    Message = $"Error en autenticación: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Regenera el hash de contraseña para un usuario
        /// </summary>
        private async Task RegenerateUserPasswordHash(Gesco.Desktop.Data.Entities.User user, string plainPassword)
        {
            try
            {
                _logger.LogInformation("🔧 Regenerating password hash for user: {UserId}", user.Id);
                
                var newHash = PasswordHelper.HashPassword(plainPassword);
                user.Password = newHash;
                user.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("✅ Password hash regenerated successfully for user: {UserId}", user.Id);
                _logger.LogInformation("📝 New hash: {Hash}", newHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error regenerating password hash for user: {UserId}", user.Id);
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
                    new Claim(ClaimTypes.NameIdentifier, user.Id), // Cédula
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("organization_id", user.OrganizationId.ToString()),
                    new Claim("role_id", user.RoleId.ToString()),
                    new Claim("cedula", user.Id) // Claim adicional para cédula
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