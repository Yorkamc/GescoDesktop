using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Shared.DTOs
{
  public class CreateSaleRequest
    {
        [Required]
        public Guid CashRegisterId { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CreateSaleItemRequest> Items { get; set; } = new();
        
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CreateSaleItemRequest
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }
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
    // CASH REGISTER DTOs (si no están ya definidos)
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
}