using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(LocalDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // SERVICE CATEGORIES (Catálogos globales)
        // ============================================

        public async Task<List<ServiceCategoryDto>> GetServiceCategoriesAsync(Guid? organizationId = null)
        {
            try
            {
                var query = _context.ServiceCategories
                    .Include(sc => sc.Organization)
                    .AsQueryable();

                if (organizationId.HasValue)
                {
                    query = query.Where(sc => sc.OrganizationId == organizationId.Value);
                }

                var categories = await query
                    .Where(sc => sc.Active)
                    .OrderBy(sc => sc.Name)
                    .Select(sc => new ServiceCategoryDto
                    {
                        Id = MapLongToGuid(sc.Id),
                        OrganizationId = sc.OrganizationId,
                        OrganizationName = sc.Organization != null ? sc.Organization.Name : null,
                        Name = sc.Name,
                        Description = sc.Description,
                        Active = sc.Active,
                        CreatedAt = sc.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} service categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service categories");
                throw;
            }
        }

        public async Task<ServiceCategoryDto?> GetServiceCategoryByIdAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                var category = await _context.ServiceCategories
                    .Include(sc => sc.Organization)
                    .FirstOrDefaultAsync(sc => sc.Id == longId);

                if (category == null)
                    return null;

                return new ServiceCategoryDto
                {
                    Id = id,
                    OrganizationId = category.OrganizationId,
                    OrganizationName = category.Organization?.Name,
                    Name = category.Name,
                    Description = category.Description,
                    Active = category.Active,
                    CreatedAt = category.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service category {Id}", id);
                throw;
            }
        }

        public async Task<ServiceCategoryDto> CreateServiceCategoryAsync(CreateServiceCategoryRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating service category: {Name}", request.Name);

                // Validar organización
                var organization = await _context.Organizations.FindAsync(request.OrganizationId);
                if (organization == null)
                {
                    throw new ArgumentException($"Organization {request.OrganizationId} not found");
                }

                // Validar que no exista otra categoría con el mismo nombre en la organización
                var exists = await _context.ServiceCategories
                    .AnyAsync(sc => sc.OrganizationId == request.OrganizationId && 
                                   sc.Name == request.Name && 
                                   sc.Active);

                if (exists)
                {
                    throw new ArgumentException($"Ya existe una categoría con el nombre '{request.Name}' en esta organización");
                }

                var category = new ServiceCategory
                {
                    OrganizationId = request.OrganizationId,
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    SyncVersion = 1
                };

                _context.ServiceCategories.Add(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Service category created: {Id}", category.Id);

                return await GetServiceCategoryByIdAsync(MapLongToGuid(category.Id))
                    ?? throw new InvalidOperationException("Failed to retrieve created category");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating service category");
                throw;
            }
        }

        public async Task<ServiceCategoryDto?> UpdateServiceCategoryAsync(Guid id, CreateServiceCategoryRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var category = await _context.ServiceCategories.FindAsync(longId);

                if (category == null)
                    return null;

                // Validar que no exista otra categoría con el mismo nombre
                var exists = await _context.ServiceCategories
                    .AnyAsync(sc => sc.Id != longId && 
                                   sc.OrganizationId == request.OrganizationId && 
                                   sc.Name == request.Name && 
                                   sc.Active);

                if (exists)
                {
                    throw new ArgumentException($"Ya existe otra categoría con el nombre '{request.Name}'");
                }

                category.Name = request.Name.Trim();
                category.Description = request.Description?.Trim();
                category.UpdatedAt = DateTime.UtcNow;
                category.SyncVersion++;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Service category updated: {Id}", id);
                return await GetServiceCategoryByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating service category {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteServiceCategoryAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var category = await _context.ServiceCategories.FindAsync(longId);

                if (category == null)
                    return false;

                // Verificar si tiene actividades asociadas
                var hasActivities = await _context.ActivityCategories
                    .AnyAsync(ac => ac.ServiceCategoryId == longId);

                if (hasActivities)
                {
                    throw new InvalidOperationException("No se puede eliminar la categoría porque tiene actividades asociadas");
                }

                // Soft delete
                category.Active = false;
                category.UpdatedAt = DateTime.UtcNow;
                category.SyncVersion++;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Service category soft deleted: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting service category {Id}", id);
                throw;
            }
        }

        // ============================================
        // ACTIVITY CATEGORIES (Relación Actividad-Categoría)
        // ============================================

        public async Task<List<ActivityCategoryDto>> GetActivityCategoriesAsync(Guid? activityId = null)
        {
            try
            {
                var query = _context.ActivityCategories
                    .Include(ac => ac.Activity)
                    .Include(ac => ac.ServiceCategory)
                    .AsQueryable();

                if (activityId.HasValue)
                {
                    var longActivityId = MapGuidToLong(activityId.Value);
                    query = query.Where(ac => ac.ActivityId == longActivityId);
                }

                var categories = await query
                    .OrderBy(ac => ac.ServiceCategory.Name)
                    .Select(ac => new ActivityCategoryDto
                    {
                        Id = MapLongToGuid(ac.Id),
                        ActivityId = MapLongToGuid(ac.ActivityId),
                        ActivityName = ac.Activity.Name,
                        ServiceCategoryId = MapLongToGuid(ac.ServiceCategoryId),
                        ServiceCategoryName = ac.ServiceCategory.Name,
                        CreatedAt = ac.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} activity categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity categories");
                throw;
            }
        }

        public async Task<ActivityCategoryDto?> GetActivityCategoryByIdAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                var category = await _context.ActivityCategories
                    .Include(ac => ac.Activity)
                    .Include(ac => ac.ServiceCategory)
                    .FirstOrDefaultAsync(ac => ac.Id == longId);

                if (category == null)
                    return null;

                return new ActivityCategoryDto
                {
                    Id = id,
                    ActivityId = MapLongToGuid(category.ActivityId),
                    ActivityName = category.Activity.Name,
                    ServiceCategoryId = MapLongToGuid(category.ServiceCategoryId),
                    ServiceCategoryName = category.ServiceCategory.Name,
                    CreatedAt = category.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity category {Id}", id);
                throw;
            }
        }

        public async Task<ActivityCategoryDto> CreateActivityCategoryAsync(CreateActivityCategoryRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var activityId = MapGuidToLong(request.ActivityId);
                var serviceCategoryId = MapGuidToLong(request.ServiceCategoryId);

                _logger.LogInformation("Creating activity category: Activity {ActivityId}, Category {CategoryId}", 
                    activityId, serviceCategoryId);

                // Validar actividad
                var activity = await _context.Activities.FindAsync(activityId);
                if (activity == null)
                {
                    throw new ArgumentException($"Activity {request.ActivityId} not found");
                }

                // Validar categoría de servicio
                var serviceCategory = await _context.ServiceCategories.FindAsync(serviceCategoryId);
                if (serviceCategory == null)
                {
                    throw new ArgumentException($"Service category {request.ServiceCategoryId} not found");
                }

                // Validar que no exista ya esta relación
                var exists = await _context.ActivityCategories
                    .AnyAsync(ac => ac.ActivityId == activityId && 
                                   ac.ServiceCategoryId == serviceCategoryId);

                if (exists)
                {
                    throw new ArgumentException("Esta categoría ya está asociada a la actividad");
                }

                var activityCategory = new ActivityCategory
                {
                    ActivityId = activityId,
                    ServiceCategoryId = serviceCategoryId,
                    CreatedAt = DateTime.UtcNow,
                    SyncVersion = 1
                };

                _context.ActivityCategories.Add(activityCategory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Activity category created: {Id}", activityCategory.Id);

                return await GetActivityCategoryByIdAsync(MapLongToGuid(activityCategory.Id))
                    ?? throw new InvalidOperationException("Failed to retrieve created activity category");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating activity category");
                throw;
            }
        }

        public async Task<bool> DeleteActivityCategoryAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var category = await _context.ActivityCategories.FindAsync(longId);

                if (category == null)
                    return false;

                // Verificar si tiene productos asociados
                var hasProducts = await _context.CategoryProducts
                    .AnyAsync(cp => cp.ActivityCategoryId == longId);

                if (hasProducts)
                {
                    throw new InvalidOperationException("No se puede eliminar la categoría porque tiene productos asociados");
                }

                _context.ActivityCategories.Remove(category);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Activity category deleted: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting activity category {Id}", id);
                throw;
            }
        }

        // ============================================
        // HELPER METHODS PARA MAPEO GUID <-> LONG
        // ============================================

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, 8);
            bytes[8] = 0xCA; bytes[9] = 0x7E; // "CATE" identifier
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}