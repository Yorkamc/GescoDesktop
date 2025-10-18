using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public AuthService(LocalDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                        "TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345";
            _tokenHandler = new JwtSecurityTokenHandler();
            
            _logger.LogInformation("AuthService initialized with JWT secret length: {Length}", _jwtSecret.Length);
        }

        public async Task<LoginResultDto> LoginAsync(string usuario, string password)
        {
            try
            {
                _logger.LogInformation("=== LOGIN ATTEMPT START ===");
                _logger.LogInformation("Login attempt for user: {Usuario}", usuario);

                // 1. BUSCAR USUARIO
                var user = await _context.Users
                    .Include(u => u.Organization)
                    .Include(u => u.Role)
                    .AsNoTracking()
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

                _logger.LogInformation("User found - ID: {Id}, Username: {Username}, Active: {Active}", 
                    user.Id, user.Username, user.Active);

                // 2. VERIFICAR QUE ESTÉ ACTIVO
                if (!user.Active)
                {
                    _logger.LogWarning("User inactive: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    };
                }

                // 3. VERIFICAR PASSWORD
                _logger.LogInformation("Verifying password for user: {Usuario}", usuario);
                _logger.LogInformation("Password hash in DB: {Hash}", user.Password.Substring(0, Math.Min(20, user.Password.Length)) + "...");
                
                bool passwordValid = false;
                try
                {
                    if (!PasswordHelper.IsValidBCryptHash(user.Password))
                    {
                        _logger.LogError("INVALID PASSWORD HASH FORMAT for user: {Usuario}", usuario);
                        _logger.LogError("Hash: {Hash}", user.Password);
                        return new LoginResultDto
                        {
                            Success = false,
                            Message = "Error de configuracion de usuario - contacte al administrador"
                        };
                    }

                    passwordValid = PasswordHelper.VerifyPassword(password, user.Password);
                    _logger.LogInformation("Password verification result: {Result}", passwordValid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Password verification error for user: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Error al verificar credenciales"
                    };
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password for user: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Contrasena incorrecta"
                    };
                }

                // 4. GENERAR TOKEN JWT
                _logger.LogInformation("Generating JWT token for user: {Usuario}", usuario);
                var token = GenerateJwtToken(user);
                _logger.LogInformation("JWT Token generated successfully. Length: {Length}", token.Length);
                _logger.LogInformation("Token preview: {Token}", token.Substring(0, Math.Min(50, token.Length)) + "...");

                // 5. ACTUALIZAR LAST LOGIN
                await UpdateLastLoginAsync(user.Id);

                _logger.LogInformation("=== LOGIN SUCCESS ===");
                _logger.LogInformation("User: {Username}, Role: {Role}, Organization: {Org}", 
                    user.Username, user.Role?.Name, user.Organization?.Name);

                return new LoginResultDto
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddHours(24),
                    IsOffline = false,
                    Usuario = new UsuarioDto
                    {
                        Id = user.Id,
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
                _logger.LogError(ex, "=== UNEXPECTED ERROR DURING LOGIN ===");
                _logger.LogError("User: {Usuario}", usuario);
                return new LoginResultDto
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Validating token...");
                
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                _logger.LogInformation("Token validation successful");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return Task.FromResult(false);
            }
        }

        public async Task<UsuarioDto?> GetCurrentUserAsync()
        {
            // Este método necesitaría el HttpContext para obtener el usuario actual
            // Por ahora retornamos null
            await Task.CompletedTask;
            return null;
        }

        private string GenerateJwtToken(Gesco.Desktop.Data.Entities.User user)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                
                _logger.LogInformation("Creating JWT token with claims:");
                _logger.LogInformation("- NameIdentifier: {Id}", user.Id);
                _logger.LogInformation("- Name: {Username}", user.Username);
                _logger.LogInformation("- Email: {Email}", user.Email);
                _logger.LogInformation("- organization_id: {OrgId}", user.OrganizationId);
                _logger.LogInformation("- role_id: {RoleId}", user.RoleId);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("organization_id", user.OrganizationId.ToString()),
                    new Claim("role_id", user.RoleId.ToString()),
                    new Claim("cedula", user.Id),
                    new Claim("sub", user.Id), // Subject claim - standard JWT claim
                    new Claim("username", user.Username)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key), 
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = _tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = _tokenHandler.WriteToken(token);
                
                _logger.LogInformation("Token created successfully. Expires: {Expiration}", tokenDescriptor.Expires);
                
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
                throw;
            }
        }

        private async Task UpdateLastLoginAsync(string userId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE users SET last_login_at = {0}, updated_at = {1} WHERE id = {2}",
                    DateTime.UtcNow, DateTime.UtcNow, userId);
                
                _logger.LogInformation("Last login updated for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update last login for user: {UserId}", userId);
            }
        }
    }
}