using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly LocalDbContext _context;
        private readonly string _jwtSecret;

        public AuthService(LocalDbContext context)
        {
            _context = context;
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "TuClaveSecretaMuyLargaYSegura2024GescoDesktop!@#$%";
        }

        public async Task<LoginResultDto> LoginAsync(string usuario, string password)
        {
            try
            {
                // Buscar usuario por nombre de usuario o email
                var user = await _context.Usuarios
                    .Include(u => u.Organizacion)
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuario || u.Correo == usuario);

                if (user == null || !user.Activo)
                {
                    return new LoginResultDto
                    {
                        Success = false,
                        Message = "Usuario no encontrado o inactivo"
                    };
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
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
                        NombreRol = GetRoleName(user.RolId)
                    }
                };
            }
            catch (Exception ex)
            {
                return new LoginResultDto
                {
                    Success = false,
                    Message = $"Error en autenticación: {ex.Message}"
                };
            }
        }

        public async Task LogoutAsync()
        {
            // En una implementación más completa, aquí se invalidarían los tokens
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
            // Esta implementación sería más completa con el contexto de la request HTTP
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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
