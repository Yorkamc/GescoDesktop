using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{

    public class CachedLookupService : ICachedLookupService
    {
        private readonly LocalDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedLookupService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        private const string ActivityStatusPrefix = "ActivityStatus_";
        private const string SalesStatusPrefix = "SalesStatus_";
        private const string PaymentMethodPrefix = "PaymentMethod_";
        private const string AllActivityStatusesKey = "AllActivityStatuses";
        private const string AllSalesStatusesKey = "AllSalesStatuses";
        private const string AllPaymentMethodsKey = "AllPaymentMethods";

        public CachedLookupService(
            LocalDbContext context, 
            IMemoryCache cache,
            ILogger<CachedLookupService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ActivityStatus?> GetActivityStatusByNameAsync(string name)
        {
            var cacheKey = $"{ActivityStatusPrefix}Name_{name}";
            
            if (!_cache.TryGetValue(cacheKey, out ActivityStatus? status))
            {
                _logger.LogDebug("Cache miss for ActivityStatus name: {Name}", name);
                
                status = await _context.ActivityStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name == name);
                
                if (status != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration);
                    
                    _cache.Set(cacheKey, status, cacheOptions);
                    _logger.LogDebug("Cached ActivityStatus: {Name}", name);
                }
            }
            
            return status;
        }

        public async Task<ActivityStatus?> GetActivityStatusByIdAsync(int id)
        {
            var cacheKey = $"{ActivityStatusPrefix}Id_{id}";
            
            if (!_cache.TryGetValue(cacheKey, out ActivityStatus? status))
            {
                _logger.LogDebug("Cache miss for ActivityStatus id: {Id}", id);
                
                status = await _context.ActivityStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (status != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration);
                    
                    _cache.Set(cacheKey, status, cacheOptions);
                    _logger.LogDebug("Cached ActivityStatus: {Id}", id);
                }
            }
            
            return status;
        }

        public async Task<SalesStatus?> GetSalesStatusByNameAsync(string name)
        {
            var cacheKey = $"{SalesStatusPrefix}Name_{name}";
            
            if (!_cache.TryGetValue(cacheKey, out SalesStatus? status))
            {
                _logger.LogDebug("Cache miss for SalesStatus name: {Name}", name);
                
                status = await _context.SalesStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name == name);
                
                if (status != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration);
                    
                    _cache.Set(cacheKey, status, cacheOptions);
                    _logger.LogDebug("Cached SalesStatus: {Name}", name);
                }
            }
            
            return status;
        }

        public async Task<SalesStatus?> GetSalesStatusByIdAsync(int id)
        {
            var cacheKey = $"{SalesStatusPrefix}Id_{id}";
            
            if (!_cache.TryGetValue(cacheKey, out SalesStatus? status))
            {
                _logger.LogDebug("Cache miss for SalesStatus id: {Id}", id);
                
                status = await _context.SalesStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (status != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration);
                    
                    _cache.Set(cacheKey, status, cacheOptions);
                    _logger.LogDebug("Cached SalesStatus: {Id}", id);
                }
            }
            
            return status;
        }

        public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(int id)
        {
            var cacheKey = $"{PaymentMethodPrefix}Id_{id}";
            
            if (!_cache.TryGetValue(cacheKey, out PaymentMethod? method))
            {
                _logger.LogDebug("Cache miss for PaymentMethod id: {Id}", id);
                
                method = await _context.PaymentMethods
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);
                
                if (method != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheDuration);
                    
                    _cache.Set(cacheKey, method, cacheOptions);
                    _logger.LogDebug("Cached PaymentMethod: {Id}", id);
                }
            }
            
            return method;
        }

        public async Task<List<ActivityStatus>> GetAllActivityStatusesAsync()
        {
            if (!_cache.TryGetValue(AllActivityStatusesKey, out List<ActivityStatus>? statuses))
            {
                _logger.LogDebug("Cache miss for all ActivityStatuses");
                
                statuses = await _context.ActivityStatuses
                    .AsNoTracking()
                    .Where(s => s.Active)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheDuration);
                
                _cache.Set(AllActivityStatusesKey, statuses, cacheOptions);
                _logger.LogDebug("Cached {Count} ActivityStatuses", statuses.Count);
            }
            
            return statuses ?? new List<ActivityStatus>();
        }

        public async Task<List<SalesStatus>> GetAllSalesStatusesAsync()
        {
            if (!_cache.TryGetValue(AllSalesStatusesKey, out List<SalesStatus>? statuses))
            {
                _logger.LogDebug("Cache miss for all SalesStatuses");
                
                statuses = await _context.SalesStatuses
                    .AsNoTracking()
                    .Where(s => s.Active)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheDuration);
                
                _cache.Set(AllSalesStatusesKey, statuses, cacheOptions);
                _logger.LogDebug("Cached {Count} SalesStatuses", statuses.Count);
            }
            
            return statuses ?? new List<SalesStatus>();
        }

        public async Task<List<PaymentMethod>> GetAllPaymentMethodsAsync()
        {
            if (!_cache.TryGetValue(AllPaymentMethodsKey, out List<PaymentMethod>? methods))
            {
                _logger.LogDebug("Cache miss for all PaymentMethods");
                
                methods = await _context.PaymentMethods
                    .AsNoTracking()
                    .Where(m => m.Active)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheDuration);
                
                _cache.Set(AllPaymentMethodsKey, methods, cacheOptions);
                _logger.LogDebug("Cached {Count} PaymentMethods", methods.Count);
            }
            
            return methods ?? new List<PaymentMethod>();
        }

        public void ClearCache()
        {
            _logger.LogInformation("Clearing all lookup caches");
            
            _cache.Remove(AllActivityStatusesKey);
            _cache.Remove(AllSalesStatusesKey);
            _cache.Remove(AllPaymentMethodsKey);
        }
    }
}