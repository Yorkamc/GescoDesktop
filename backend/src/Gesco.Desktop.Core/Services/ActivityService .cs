using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                _logger.LogInformation("Getting activities for organization: {OrgId}", organizationId);
                
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
                _logger.LogInformation("Getting activity by ID. Guid: {Guid}, Long: {Long}", id, longId);
                
                var activity = await _context.Activities
                    .AsNoTracking()
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Include(a => a.ManagerUser)
                    .Where(a => a.Id == longId)
                    .Select(a => new ActivityDto
                    {
                        Id = MapLongToGuid(a.Id), // Regenerar desde BD para consistencia
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

                if (activity == null)
                {
                    _logger.LogWarning("Activity not found: {ActivityId}", id);
                }

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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== CREATE ACTIVITY START ===");
                _logger.LogInformation("Request: {@Request}", new
                {
                    request.Name,
                    request.StartDate,
                    request.ActivityStatusId,
                    request.ManagerUserId,
                    request.OrganizationId
                });

                // 1. VALIDAR STATUS
                long statusId = 1L; // Por defecto "Not Started"
                
                if (request.ActivityStatusId > 0)
                {
                    var statusExists = await _context.ActivityStatuses
                        .AsNoTracking()
                        .AnyAsync(s => s.Id == (long)request.ActivityStatusId && s.Active);
                    
                    if (statusExists)
                    {
                        statusId = (long)request.ActivityStatusId;
                        _logger.LogInformation("Using ActivityStatus: {StatusId}", statusId);
                    }
                    else
                    {
                        _logger.LogWarning("Activity status {StatusId} not found or inactive, using default (1)", 
                            request.ActivityStatusId);
                    }
                }

                // 2. VALIDAR MANAGER USER (OPCIONAL)
                if (!string.IsNullOrWhiteSpace(request.ManagerUserId))
                {
                    var managerExists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == request.ManagerUserId.Trim() && u.Active);

                    if (!managerExists)
                    {
                        _logger.LogError("Manager user not found or inactive: {ManagerId}", request.ManagerUserId);
                        throw new ArgumentException($"Usuario manager con cédula {request.ManagerUserId} no encontrado o inactivo");
                    }
                    
                    _logger.LogInformation("Manager user validated: {ManagerId}", request.ManagerUserId);
                }
                else
                {
                    _logger.LogInformation("No manager user specified");
                }

                // 3. VALIDAR ORGANIZATION (OPCIONAL)
                if (request.OrganizationId.HasValue && request.OrganizationId.Value != Guid.Empty)
                {
                    var orgExists = await _context.Organizations
                        .AsNoTracking()
                        .AnyAsync(o => o.Id == request.OrganizationId.Value && o.Active);

                    if (!orgExists)
                    {
                        _logger.LogError("Organization not found: {OrgId}", request.OrganizationId.Value);
                        throw new ArgumentException($"Organización {request.OrganizationId.Value} no encontrada");
                    }
                    
                    _logger.LogInformation("Organization validated: {OrgId}", request.OrganizationId.Value);
                }
                else
                {
                    _logger.LogInformation("No organization specified");
                }

                // 4. CREAR ENTIDAD
                var activity = new Activity
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    StartDate = request.StartDate,
                    StartTime = request.StartTime,
                    EndDate = request.EndDate,
                    EndTime = request.EndTime,
                    Location = request.Location?.Trim(),
                    ActivityStatusId = statusId,
                    ManagerUserId = string.IsNullOrWhiteSpace(request.ManagerUserId) 
                        ? null 
                        : request.ManagerUserId.Trim(),
                    OrganizationId = (!request.OrganizationId.HasValue || request.OrganizationId.Value == Guid.Empty)
                        ? null 
                        : request.OrganizationId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ManagerUserId?.Trim(),
                    SyncVersion = 1,
                    SyncStatus = "pending"
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Activity entity saved with ID: {ActivityId}", activity.Id);

                await transaction.CommitAsync();

                // 5. RECARGAR CON INCLUDES
                var createdActivity = await _context.Activities
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Include(a => a.ManagerUser)
                    .FirstOrDefaultAsync(a => a.Id == activity.Id);

                if (createdActivity == null)
                {
                    _logger.LogError("Failed to reload created activity");
                    throw new InvalidOperationException("Error retrieving created activity");
                }

                // 6. MAPEAR A DTO
                var dto = new ActivityDto
                {
                    Id = MapLongToGuid(createdActivity.Id),
                    Name = createdActivity.Name,
                    Description = createdActivity.Description,
                    StartDate = createdActivity.StartDate,
                    StartTime = createdActivity.StartTime,
                    EndDate = createdActivity.EndDate,
                    EndTime = createdActivity.EndTime,
                    Location = createdActivity.Location,
                    ActivityStatusId = (int)createdActivity.ActivityStatusId,
                    StatusName = createdActivity.ActivityStatus?.Name,
                    ManagerUserId = createdActivity.ManagerUserId,
                    ManagerUserName = createdActivity.ManagerUser?.FullName ?? createdActivity.ManagerUser?.Username,
                    OrganizationId = createdActivity.OrganizationId,
                    OrganizationName = createdActivity.Organization?.Name,
                    CreatedAt = createdActivity.CreatedAt
                };

                _logger.LogInformation("=== CREATE ACTIVITY SUCCESS ===");
                _logger.LogInformation("Created activity: {Id} - {Name}", dto.Id, dto.Name);

                return dto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "=== CREATE ACTIVITY FAILED ===");
                _logger.LogError("Request: {Name}", request.Name);
                throw;
            }
        }

        public async Task<ActivityDto?> UpdateActivityAsync(Guid id, CreateActivityRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                
                _logger.LogInformation("Updating activity {ActivityId}: {Name}", id, request.Name);

                var activity = await _context.Activities.FindAsync(longId);
                if (activity == null)
                {
                    _logger.LogWarning("Activity not found: {ActivityId}", id);
                    return null;
                }

                // VALIDAR MANAGER SI CAMBIÓ
                if (!string.IsNullOrWhiteSpace(request.ManagerUserId) && 
                    request.ManagerUserId.Trim() != activity.ManagerUserId)
                {
                    var managerExists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == request.ManagerUserId.Trim() && u.Active);
                    
                    if (!managerExists)
                    {
                        throw new ArgumentException($"Usuario manager con cédula {request.ManagerUserId} no encontrado o inactivo");
                    }
                }

                // VALIDAR ORGANIZACIÓN SI CAMBIÓ
                if (request.OrganizationId.HasValue && 
                    request.OrganizationId.Value != Guid.Empty &&
                    request.OrganizationId != activity.OrganizationId)
                {
                    var orgExists = await _context.Organizations
                        .AsNoTracking()
                        .AnyAsync(o => o.Id == request.OrganizationId.Value && o.Active);
                    
                    if (!orgExists)
                    {
                        throw new ArgumentException($"Organización {request.OrganizationId.Value} no encontrada");
                    }
                }

                // ACTUALIZAR CAMPOS
                activity.Name = request.Name.Trim();
                activity.Description = request.Description?.Trim();
                activity.StartDate = request.StartDate;
                activity.StartTime = request.StartTime;
                activity.EndDate = request.EndDate;
                activity.EndTime = request.EndTime;
                activity.Location = request.Location?.Trim();
                
                // ACTUALIZAR STATUS
                if (request.ActivityStatusId > 0)
                {
                    var statusExists = await _context.ActivityStatuses
                        .AnyAsync(s => s.Id == (long)request.ActivityStatusId && s.Active);
                    
                    if (statusExists)
                    {
                        activity.ActivityStatusId = (long)request.ActivityStatusId;
                    }
                }

                activity.ManagerUserId = string.IsNullOrWhiteSpace(request.ManagerUserId)
                    ? null
                    : request.ManagerUserId.Trim();
                    
                activity.OrganizationId = (!request.OrganizationId.HasValue || request.OrganizationId.Value == Guid.Empty)
                    ? null 
                    : request.OrganizationId.Value;
                    
                activity.UpdatedAt = DateTime.UtcNow;
                activity.UpdatedBy = request.ManagerUserId?.Trim();
                activity.SyncVersion++;
                activity.SyncStatus = "pending";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Activity updated successfully: {ActivityId}", id);

                return await GetActivityByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating activity {ActivityId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteActivityAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                
                // Verificar registros relacionados
                var hasRelatedRecords = await _context.CashRegisters
                    .AnyAsync(cr => cr.ActivityId == longId);
                
                if (hasRelatedRecords)
                {
                    _logger.LogWarning("Cannot delete activity {ActivityId}: has related records", id);
                    throw new InvalidOperationException("No se puede eliminar la actividad porque tiene registros relacionados");
                }

                var activity = await _context.Activities.FindAsync(longId);
                if (activity == null)
                {
                    return false;
                }

                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Deleted activity: {ActivityId}", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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

        // ============================================
        // MÉTODOS HELPER PARA MAPEO LONG <-> GUID
        // ============================================

        /// <summary>
        /// Convierte long (BD) a Guid (DTO) de forma determinística
        /// </summary>
        private static Guid MapLongToGuid(long longId)
        {
            var guidBytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            
            Array.Copy(longBytes, 0, guidBytes, 0, 8);
            
            guidBytes[8] = 0xAC;
            guidBytes[9] = 0x71;
            guidBytes[10] = 0x00;
            guidBytes[11] = 0x00;
            guidBytes[12] = 0x00;
            guidBytes[13] = 0x00;
            guidBytes[14] = 0x00;
            guidBytes[15] = 0x01;
            
            return new Guid(guidBytes);
        }

        /// <summary>
        /// Convierte Guid (DTO) a long (BD)
        /// </summary>
        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}