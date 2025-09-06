using System;
using System.Collections.Generic;
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
    // USER DTOs
    // ============================================
    public class UserDto
    {
        public Guid Id { get; set; }
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
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public Guid OrganizationId { get; set; }
        public int RoleId { get; set; }
    }
}