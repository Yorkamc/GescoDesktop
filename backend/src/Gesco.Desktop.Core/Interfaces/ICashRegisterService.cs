using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
   public interface ICashRegisterService
    {
        Task<List<CashRegisterDto>> GetCashRegistersAsync(Guid? activityId = null);
        Task<CashRegisterDto?> GetCashRegisterByIdAsync(Guid id);
        Task<CashRegisterDto> CreateCashRegisterAsync(CreateCashRegisterRequest request);
        Task<CashRegisterDto?> UpdateCashRegisterAsync(Guid id, CreateCashRegisterRequest request);
        Task<bool> DeleteCashRegisterAsync(Guid id);
        Task<CashRegisterDto?> OpenCashRegisterAsync(Guid id, string operatorUserId);
        Task<CashRegisterDto?> CloseCashRegisterAsync(Guid id, CloseCashRegisterRequest request);
        Task<List<CashRegisterDto>> GetOpenCashRegistersAsync();
        Task<CashRegisterClosureDto?> GetLastClosureAsync(Guid cashRegisterId);
    }
}