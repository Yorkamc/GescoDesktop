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
        public Guid? ManagerUserId { get; set; }
        public string? ManagerName { get; set; }
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
        
        public Guid? ManagerUserId { get; set; }
        
        public Guid? OrganizationId { get; set; }
    }

    // ============================================
    // SALES DTOs - CORREGIDOS
    // ============================================
    public class SalesTransactionDto
    {
        public Guid Id { get; set; }
        public int CashRegisterId { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public int SalesStatusId { get; set; }
        public string? StatusName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TransactionDetailDto> Details { get; set; } = new();
        public List<TransactionPaymentDto> Payments { get; set; } = new();
    }

    public class TransactionDetailDto
    {
        public Guid Id { get; set; }
        public int? ProductId { get; set; }
        public int? ComboId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCombo { get; set; }
    }

    public class TransactionPaymentDto
    {
        public Guid Id { get; set; }
        public int PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    // ============================================
    // PRODUCT DTOs - CORREGIDOS
    // ============================================
    public class CategoryProductDto
    {
        public Guid Id { get; set; }
        public int ActivityCategoryId { get; set; }
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
        public int ActivityCategoryId { get; set; }
        
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
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int MovementTypeId { get; set; }
        public string? MovementTypeName { get; set; }
        public int Quantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalValue { get; set; }
        public string? Justification { get; set; }
        public DateTime MovementDate { get; set; }
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
}