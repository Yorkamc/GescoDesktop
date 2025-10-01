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
                        "GescoDesktop2024SecretKey12345678901234567890";
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task<LoginResultDto> LoginAsync(string usuario, string password)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Usuario}", usuario);

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

                if (!user.Active)
                {
                    _logger.LogWarning("User inactive: {Usuario}", usuario);
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario inactivo"
                    };
                }

                bool passwordValid = false;
                try
                {
                    if (!PasswordHelper.IsValidBCryptHash(user.Password))
                    {
                        _logger.LogWarning("Invalid password hash format for user: {Usuario}", usuario);
                        return new LoginResultDto
                        {
                            Success = false,
                            Message = "Error de configuracion de usuario"
                        };
                    }

                    passwordValid = PasswordHelper.VerifyPassword(password, user.Password);
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

                var token = GenerateJwtToken(user);

                await UpdateLastLoginAsync(user.Id);

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
                _logger.LogError(ex, "Unexpected error during login for user: {Usuario}", usuario);
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
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                _tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<UsuarioDto?> GetCurrentUserAsync()
        {
            return Task.FromResult<UsuarioDto?>(null);
        }

        private string GenerateJwtToken(Gesco.Desktop.Data.Entities.User user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("organization_id", user.OrganizationId.ToString()),
                    new Claim("role_id", user.RoleId.ToString()),
                    new Claim("cedula", user.Id)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        private async Task UpdateLastLoginAsync(string userId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE users SET last_login_at = {0}, updated_at = {1} WHERE id = {2}",
                    DateTime.UtcNow, DateTime.UtcNow, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update last login for user: {UserId}", userId);
            }
        }
    }
}