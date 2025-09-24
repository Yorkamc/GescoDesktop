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
                    .ToListAsync();

                var result = new List<ActivityDto>();
                foreach (var a in activities)
                {
                    // ✅ ACTUALIZADO: Obtener información del manager si existe
                    string? managerUserName = null;
                    if (!string.IsNullOrEmpty(a.ManagerUserId))
                    {
                        var manager = await _context.Users
                            .Where(u => u.Id == a.ManagerUserId)
                            .Select(u => u.FullName ?? u.Username)
                            .FirstOrDefaultAsync();
                        managerUserName = manager;
                    }

                    result.Add(new ActivityDto
                    {
                        Id = GenerateGuidFromInt(a.Id), // Mapear int ID a Guid para DTO
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = a.ActivityStatusId, // int -> int (correcto)
                        StatusName = a.ActivityStatus?.Name,
                        ManagerUserId = a.ManagerUserId, // ✅ string -> string (cédula)
                        ManagerUserName = managerUserName, // ✅ Nombre del manager
                        OrganizationId = a.OrganizationId, // Guid -> Guid (correcto)
                        OrganizationName = a.Organization?.Name,
                        CreatedAt = a.CreatedAt
                    });
                }

                _logger.LogInformation("Retrieved {Count} activities", result.Count);
                return result;
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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(id);
                
                var activity = await _context.Activities
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .FirstOrDefaultAsync(a => a.Id == intId);

                if (activity == null)
                {
                    return null;
                }

                // ✅ ACTUALIZADO: Obtener información del manager
                string? managerUserName = null;
                if (!string.IsNullOrEmpty(activity.ManagerUserId))
                {
                    var manager = await _context.Users
                        .Where(u => u.Id == activity.ManagerUserId)
                        .Select(u => u.FullName ?? u.Username)
                        .FirstOrDefaultAsync();
                    managerUserName = manager;
                }

                return new ActivityDto
                {
                    Id = id, // Mantener el Guid del parámetro
                    Name = activity.Name,
                    Description = activity.Description,
                    StartDate = activity.StartDate,
                    StartTime = activity.StartTime,
                    EndDate = activity.EndDate,
                    EndTime = activity.EndTime,
                    Location = activity.Location,
                    ActivityStatusId = activity.ActivityStatusId, // int -> int (correcto)
                    StatusName = activity.ActivityStatus?.Name,
                    ManagerUserId = activity.ManagerUserId, // ✅ string -> string (cédula)
                    ManagerUserName = managerUserName, // ✅ Nombre del manager
                    OrganizationId = activity.OrganizationId, // Guid -> Guid (correcto)
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
                // Get the first activity status as default (or find by name)
                var defaultStatus = await _context.ActivityStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Not Started");
                
                if (defaultStatus == null)
                {
                    defaultStatus = await _context.ActivityStatuses.FirstOrDefaultAsync();
                }

                // ✅ ACTUALIZADO: Validar que el manager existe si se proporciona cédula
                if (!string.IsNullOrEmpty(request.ManagerUserId))
                {
                    var managerExists = await _context.Users
                        .AnyAsync(u => u.Id == request.ManagerUserId && u.Active);
                    
                    if (!managerExists)
                    {
                        _logger.LogWarning("Manager user with cédula {Cedula} not found or inactive", request.ManagerUserId);
                        throw new ArgumentException($"Usuario manager con cédula {request.ManagerUserId} no encontrado o inactivo");
                    }
                }

                var activity = new Activity
                {
                    // Id se genera automáticamente como int
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    StartTime = request.StartTime,
                    EndDate = request.EndDate,
                    EndTime = request.EndTime,
                    Location = request.Location,
                    ActivityStatusId = defaultStatus?.Id ?? 1, // int -> int (correcto)
                    ManagerUserId = request.ManagerUserId, // ✅ string -> string (cédula)
                    OrganizationId = request.OrganizationId, // Guid -> Guid (correcto)
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.ManagerUserId // ✅ Creado por el manager (cédula)
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new activity: {ActivityName} with ID {ActivityId}, Manager: {ManagerId}", 
                    activity.Name, activity.Id, request.ManagerUserId);

                // Generar Guid para el DTO de respuesta basado en el int ID
                var responseGuid = GenerateGuidFromInt(activity.Id);
                
                // Obtener nombre del manager para la respuesta
                string? managerUserName = null;
                if (!string.IsNullOrEmpty(request.ManagerUserId))
                {
                    var manager = await _context.Users
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
                    ActivityStatusId = activity.ActivityStatusId, // int -> int (correcto)
                    StatusName = defaultStatus?.Name,
                    ManagerUserId = activity.ManagerUserId, // ✅ string -> string (cédula)
                    ManagerUserName = managerUserName, // ✅ Nombre del manager
                    OrganizationId = activity.OrganizationId, // Guid -> Guid (correcto)
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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(id);
                
                var activity = await _context.Activities.FindAsync(intId);
                if (activity == null)
                {
                    return null;
                }

                // ✅ ACTUALIZADO: Validar que el manager existe si se cambia
                if (!string.IsNullOrEmpty(request.ManagerUserId) && request.ManagerUserId != activity.ManagerUserId)
                {
                    var managerExists = await _context.Users
                        .AnyAsync(u => u.Id == request.ManagerUserId && u.Active);
                    
                    if (!managerExists)
                    {
                        _logger.LogWarning("Manager user with cédula {Cedula} not found or inactive", request.ManagerUserId);
                        throw new ArgumentException($"Usuario manager con cédula {request.ManagerUserId} no encontrado o inactivo");
                    }
                }

                activity.Name = request.Name;
                activity.Description = request.Description;
                activity.StartDate = request.StartDate;
                activity.StartTime = request.StartTime;
                activity.EndDate = request.EndDate;
                activity.EndTime = request.EndTime;
                activity.Location = request.Location;
                
                // Manejar actualización de status correctamente
                if (request.ActivityStatusId > 0)
                {
                    var status = await _context.ActivityStatuses.FindAsync(request.ActivityStatusId);
                    if (status != null)
                    {
                        activity.ActivityStatusId = status.Id; // int -> int (correcto)
                    }
                }

                activity.ManagerUserId = request.ManagerUserId; // ✅ string -> string (cédula)
                activity.OrganizationId = request.OrganizationId; // Guid -> Guid (correcto)
                activity.UpdatedAt = DateTime.UtcNow;
                activity.UpdatedBy = request.ManagerUserId; // ✅ Actualizado por el manager (cédula)

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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(id);
                
                var activity = await _context.Activities.FindAsync(intId);
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
                // Get "In Progress" and "Not Started" statuses
                var activeStatuses = await _context.ActivityStatuses
                    .Where(s => s.Name == "In Progress" || s.Name == "Not Started")
                    .Select(s => s.Id)
                    .ToListAsync();

                var activities = await _context.Activities
                    .Include(a => a.ActivityStatus)
                    .Include(a => a.Organization)
                    .Where(a => activeStatuses.Contains(a.ActivityStatusId))
                    .OrderByDescending(a => a.StartDate)
                    .ToListAsync();

                var result = new List<ActivityDto>();
                foreach (var a in activities)
                {
                    // ✅ ACTUALIZADO: Obtener información del manager
                    string? managerUserName = null;
                    if (!string.IsNullOrEmpty(a.ManagerUserId))
                    {
                        var manager = await _context.Users
                            .Where(u => u.Id == a.ManagerUserId)
                            .Select(u => u.FullName ?? u.Username)
                            .FirstOrDefaultAsync();
                        managerUserName = manager;
                    }

                    result.Add(new ActivityDto
                    {
                        Id = GenerateGuidFromInt(a.Id), // Mapear int ID a Guid
                        Name = a.Name,
                        Description = a.Description,
                        StartDate = a.StartDate,
                        StartTime = a.StartTime,
                        EndDate = a.EndDate,
                        EndTime = a.EndTime,
                        Location = a.Location,
                        ActivityStatusId = a.ActivityStatusId, // int -> int (correcto)
                        StatusName = a.ActivityStatus?.Name,
                        ManagerUserId = a.ManagerUserId, // ✅ string -> string (cédula)
                        ManagerUserName = managerUserName, // ✅ Nombre del manager
                        OrganizationId = a.OrganizationId, // Guid -> Guid (correcto)
                        OrganizationName = a.Organization?.Name,
                        CreatedAt = a.CreatedAt
                    });
                }

                return result;
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

                // Get status IDs by name
                var inProgressStatus = await _context.ActivityStatuses
                    .FirstOrDefaultAsync(s => s.Name == "In Progress");

                var completedSalesStatus = await _context.SalesStatuses
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

        // MÉTODOS HELPER PARA MAPEO INT <-> GUID
        private static Guid GenerateGuidFromInt(int intId)
        {
            // Generar un Guid determinístico basado en el int ID
            var bytes = new byte[16];
            var intBytes = BitConverter.GetBytes(intId);
            Array.Copy(intBytes, 0, bytes, 0, Math.Min(4, intBytes.Length));
            
            // Llenar el resto con un patrón predecible para que sea determinístico
            for (int i = 4; i < 16; i++)
            {
                bytes[i] = (byte)(intId % 256);
            }
            
            return new Guid(bytes);
        }

        private static int ExtractIntFromGuid(Guid guid)
        {
            // Extraer el int ID del Guid
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}