using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Shared.DTOs
{
    // ============================================
    // DTOs MEJORADOS PARA PRODUCTOS POR ACTIVIDAD
    // ============================================

    /// <summary>
    /// DTO mejorado para productos con información completa de actividad y categoría
    /// </summary>
    public class CategoryProductDetailedDto
    {
        public Guid Id { get; set; }
        
        // Información de ActivityCategory
        public Guid? ActivityCategoryId { get; set; }
        public string? ActivityCategoryName { get; set; }
        
        // Información de Activity
        public Guid? ActivityId { get; set; }
        public string? ActivityName { get; set; }
        
        // Información de ServiceCategory
        public Guid? ServiceCategoryId { get; set; }
        public string? ServiceCategoryName { get; set; }
        
        // Información del producto
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int AlertQuantity { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAssignedToActivity => ActivityCategoryId.HasValue;
    }

    /// <summary>
    /// Request mejorado para crear productos usando Guid de ActivityCategory
    /// </summary>
    public class CreateProductForActivityRequest
    {
        public Guid? ActivityCategoryId { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad inicial debe ser mayor o igual a 0")]
        public int InitialQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad de alerta debe ser mayor o igual a 0")]
        public int AlertQuantity { get; set; } = 10;
    }
    public class AssignProductToActivityRequest
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid ActivityCategoryId { get; set; }
}

    /// <summary>
    /// DTO para obtener resumen de productos por actividad
    /// </summary>
    public class ActivityProductsSummaryDto
    {
        public Guid ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<CategoryProductsGroupDto> CategoriesWithProducts { get; set; } = new();
    }

    /// <summary>
    /// Productos agrupados por categoría
    /// </summary>
    public class CategoryProductsGroupDto
    {
        public Guid ActivityCategoryId { get; set; }
        public Guid ServiceCategoryId { get; set; }
        public string ServiceCategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int ActiveProductCount { get; set; }
        public List<CategoryProductDetailedDto> Products { get; set; } = new();
    }

    /// <summary>
    /// Request para asignar categorías a una actividad
    /// </summary>
    public class AssignCategoriesToActivityRequest
    {
        [Required]
        public Guid ActivityId { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "Debe seleccionar al menos una categoría")]
        public List<Guid> ServiceCategoryIds { get; set; } = new();
    }

    /// <summary>
    /// Response para operaciones batch
    /// </summary>
    public class BatchOperationResultDto
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> SuccessIds { get; set; } = new();
        public List<BatchErrorDto> Errors { get; set; } = new();
    }

    public class BatchErrorDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}