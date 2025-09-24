using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Shared.DTOs
{
    public class ActivityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public TimeOnly? StartTime { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? Location { get; set; }
        public int ActivityStatusId { get; set; } // Mantener como int para compatibilidad con frontend
        public string? StatusName { get; set; }
        public string? ManagerUserId { get; set; } // ✅ CORREGIDO: string para cédula
        public string? ManagerUserName { get; set; } // ✅ CORREGIDO: propiedad renombrada
        public Guid? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateActivityRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public DateOnly StartDate { get; set; }
        
        public TimeOnly? StartTime { get; set; }
        
        public DateOnly? EndDate { get; set; }
        
        public TimeOnly? EndTime { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        public int ActivityStatusId { get; set; } = 1; // Default to first status
        
        [MaxLength(50)]
        public string? ManagerUserId { get; set; } // ✅ CORREGIDO: string para cédula
        
        public Guid? OrganizationId { get; set; }
    }

    // ============================================
    // SALES DTOs - CORREGIDOS
    // ============================================
    public class SalesTransactionDto
    {
        public Guid Id { get; set; }
        public Guid CashRegisterId { get; set; } // CORREGIDO: Guid
        public string TransactionNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public int SalesStatusId { get; set; } // Mantener int para frontend
        public string? StatusName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TransactionDetailDto> Details { get; set; } = new();
        public List<TransactionPaymentDto> Payments { get; set; } = new();
    }

    public class TransactionDetailDto
    {
        public Guid Id { get; set; }
        public Guid? ProductId { get; set; } // CORREGIDO: Guid
        public Guid? ComboId { get; set; } // CORREGIDO: Guid
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCombo { get; set; }
    }

    public class TransactionPaymentDto
    {
        public Guid Id { get; set; }
        public int PaymentMethodId { get; set; } // Mantener int para frontend
        public string? PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string ProcessedBy { get; set; } = string.Empty; // ✅ CORREGIDO: string para cédula
        public string? ProcessedByName { get; set; } // ✅ NUEVO: nombre del usuario que procesó
    }

    // ============================================
    // PRODUCT DTOs - CORREGIDOS
    // ============================================
    public class CategoryProductDto
    {
        public Guid Id { get; set; }
        public int ActivityCategoryId { get; set; } // Mantener int para frontend
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
        public int ActivityCategoryId { get; set; } // Mantener int para frontend
        
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
    // INVENTORY DTOs - CORREGIDOS
    // ============================================
    public class InventoryMovementDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; } // CORREGIDO: Guid
        public string? ProductName { get; set; }
        public int MovementTypeId { get; set; } // Mantener int para frontend
        public string? MovementTypeName { get; set; }
        public int Quantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalValue { get; set; }
        public string? Justification { get; set; }
        public DateTime MovementDate { get; set; }
        public string? PerformedBy { get; set; } // ✅ CORREGIDO: string para cédula
        public string? PerformedByName { get; set; } // ✅ NUEVO: nombre del usuario
        public string? AuthorizedBy { get; set; } // ✅ CORREGIDO: string para cédula
        public string? AuthorizedByName { get; set; } // ✅ NUEVO: nombre del usuario
    }

    // ============================================
    // CASH REGISTER DTOs - CORREGIDOS
    // ============================================
    public class CashRegisterDto
    {
        public Guid Id { get; set; }
        public Guid ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public int RegisterNumber { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public bool IsOpen { get; set; }
        public DateTime? OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? OperatorUserId { get; set; } // ✅ CORREGIDO: string para cédula
        public string? OperatorUserName { get; set; } // ✅ NUEVO: nombre del operador
        public string? SupervisorUserId { get; set; } // ✅ CORREGIDO: string para cédula
        public string? SupervisorUserName { get; set; } // ✅ NUEVO: nombre del supervisor
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCashRegisterRequest
    {
        [Required]
        public Guid ActivityId { get; set; }
        
        [Required]
        public int RegisterNumber { get; set; }
        
        [MaxLength(100)]
        public string? Name { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        [MaxLength(50)]
        public string? OperatorUserId { get; set; } // ✅ CORREGIDO: string para cédula
        
        [MaxLength(50)]
        public string? SupervisorUserId { get; set; } // ✅ CORREGIDO: string para cédula
    }

    // ============================================
    // DASHBOARD DTOs - CORREGIDOS
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
    // USER DTOs - CORREGIDOS
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

    // ============================================
    // HELPER METHODS PARA MAPEO DE TIPOS
    // ============================================
    public static class EntityMappingHelpers
    {
        /// <summary>
        /// Convierte Guid de entidad a int para DTOs (frontend compatibility)
        /// </summary>
        public static int MapGuidToInt(Guid guid, List<dynamic> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].Id == guid)
                    return i + 1;
            }
            return 0;
        }

        /// <summary>
        /// Convierte int de DTO a Guid de entidad
        /// </summary>
        public static Guid? MapIntToGuid(int index, List<dynamic> entities)
        {
            if (index > 0 && index <= entities.Count)
                return entities[index - 1].Id;
            return null;
        }

        /// <summary>
        /// Mapea cédula string a User entity ID
        /// </summary>
        public static string? MapCedulaToUserId(string? cedula)
        {
            return string.IsNullOrEmpty(cedula) ? null : cedula.Trim();
        }

        /// <summary>
        /// Valida formato de cédula costarricense
        /// </summary>
        public static bool IsValidCedula(string? cedula)
        {
            if (string.IsNullOrEmpty(cedula)) return false;
            
            // Formato básico: 9 dígitos para físicas, 10 para jurídicas
            return cedula.Length >= 9 && cedula.Length <= 12 && cedula.All(char.IsDigit);
        }
    }
}