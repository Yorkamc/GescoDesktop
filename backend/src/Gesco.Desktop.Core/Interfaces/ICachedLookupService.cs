using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities;

namespace Gesco.Desktop.Core.Interfaces
{
   public interface ICachedLookupService
    {
        Task<ActivityStatus?> GetActivityStatusByNameAsync(string name);
        Task<ActivityStatus?> GetActivityStatusByIdAsync(int id);
        Task<SalesStatus?> GetSalesStatusByNameAsync(string name);
        Task<SalesStatus?> GetSalesStatusByIdAsync(int id);
        Task<PaymentMethod?> GetPaymentMethodByIdAsync(int id);
        Task<List<ActivityStatus>> GetAllActivityStatusesAsync();
        Task<List<SalesStatus>> GetAllSalesStatusesAsync();
        Task<List<PaymentMethod>> GetAllPaymentMethodsAsync();
        void ClearCache();
    }

}