using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    public interface ISalesTransactionService
    {
        Task<List<SalesTransactionDto>> GetSalesAsync(Guid? cashRegisterId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<SalesTransactionDto?> GetSaleByIdAsync(Guid id);
        Task<SalesTransactionDto> CreateSaleAsync(CreateSaleRequest request);
        Task<SalesTransactionDto?> UpdateSaleAsync(Guid id, CreateSaleRequest request);
        Task<bool> CancelSaleAsync(Guid id, string reason);
        Task<SalesTransactionDto?> CompleteSaleAsync(Guid id, List<CreatePaymentRequest> payments);
        Task<List<SalesTransactionDto>> GetSalesByCashRegisterAsync(Guid cashRegisterId);
        Task<SalesSummaryDto> GetSalesSummaryAsync(Guid? cashRegisterId = null, DateTime? date = null);
    }
}