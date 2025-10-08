using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class ActivityService : IActivityService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(LocalDbContext context, ILogger<ActivityService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ActivityDto>> GetActivitiesAsync(Guid? organizationId = null)
        {
            try
            {
                var query = _context.Activities
                    .AsNoTracking()
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Include(a => a.ManagerUser)
                    .AsQueryable();

                if (organizationId.HasValue)
                {
                    query = query.Where(a => a.OrganizationId == organizationId);
                }

                var activities = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new ActivityDto
                    {
                        Id = MapLongToGuid(a.Id),
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = (int)a.ActivityStatusId,
                        StatusName = a.ActivityStatus != null ? a.ActivityStatus.Name : null,
                        ManagerUserId = a.ManagerUserId,
                        ManagerUserName = a.ManagerUser != null 
                            ? a.ManagerUser.FullName ?? a.ManagerUser.Username 
                            : null,
                        OrganizationId = a.OrganizationId,
                        OrganizationName = a.Organization != null ? a.Organization.Name : null,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} activities", activities.Count);
                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities");
                throw;
            }
        }

        public async Task<ActivityDto?> GetActivityByIdAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                
                var activity = await _context.Activities
                    .AsNoTracking()
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Include(a => a.ManagerUser)
                    .Where(a => a.Id == longId)
                    .Select(a => new ActivityDto
                    {
                        Id = id,
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = (int)a.ActivityStatusId,
                        StatusName = a.ActivityStatus != null ? a.ActivityStatus.Name : null,
                        ManagerUserId = a.ManagerUserId,
                        ManagerUserName = a.ManagerUser != null 
                            ? a.ManagerUser.FullName ?? a.ManagerUser.Username 
                            : null,
                        OrganizationId = a.OrganizationId,
                        OrganizationName = a.Organization != null ? a.Organization.Name : null,
                        CreatedAt = a.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity {ActivityId}", id);
                throw;
            }
        }

        public async Task<ActivityDto> CreateActivityAsync(CreateActivityRequest request)
        {
            try
            {
                var defaultStatus = await _context.ActivityStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name == "Not Started");
                
                if (defaultStatus == null)
                {
                    defaultStatus = await _context.ActivityStatuses
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }

                if (!string.IsNullOrEmpty(request.ManagerUserId))
                {
                    var managerExists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == request.ManagerUserId && u.Active);
                    
                    if (!managerExists)
                    {
                        _logger.LogWarning("Manager user with cedula {ManagerId} not found or inactive", request.ManagerUserId);
                        throw new ArgumentException($"Usuario manager con cedula {request.ManagerUserId} no encontrado o inactivo");
                    }
                }

                var activity = new Activity
                {
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    StartTime = request.StartTime,
                    EndDate = request.EndDate,
                    EndTime = request.EndTime,
                    Location = request.Location,
                    ActivityStatusId = defaultStatus?.Id ?? 1L,
                    ManagerUserId = request.ManagerUserId,
                    OrganizationId = request.OrganizationId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ManagerUserId
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new activity: {ActivityName} with ID {ActivityId}, Manager: {ManagerId}", 
                    activity.Name, activity.Id, request.ManagerUserId);

                var responseGuid = MapLongToGuid(activity.Id);
                
                string? managerUserName = null;
                if (!string.IsNullOrEmpty(request.ManagerUserId))
                {
                    var manager = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == request.ManagerUserId)
                        .Select(u => u.FullName ?? u.Username)
                        .FirstOrDefaultAsync();
                    managerUserName = manager;
                }
                
                return new ActivityDto
                {
                    Id = responseGuid,
                    Name = activity.Name,
                    Description = activity.Description,
                    StartDate = activity.StartDate,
                    StartTime = activity.StartTime,
                    EndDate = activity.EndDate,
                    EndTime = activity.EndTime,
                    Location = activity.Location,
                    ActivityStatusId = (int)activity.ActivityStatusId,
                    StatusName = defaultStatus?.Name,
                    ManagerUserId = activity.ManagerUserId,
                    ManagerUserName = managerUserName,
                    OrganizationId = activity.OrganizationId,
                    CreatedAt = activity.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                throw;
            }
        }

        public async Task<ActivityDto?> UpdateActivityAsync(Guid id, CreateActivityRequest request)
        {
            try
            {
                var longId = MapGuidToLong(id);
                
                var activity = await _context.Activities.FindAsync(longId);
                if (activity == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(request.ManagerUserId) && request.ManagerUserId != activity.ManagerUserId)
                {
                    var managerExists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == request.ManagerUserId && u.Active);
                    
                    if (!managerExists)
                    {
                        _logger.LogWarning("Manager user with cedula {ManagerId} not found or inactive", request.ManagerUserId);
                        throw new ArgumentException($"Usuario manager con cedula {request.ManagerUserId} no encontrado o inactivo");
                    }
                }

                activity.Name = request.Name;
                activity.Description = request.Description;
                activity.StartDate = request.StartDate;
                activity.StartTime = request.StartTime;
                activity.EndDate = request.EndDate;
                activity.EndTime = request.EndTime;
                activity.Location = request.Location;
                
                if (request.ActivityStatusId > 0)
                {
                    var status = await _context.ActivityStatuses.FindAsync((long)request.ActivityStatusId);
                    if (status != null)
                    {
                        activity.ActivityStatusId = status.Id;
                    }
                }

                activity.ManagerUserId = request.ManagerUserId;
                activity.OrganizationId = request.OrganizationId;
                activity.UpdatedAt = DateTime.UtcNow;
                activity.UpdatedBy = request.ManagerUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated activity: {ActivityId}, New Manager: {ManagerId}", id, request.ManagerUserId);

                return await GetActivityByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity {ActivityId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteActivityAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                
                var activity = await _context.Activities.FindAsync(longId);
                if (activity == null)
                {
                    return false;
                }

                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted activity: {ActivityId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity {ActivityId}", id);
                throw;
            }
        }

        public async Task<List<ActivityDto>> GetActiveActivitiesAsync()
        {
            try
            {
                var activeStatuses = await _context.ActivityStatuses
                    .AsNoTracking()
                    .Where(s => s.Name == "In Progress" || s.Name == "Not Started")
                    .Select(s => s.Id)
                    .ToListAsync();

                var activities = await _context.Activities
                    .AsNoTracking()
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Include(a => a.ManagerUser)
                    .Where(a => activeStatuses.Contains(a.ActivityStatusId))
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new ActivityDto
                    {
                        Id = MapLongToGuid(a.Id),
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = (int)a.ActivityStatusId,
                        StatusName = a.ActivityStatus != null ? a.ActivityStatus.Name : null,
                        ManagerUserId = a.ManagerUserId,
                        ManagerUserName = a.ManagerUser != null 
                            ? a.ManagerUser.FullName ?? a.ManagerUser.Username 
                            : null,
                        OrganizationId = a.OrganizationId,
                        OrganizationName = a.Organization != null ? a.Organization.Name : null,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active activities");
                throw;
            }
        }

        public async Task<DashboardStatsDto> GetActivityStatsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var inProgressStatus = await _context.ActivityStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name == "In Progress");

                var completedSalesStatus = await _context.SalesStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Name == "Completed");

                var stats = new DashboardStatsDto
                {
                    TotalActivities = await _context.Activities.CountAsync(),
                    ActiveActivities = inProgressStatus != null 
                        ? await _context.Activities.CountAsync(a => a.ActivityStatusId == inProgressStatus.Id)
                        : 0,
                    
                    TodaySales = completedSalesStatus != null
                        ? await _context.SalesTransactions
                            .Where(t => t.TransactionDate.Date == today && t.SalesStatusId == completedSalesStatus.Id)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m
                        : await _context.SalesTransactions
                            .Where(t => t.TransactionDate.Date == today)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m,
                    
                    TodayTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate.Date == today),
                    
                    MonthSales = completedSalesStatus != null
                        ? await _context.SalesTransactions
                            .Where(t => t.TransactionDate >= thisMonth && t.SalesStatusId == completedSalesStatus.Id)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m
                        : await _context.SalesTransactions
                            .Where(t => t.TransactionDate >= thisMonth)
                            .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m,
                    
                    MonthTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate >= thisMonth),
                    
                    TotalUsers = await _context.Users.CountAsync(),
                    ActiveUsers = await _context.Users.CountAsync(u => u.Active),
                    
                    TotalProducts = await _context.CategoryProducts.CountAsync(),
                    ActiveProducts = await _context.CategoryProducts.CountAsync(p => p.Active),
                    LowStockProducts = await _context.CategoryProducts
                        .CountAsync(p => p.CurrentQuantity <= p.AlertQuantity && p.Active),
                    
                    QueryDate = DateTime.UtcNow,
                    ReportPeriod = $"Dia {today:dd/MM/yyyy} y mes {thisMonth:MM/yyyy}"
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating activity stats");
                throw;
            }
        }

        // ============================================
        // MÉTODOS HELPER PARA MAPEO LONG <-> GUID
        // ============================================

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, Math.Min(8, longBytes.Length));
            
            // Llenar resto con patrón determinístico
            for (int i = 8; i < 16; i++)
            {
                bytes[i] = (byte)((longId >> ((i - 8) * 8)) % 256);
            }
            
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}