using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Shared.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? PurchaserName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============================================
    // USER DTOs - ✅ CORREGIDOS PARA USAR CÉDULA COMO STRING
    // ============================================
    public class UserDto
    {
        public string Id { get; set; } = string.Empty; // ✅ CORREGIDO: string para cédula
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public bool Active { get; set; }
        public Guid OrganizationId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserRequest
    {
        [Required]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty; // ✅ CORREGIDO: cédula como string
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? FullName { get; set; }
        
        [MaxLength(50)]
        public string? Phone { get; set; }
        
        [Required]
        public Guid OrganizationId { get; set; }
        
        [Required]
        public int RoleId { get; set; }
    }
}