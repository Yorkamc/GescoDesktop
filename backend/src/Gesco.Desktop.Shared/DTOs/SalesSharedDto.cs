using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Shared.DTOs
{
    // ============================================
    // SALES REQUEST DTOs - ACTUALIZADO CON SOPORTE PARA COMBOS
    // ============================================
    
    public class CreateSaleRequest
    {
        [Required]
        public Guid CashRegisterId { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto o combo")]
        public List<CreateSaleItemRequest> Items { get; set; } = new();
        
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CreateSaleItemRequest
    {
        // ✅ ProductId ahora es opcional (puede ser null si es combo)
        public Guid? ProductId { get; set; }
        
        // ✅ ComboId ahora es opcional (puede ser null si es producto)
        public Guid? ComboId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }
        
        /// <summary>
        /// Validar que tenga ProductId O ComboId, pero no ambos
        /// </summary>
        public bool IsValid()
        {
            // Debe tener al menos uno
            if (!ProductId.HasValue && !ComboId.HasValue)
                return false;
            
            // No puede tener ambos
            if (ProductId.HasValue && ComboId.HasValue)
                return false;
            
            return true;
        }
    }

    public class CreatePaymentRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un método de pago válido")]
        public int PaymentMethodId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
        
        [MaxLength(100)]
        public string? Reference { get; set; }
        
        public string ProcessedBy { get; set; } = string.Empty;
    }

    // ============================================
    // SALES SUMMARY DTOs
    // ============================================

    public class SalesSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageTransaction { get; set; }
        public int TotalItemsSold { get; set; }
    }

    // ============================================
    // CASH REGISTER DTOs
    // ============================================

    public class CreateCashRegisterRequest
    {
        [Required]
        public Guid ActivityId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int RegisterNumber { get; set; }
        
        [MaxLength(100)]
        public string? Name { get; set; }
        
        [MaxLength(200)]
        public string? Location { get; set; }
        
        [MaxLength(50)]
        public string? OperatorUserId { get; set; }
        
        [MaxLength(50)]
        public string? SupervisorUserId { get; set; }
    }

    public class CloseCashRegisterRequest
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal CashDeclared { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ClosedBy { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? SupervisedBy { get; set; }
        
        [MaxLength(500)]
        public string? Observations { get; set; }
    }

    public class CashRegisterClosureDto
    {
        public Guid Id { get; set; }
        public Guid CashRegisterId { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal CashCalculated { get; set; }
        public decimal CardsCalculated { get; set; }
        public decimal SinpeCalculated { get; set; }
        public decimal? CashDeclared { get; set; }
        public decimal? CashDifference { get; set; }
        public string? ClosedBy { get; set; }
        public string? ClosedByName { get; set; }
        public string? SupervisedBy { get; set; }
        public string? SupervisedByName { get; set; }
        public string? Observations { get; set; }
    }

    // ============================================
    // SALES COMBO DTOs
    // ============================================
    
    public class SalesComboDto
    {
        public Guid Id { get; set; }
        public Guid ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal ComboPrice { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ComboItemDto> Items { get; set; } = new();
    }

    public class ComboItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateSalesComboRequest
    {
        [Required]
        public Guid ActivityId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio del combo debe ser mayor a 0")]
        public decimal ComboPrice { get; set; }
        
        [Required]
        [MinLength(2, ErrorMessage = "Un combo debe tener al menos 2 productos")]
        public List<CreateComboItemRequest> Items { get; set; } = new();
    }

    public class CreateComboItemRequest
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Quantity { get; set; } = 1;
    }
}