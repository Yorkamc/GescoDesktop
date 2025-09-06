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
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .AsQueryable();

                if (organizationId.HasValue)
                {
                    query = query.Where(a => a.OrganizationId == organizationId);
                }

                var activities = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = a.ActivityStatusId,
                        StatusName = a.ActivityStatus.Name,
                        ManagerUserId = a.ManagerUserId,
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
                var activity = await _context.Activities
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (activity == null)
                {
                    return null;
                }

                return new ActivityDto
                {
                    Id = activity.Id,
                    Name = activity.Name,
                    Description = activity.Description,
                    StartDate = activity.StartDate,
                    StartTime = activity.StartTime,
                    EndDate = activity.EndDate,
                    EndTime = activity.EndTime,
                    Location = activity.Location,
                    ActivityStatusId = activity.ActivityStatusId,
                    StatusName = activity.ActivityStatus.Name,
                    ManagerUserId = activity.ManagerUserId,
                    OrganizationId = activity.OrganizationId,
                    OrganizationName = activity.Organization?.Name,
                    CreatedAt = activity.CreatedAt
                };
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
                var activity = new Activity
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    StartTime = request.StartTime,
                    EndDate = request.EndDate,
                    EndTime = request.EndTime,
                    Location = request.Location,
                    ActivityStatusId = request.ActivityStatusId,
                    ManagerUserId = request.ManagerUserId,
                    OrganizationId = request.OrganizationId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new activity: {ActivityName} with ID {ActivityId}", 
                    activity.Name, activity.Id);

                // Return the created activity with includes
                return await GetActivityByIdAsync(activity.Id) ?? 
                    throw new InvalidOperationException("Failed to retrieve created activity");
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
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    return null;
                }

                activity.Name = request.Name;
                activity.Description = request.Description;
                activity.StartDate = request.StartDate;
                activity.StartTime = request.StartTime;
                activity.EndDate = request.EndDate;
                activity.EndTime = request.EndTime;
                activity.Location = request.Location;
                activity.ActivityStatusId = request.ActivityStatusId;
                activity.ManagerUserId = request.ManagerUserId;
                activity.OrganizationId = request.OrganizationId;
                activity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated activity: {ActivityId}", id);

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
                var activity = await _context.Activities.FindAsync(id);
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
                // Assuming status ID 2 is "In Progress" based on your seed data
                var activeStatusIds = await _context.ActivityStatuses
                    .Where(s => s.Name == "In Progress" || s.Name == "Not Started")
                    .Select(s => s.Id)
                    .ToListAsync();

                var activities = await _context.Activities
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Where(a => activeStatusIds.Contains(a.ActivityStatusId))
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = a.ActivityStatusId,
                        StatusName = a.ActivityStatus.Name,
                        ManagerUserId = a.ManagerUserId,
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

                // Get status IDs
                var inProgressStatusId = await _context.ActivityStatuses
                    .Where(s => s.Name == "In Progress")
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                var completedStatusId = await _context.SalesStatuses
                    .Where(s => s.Name == "Completed")
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();

                var stats = new DashboardStatsDto
                {
                    TotalActivities = await _context.Activities.CountAsync(),
                    ActiveActivities = await _context.Activities
                        .CountAsync(a => a.ActivityStatusId == inProgressStatusId),
                    
                    TodaySales = await _context.SalesTransactions
                        .Where(t => t.TransactionDate.Date == today && 
                                   t.SalesStatusId == completedStatusId)
                        .SumAsync(t => (decimal?)t.TotalAmount) ?? 0m,
                    
                    TodayTransactions = await _context.SalesTransactions
                        .CountAsync(t => t.TransactionDate.Date == today),
                    
                    MonthSales = await _context.SalesTransactions
                        .Where(t => t.TransactionDate >= thisMonth && 
                                   t.SalesStatusId == completedStatusId)
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
                    ReportPeriod = $"Día {today:dd/MM/yyyy} y mes {thisMonth:MM/yyyy}"
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating activity stats");
                throw;
            }
        }
    }
}