using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Gesco.Desktop.Shared.Converters;

namespace Gesco.Desktop.Shared.DTOs
{
    // ============================================
    // ACTIVITY DTOs
    // ============================================

    public class ActivityDto
    {
        public Guid Id { get; set; } // ✅ Guid para frontend (mapea desde long en backend)
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? Location { get; set; }
        public int ActivityStatusId { get; set; } // ✅ int para frontend (cast desde long)
        public string? StatusName { get; set; }
        public string? ManagerUserId { get; set; } // ✅ string para cédula
        public string? ManagerUserName { get; set; }
        public Guid? OrganizationId { get; set; } // ✅ Guid para Organization
        public string? OrganizationName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateActivityRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateOnly StartDate { get; set; }
        
        // ✅ Agregar JsonConverter para aceptar strings vacíos
        [JsonConverter(typeof(NullableTimeOnlyJsonConverter))]
        public TimeOnly? StartTime { get; set; }
        
        public DateOnly? EndDate { get; set; }
        
        // ✅ Agregar JsonConverter para aceptar strings vacíos
        [JsonConverter(typeof(NullableTimeOnlyJsonConverter))]
        public TimeOnly? EndTime { get; set; }
        
        [MaxLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string? Location { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "El ID de estado debe ser mayor o igual a 0")]
        public int ActivityStatusId { get; set; } = 1; // Default: Not Started
        
        [MaxLength(50, ErrorMessage = "El ID de usuario manager no puede exceder 50 caracteres")]
        public string? ManagerUserId { get; set; }
        
        public Guid? OrganizationId { get; set; }
    }

    // ============================================
    // SALES DTOs
    // ============================================

    public class SalesTransactionDto
    {
        public Guid Id { get; set; } // ✅ Guid (mapea desde long)
        public Guid CashRegisterId { get; set; } // ✅ Guid (mapea desde long)
        public string TransactionNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public int SalesStatusId { get; set; } // ✅ int para frontend
        public string? StatusName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TransactionDetailDto> Details { get; set; } = new();
        public List<TransactionPaymentDto> Payments { get; set; } = new();
    }

    public class TransactionDetailDto
    {
        public Guid Id { get; set; } // ✅ Guid (mapea desde long)
        public Guid? ProductId { get; set; } // ✅ Guid (mapea desde long)
        public Guid? ComboId { get; set; } // ✅ Guid (mapea desde long)
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCombo { get; set; }
    }

    public class TransactionPaymentDto
    {
        public Guid Id { get; set; } // ✅ Guid (mapea desde long)
        public int PaymentMethodId { get; set; } // ✅ int para frontend
        public string? PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty; // ✅ string para cédula
        public string? ProcessedByName { get; set; }
    }

    // ============================================
    // PRODUCT DTOs
    // ============================================

    public class CategoryProductDto
    {
        public Guid Id { get; set; } // ✅ Guid (mapea desde long)
        public int ActivityCategoryId { get; set; } // ✅ int para frontend (orden)
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int InitialQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public int AlertQuantity { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductRequest
    {
        public int ActivityCategoryId { get; set; } // ✅ int (orden de categoría)
        
        [MaxLength(50)]
        public string? Code { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        
        [Range(0, int.MaxValue)]
        public int InitialQuantity { get; set; }
        
        [Range(0, int.MaxValue)]
        public int AlertQuantity { get; set; } = 10;
    }

    // ============================================
    // INVENTORY DTOs
    // ============================================

    public class InventoryMovementDto
    {
        public Guid Id { get; set; } // ✅ Guid (mapea desde long)
        public Guid ProductId { get; set; } // ✅ Guid (mapea desde long)
        public string? ProductName { get; set; }
        public int MovementTypeId { get; set; } // ✅ int para frontend
        public string? MovementTypeName { get; set; }
        public int Quantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalValue { get; set; }
        public string? Justification { get; set; }
        public DateTime MovementDate { get; set; }
        public string? PerformedBy { get; set; } // ✅ string para cédula
        public string? PerformedByName { get; set; }
        public string? AuthorizedBy { get; set; } // ✅ string para cédula
        public string? AuthorizedByName { get; set; }
    }

    // ============================================
    // CASH REGISTER DTOs
    // ============================================

    public class CashRegisterDto
    {
        public Guid Id { get; set; } // ✅ Guid (mapea desde long)
        public Guid ActivityId { get; set; } // ✅ Guid (mapea desde long)
        public string? ActivityName { get; set; }
        public int RegisterNumber { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? OperatorUserId { get; set; } // ✅ string para cédula
        public string? OperatorUserName { get; set; }
        public string? SupervisorUserId { get; set; } // ✅ string para cédula
        public string? SupervisorUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    
    // ============================================
    // DASHBOARD DTOs
    // ============================================

    public class DashboardStatsDto
    {
        public int TotalActivities { get; set; }
        public int ActiveActivities { get; set; }
        public decimal TodaySales { get; set; }
        public int TodayTransactions { get; set; }
        public decimal MonthSales { get; set; }
        public int MonthTransactions { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public DateTime QueryDate { get; set; }
        public string ReportPeriod { get; set; } = string.Empty;
    }

    // ============================================
    // COMMON RESPONSE DTOs
    // ============================================

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ApiResponse : ApiResponse<object>
    {
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    // ============================================
    // LOOKUP DTOs (Para catalogos/lookups)
    // ============================================

    public class ActivityStatusDto
    {
        public int Id { get; set; } // ✅ int para frontend (se castea desde long)
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
    }

    public class SalesStatusDto
    {
        public int Id { get; set; } // ✅ int para frontend
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
    }

    public class PaymentMethodDto
    {
        public int Id { get; set; } // ✅ int para frontend
        public string Name { get; set; } = string.Empty;
        public bool RequiresReference { get; set; }
        public bool Active { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; } // ✅ int para frontend
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Active { get; set; }
    }

    // ============================================
    // HELPER METHODS PARA MAPEO
    // ============================================

    public static class EntityMappingHelpers
    {
        /// <summary>
        /// Valida formato de cédula costarricense
        /// </summary>
        public static bool IsValidCedula(string? cedula)
        {
            if (string.IsNullOrEmpty(cedula)) return false;
            
            // Formato básico: 9 dígitos para físicas, 10 para jurídicas
            return cedula.Length >= 9 && cedula.Length <= 12 && cedula.All(char.IsDigit);
        }

        /// <summary>
        /// Mapea cédula string a User entity ID
        /// </summary>
        public static string? MapCedulaToUserId(string? cedula)
        {
            return string.IsNullOrEmpty(cedula) ? null : cedula.Trim();
        }
    }
}