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
                _logger.LogInformation("Creating activity: {ActivityName}", request.Name);

                // 1. OBTENER STATUS ACTIVO (CON VALIDACIÓN ROBUSTA)
                var defaultStatus = await _context.ActivityStatuses
                    .AsNoTracking()
                    .Where(s => s.Active)
                    .OrderBy(s => s.Id)
                    .FirstOrDefaultAsync();

                if (defaultStatus == null)
                {
                    _logger.LogError("No active activity statuses found");
                    throw new InvalidOperationException(
                        "No se encontró ningún estado de actividad válido. " +
                        "Contacte al administrador del sistema."
                    );
                }

                _logger.LogDebug("Using activity status: {StatusName} (ID: {StatusId})", 
                    defaultStatus.Name, defaultStatus.Id);

                // 2. VALIDAR MANAGER SI SE PROPORCIONA
                if (!string.IsNullOrWhiteSpace(request.ManagerUserId))
                {
                    var managerExists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == request.ManagerUserId && u.Active);

                    if (!managerExists)
                    {
                        _logger.LogWarning(
                            "Manager user not found or inactive: {ManagerId}", 
                            request.ManagerUserId
                        );
                        throw new ArgumentException(
                            $"Usuario manager con cédula {request.ManagerUserId} no encontrado o inactivo",
                            nameof(request.ManagerUserId)
                        );
                    }

                    _logger.LogDebug("Manager validated: {ManagerId}", request.ManagerUserId);
                }
                else
                {
                    _logger.LogDebug("No manager specified (optional)");
                }

                // 3. VALIDAR ORGANIZACIÓN SI SE PROPORCIONA
                if (request.OrganizationId.HasValue && request.OrganizationId.Value != Guid.Empty)
                {
                    var orgExists = await _context.Organizations
                        .AsNoTracking()
                        .AnyAsync(o => o.Id == request.OrganizationId.Value && o.Active);

                    if (!orgExists)
                    {
                        _logger.LogWarning(
                            "Organization not found: {OrgId}", 
                            request.OrganizationId.Value
                        );
                        throw new ArgumentException(
                            $"Organización {request.OrganizationId.Value} no encontrada",
                            nameof(request.OrganizationId)
                        );
                    }

                    _logger.LogDebug("Organization validated: {OrgId}", request.OrganizationId.Value);
                }

                // 4. CREAR ENTIDAD
                var activity = new Activity
                {
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    StartTime = request.StartTime,
                    EndDate = request.EndDate,
                    EndTime = request.EndTime,
                    Location = request.Location,
                    ActivityStatusId = defaultStatus.Id,
                    ManagerUserId = string.IsNullOrWhiteSpace(request.ManagerUserId) 
                        ? null 
                        : request.ManagerUserId.Trim(),
                    OrganizationId = request.OrganizationId == Guid.Empty 
                        ? null 
                        : request.OrganizationId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ManagerUserId
                };

                _logger.LogDebug(
                    "Activity entity created: Name={Name}, StatusId={StatusId}, ManagerId={ManagerId}",
                    activity.Name,
                    activity.ActivityStatusId,
                    activity.ManagerUserId ?? "null"
                );

                // 5. GUARDAR CON MANEJO DE ERRORES
                try
                {
                    _context.Activities.Add(activity);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Activity saved successfully: ID={ActivityId}, Name={ActivityName}",
                        activity.Id,
                        activity.Name
                    );
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error saving activity");

                    if (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                    {
                        throw new InvalidOperationException(
                            "Error de relación en base de datos. Verifique que el manager y organización existan.",
                            ex
                        );
                    }

                    throw new InvalidOperationException(
                        "Error al guardar actividad en base de datos. Por favor intente nuevamente.",
                        ex
                    );
                }

                // 6. MAPEAR A DTO
                var responseGuid = MapLongToGuid(activity.Id);

                _logger.LogDebug(
                    "Mapping activity ID: long={LongId} -> Guid={GuidId}",
                    activity.Id,
                    responseGuid
                );

                // 7. OBTENER DATOS RELACIONADOS
                string? managerUserName = null;
                if (!string.IsNullOrEmpty(activity.ManagerUserId))
                {
                    managerUserName = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == activity.ManagerUserId)
                        .Select(u => u.FullName ?? u.Username)
                        .FirstOrDefaultAsync();
                }

                string? organizationName = null;
                if (activity.OrganizationId.HasValue)
                {
                    organizationName = await _context.Organizations
                        .AsNoTracking()
                        .Where(o => o.Id == activity.OrganizationId.Value)
                        .Select(o => o.Name)
                        .FirstOrDefaultAsync();
                }

                // 8. CONSTRUIR DTO RESPUESTA
                var dto = new ActivityDto
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
                    StatusName = defaultStatus.Name,
                    ManagerUserId = activity.ManagerUserId,
                    ManagerUserName = managerUserName,
                    OrganizationId = activity.OrganizationId,
                    OrganizationName = organizationName,
                    CreatedAt = activity.CreatedAt
                };

                _logger.LogInformation(
                    "Activity DTO created successfully: ID={DtoId}, Name={Name}",
                    dto.Id,
                    dto.Name
                );

                return dto;
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error creating activity: {ActivityName}", request.Name);
                throw new InvalidOperationException(
                    "Error inesperado al crear actividad. Por favor contacte al soporte técnico.",
                    ex
                );
            }
        }

        public async Task<ActivityDto?> UpdateActivityAsync(Guid id, CreateActivityRequest request)
        {
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

                // VALIDAR MANAGER SI SE PROPORCIONA Y CAMBIÓ
                if (!string.IsNullOrWhiteSpace(request.ManagerUserId) && 
                    request.ManagerUserId != activity.ManagerUserId)
                {
                    var managerExists = await _context.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.Id == request.ManagerUserId && u.Active);
                    
                    if (!managerExists)
                    {
                        _logger.LogWarning("Manager user not found: {ManagerId}", request.ManagerUserId);
                        throw new ArgumentException(
                            $"Usuario manager con cédula {request.ManagerUserId} no encontrado o inactivo",
                            nameof(request.ManagerUserId)
                        );
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
                        throw new ArgumentException(
                            $"Organización {request.OrganizationId.Value} no encontrada",
                            nameof(request.OrganizationId)
                        );
                    }
                }

                // ACTUALIZAR CAMPOS
                activity.Name = request.Name;
                activity.Description = request.Description;
                activity.StartDate = request.StartDate;
                activity.StartTime = request.StartTime;
                activity.EndDate = request.EndDate;
                activity.EndTime = request.EndTime;
                activity.Location = request.Location;
                
                // ACTUALIZAR STATUS SI SE PROPORCIONA
                if (request.ActivityStatusId > 0)
                {
                    var status = await _context.ActivityStatuses.FindAsync((long)request.ActivityStatusId);
                    if (status != null && status.Active)
                    {
                        activity.ActivityStatusId = status.Id;
                    }
                }

                activity.ManagerUserId = string.IsNullOrWhiteSpace(request.ManagerUserId)
                    ? null
                    : request.ManagerUserId.Trim();
                    
                activity.OrganizationId = request.OrganizationId == Guid.Empty 
                    ? null 
                    : request.OrganizationId;
                    
                activity.UpdatedAt = DateTime.UtcNow;
                activity.UpdatedBy = request.ManagerUserId;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Activity updated successfully: {ActivityId}", id);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error updating activity {ActivityId}", id);
                    
                    if (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
                    {
                        throw new InvalidOperationException(
                            "Error de relación en base de datos. Verifique que el manager y organización existan.",
                            ex
                        );
                    }
                    
                    throw new InvalidOperationException(
                        "Error al actualizar actividad en base de datos.",
                        ex
                    );
                }

                return await GetActivityByIdAsync(id);
            }
            catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error updating activity {ActivityId}", id);
                throw new InvalidOperationException(
                    "Error inesperado al actualizar actividad.",
                    ex
                );
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
        /// Convierte long (BD) a Guid (DTO) de forma determinística y simple
        /// </summary>
        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            
            // Copiar los 8 bytes del long a los primeros 8 bytes del Guid
            Array.Copy(longBytes, 0, bytes, 0, 8);
            
            // Llenar el resto con ceros para mapeo predecible
            for (int i = 8; i < 16; i++)
            {
                bytes[i] = 0;
            }
            
            return new Guid(bytes);
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