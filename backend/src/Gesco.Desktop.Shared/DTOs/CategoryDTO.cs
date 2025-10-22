using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Gesco.Desktop.Shared.DTOs
{
    // ============================================
    // CATEGORY DTOs
    // ============================================

    public class ServiceCategoryDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceCategoryRequest
    {
        [Required(ErrorMessage = "La organización es requerida")]
        public Guid OrganizationId { get; set; }
        
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }
    }

    public class ActivityCategoryDto
    {
        public Guid Id { get; set; }
        public Guid ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public string? ServiceCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateActivityCategoryRequest
    {
        [Required(ErrorMessage = "La actividad es requerida")]
        public Guid ActivityId { get; set; }
        
        [Required(ErrorMessage = "La categoría de servicio es requerida")]
        public Guid ServiceCategoryId { get; set; }
    }

    // ============================================
    // HELPER METHODS PARA MAPEO
    // ============================================

    
}