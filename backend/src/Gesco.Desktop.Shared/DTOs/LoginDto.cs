using System;

namespace Gesco.Desktop.Shared.DTOs
{
   public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UsuarioDto? Usuario { get; set; }
        public string? Token { get; set; }
        public bool IsOffline { get; set; }
        public DateTime? TokenExpiration { get; set; }
    }

    public class UsuarioDto
    {
        public string Id { get; set; } = string.Empty; // Changed to string to support Guid
        public string? NombreUsuario { get; set; }
        public string? Correo { get; set; }
        public string? NombreCompleto { get; set; }
        public string? OrganizacionId { get; set; } // Changed to string to support Guid
        public string RolId { get; set; } = string.Empty; // Changed to string to support Guid
        public string? NombreRol { get; set; }
    }

    public class LoginRequest
    {
        public string? Usuario { get; set; }
        public string? Password { get; set; }
    }
}
