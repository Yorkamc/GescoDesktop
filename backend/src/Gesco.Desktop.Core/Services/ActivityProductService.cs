using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class ActivityProductService : IActivityProductService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<ActivityProductService> _logger;

        public ActivityProductService(
            LocalDbContext context, 
            ILogger<ActivityProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================
        // CONSULTAS POR ACTIVIDAD
        // ============================================

        public async Task<List<CategoryProductDetailedDto>> GetProductsByActivityAsync(Guid activityId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);
                
                var products = await _context.CategoryProducts
                    .AsNoTracking()
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.ServiceCategory)
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.Activity)
                    .Where(p => p.ActivityCategory.ActivityId == activityIdLong && p.Active)
                    .OrderBy(p => p.ActivityCategory.ServiceCategory.Name)
                    .ThenBy(p => p.Name)
                    .Select(p => new CategoryProductDetailedDto
                    {
                        Id = MapLongToGuid(p.Id),
                        ActivityCategoryId = MapLongToGuid(p.ActivityCategoryId),
                        ActivityCategoryName = p.ActivityCategory.ServiceCategory.Name,
                        ActivityId = MapLongToGuid(p.ActivityCategory.ActivityId),
                        ActivityName = p.ActivityCategory.Activity.Name,
                        ServiceCategoryId = MapLongToGuid(p.ActivityCategory.ServiceCategoryId),
                        ServiceCategoryName = p.ActivityCategory.ServiceCategory.Name,
                        Code = p.Code,
                        Name = p.Name,
                        Description = p.Description,
                        UnitPrice = p.UnitPrice,
                        InitialQuantity = p.InitialQuantity,
                        CurrentQuantity = p.CurrentQuantity,
                        AlertQuantity = p.AlertQuantity,
                        Active = p.Active,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} products for activity {ActivityId}", 
                    products.Count, 
                    activityId);

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for activity {ActivityId}", activityId);
                throw;
            }
        }

        public async Task<ActivityProductsSummaryDto> GetActivityProductsSummaryAsync(Guid activityId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);
                
                var activity = await _context.Activities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == activityIdLong);

                if (activity == null)
                {
                    throw new ArgumentException($"Activity {activityId} not found");
                }

                var categoriesWithProducts = await _context.ActivityCategories
                    .AsNoTracking()
                    .Include(ac => ac.ServiceCategory)
                    .Include(ac => ac.CategoryProducts)
                    .Where(ac => ac.ActivityId == activityIdLong)
                    .Select(ac => new CategoryProductsGroupDto
                    {
                        ActivityCategoryId = MapLongToGuid(ac.Id),
                        ServiceCategoryId = MapLongToGuid(ac.ServiceCategoryId),
                        ServiceCategoryName = ac.ServiceCategory.Name,
                        ProductCount = ac.CategoryProducts.Count(p => p.Active),
                        ActiveProductCount = ac.CategoryProducts.Count(p => p.Active),
                        Products = ac.CategoryProducts
                            .Where(p => p.Active)
                            .Select(p => new CategoryProductDetailedDto
                            {
                                Id = MapLongToGuid(p.Id),
                                ActivityCategoryId = MapLongToGuid(ac.Id),
                                ActivityCategoryName = ac.ServiceCategory.Name,
                                ActivityId = activityId,
                                ActivityName = activity.Name,
                                ServiceCategoryId = MapLongToGuid(ac.ServiceCategoryId),
                                ServiceCategoryName = ac.ServiceCategory.Name,
                                Code = p.Code,
                                Name = p.Name,
                                Description = p.Description,
                                UnitPrice = p.UnitPrice,
                                InitialQuantity = p.InitialQuantity,
                                CurrentQuantity = p.CurrentQuantity,
                                AlertQuantity = p.AlertQuantity,
                                Active = p.Active,
                                CreatedAt = p.CreatedAt
                            })
                            .ToList()
                    })
                    .ToListAsync();

                var allProducts = categoriesWithProducts.SelectMany(c => c.Products).ToList();

                var summary = new ActivityProductsSummaryDto
                {
                    ActivityId = activityId,
                    ActivityName = activity.Name,
                    TotalCategories = categoriesWithProducts.Count,
                    TotalProducts = allProducts.Count,
                    ActiveProducts = allProducts.Count(p => p.Active),
                    LowStockProducts = allProducts.Count(p => p.CurrentQuantity <= p.AlertQuantity),
                    TotalInventoryValue = allProducts.Sum(p => p.CurrentQuantity * p.UnitPrice),
                    CategoriesWithProducts = categoriesWithProducts
                };

                _logger.LogInformation(
                    "Generated summary for activity {ActivityId}: {ProductCount} products in {CategoryCount} categories",
                    activityId, 
                    summary.TotalProducts, 
                    summary.TotalCategories);

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary for activity {ActivityId}", activityId);
                throw;
            }
        }

        public async Task<List<CategoryProductDetailedDto>> GetProductsByActivityCategoryAsync(
            Guid activityId, 
            Guid serviceCategoryId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);
                var serviceCategoryIdLong = MapGuidToLong(serviceCategoryId);

                var products = await _context.CategoryProducts
                    .AsNoTracking()
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.ServiceCategory)
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.Activity)
                    .Where(p => p.ActivityCategory.ActivityId == activityIdLong 
                             && p.ActivityCategory.ServiceCategoryId == serviceCategoryIdLong
                             && p.Active)
                    .OrderBy(p => p.Name)
                    .Select(p => new CategoryProductDetailedDto
                    {
                        Id = MapLongToGuid(p.Id),
                        ActivityCategoryId = MapLongToGuid(p.ActivityCategoryId),
                        ActivityCategoryName = p.ActivityCategory.ServiceCategory.Name,
                        ActivityId = activityId,
                        ActivityName = p.ActivityCategory.Activity.Name,
                        ServiceCategoryId = serviceCategoryId,
                        ServiceCategoryName = p.ActivityCategory.ServiceCategory.Name,
                        Code = p.Code,
                        Name = p.Name,
                        Description = p.Description,
                        UnitPrice = p.UnitPrice,
                        InitialQuantity = p.InitialQuantity,
                        CurrentQuantity = p.CurrentQuantity,
                        AlertQuantity = p.AlertQuantity,
                        Active = p.Active,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} products for activity {ActivityId} and category {CategoryId}",
                    products.Count,
                    activityId,
                    serviceCategoryId);

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving products for activity {ActivityId} and category {CategoryId}",
                    activityId,
                    serviceCategoryId);
                throw;
            }
        }

        // ============================================
        // GESTIÓN DE PRODUCTOS
        // ============================================

        public async Task<CategoryProductDetailedDto> CreateProductForActivityAsync(
            Guid activityId,
            CreateProductForActivityRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation(
                    "Creating product for activity {ActivityId}: {ProductName}",
                    activityId,
                    request.Name);

                // Validar que la ActivityCategory pertenece a la actividad
                var isValid = await ValidateActivityCategoryBelongsToActivityAsync(
                    activityId, 
                    request.ActivityCategoryId);

                if (!isValid)
                {
                    throw new ArgumentException(
                        $"ActivityCategory {request.ActivityCategoryId} does not belong to Activity {activityId}");
                }

                var activityCategoryIdLong = MapGuidToLong(request.ActivityCategoryId);

                // Crear el producto
                var product = new CategoryProduct
                {
                    ActivityCategoryId = activityCategoryIdLong,
                    Code = request.Code?.Trim(),
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    UnitPrice = request.UnitPrice,
                    InitialQuantity = request.InitialQuantity,
                    CurrentQuantity = request.InitialQuantity,
                    AlertQuantity = request.AlertQuantity,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    SyncVersion = 1
                };

                _context.CategoryProducts.Add(product);
                await _context.SaveChangesAsync();

                // Crear movimiento inicial de inventario si hay cantidad inicial
                if (request.InitialQuantity > 0)
                {
                    var stockInType = await _context.InventoryMovementTypes
                        .FirstOrDefaultAsync(t => t.Name == "Stock In");

                    if (stockInType != null)
                    {
                        var movement = new InventoryMovement
                        {
                            ProductId = product.Id,
                            MovementTypeId = stockInType.Id,
                            Quantity = request.InitialQuantity,
                            PreviousQuantity = 0,
                            NewQuantity = request.InitialQuantity,
                            MovementDate = DateTime.UtcNow,
                            Justification = "Initial stock",
                            CreatedAt = DateTime.UtcNow,
                            SyncVersion = 1
                        };

                        _context.InventoryMovements.Add(movement);
                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();

                // Recargar con includes para el DTO
                var createdProduct = await _context.CategoryProducts
                    .AsNoTracking()
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.ServiceCategory)
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.Activity)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (createdProduct == null)
                {
                    throw new InvalidOperationException("Failed to reload created product");
                }

                var dto = MapToDetailedDto(createdProduct, activityId);

                _logger.LogInformation(
                    "Product created successfully: {ProductId} - {ProductName}",
                    dto.Id,
                    dto.Name);

                return dto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating product for activity {ActivityId}", activityId);
                throw;
            }
        }

        public async Task<CategoryProductDetailedDto?> UpdateProductForActivityAsync(
            Guid activityId,
            Guid productId,
            CreateProductForActivityRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation(
                    "Updating product {ProductId} for activity {ActivityId}",
                    productId,
                    activityId);

                // Validar que el producto pertenece a la actividad
                var isValid = await ValidateProductBelongsToActivityAsync(activityId, productId);
                if (!isValid)
                {
                    _logger.LogWarning(
                        "Product {ProductId} does not belong to activity {ActivityId}",
                        productId,
                        activityId);
                    return null;
                }

                // Validar nueva ActivityCategory si cambió
                var isValidCategory = await ValidateActivityCategoryBelongsToActivityAsync(
                    activityId,
                    request.ActivityCategoryId);

                if (!isValidCategory)
                {
                    throw new ArgumentException(
                        $"ActivityCategory {request.ActivityCategoryId} does not belong to Activity {activityId}");
                }

                var productIdLong = MapGuidToLong(productId);
                var product = await _context.CategoryProducts.FindAsync(productIdLong);

                if (product == null)
                {
                    return null;
                }

                // Actualizar campos
                product.ActivityCategoryId = MapGuidToLong(request.ActivityCategoryId);
                product.Code = request.Code?.Trim();
                product.Name = request.Name.Trim();
                product.Description = request.Description?.Trim();
                product.UnitPrice = request.UnitPrice;
                product.AlertQuantity = request.AlertQuantity;

                if (request.InitialQuantity != product.InitialQuantity)
                {
                    product.InitialQuantity = request.InitialQuantity;
                }

                product.UpdatedAt = DateTime.UtcNow;
                product.SyncVersion++;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Recargar con includes
                var updatedProduct = await _context.CategoryProducts
                    .AsNoTracking()
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.ServiceCategory)
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.Activity)
                    .FirstOrDefaultAsync(p => p.Id == productIdLong);

                if (updatedProduct == null)
                {
                    return null;
                }

                var dto = MapToDetailedDto(updatedProduct, activityId);

                _logger.LogInformation("Product updated successfully: {ProductId}", productId);

                return dto;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(
                    ex,
                    "Error updating product {ProductId} for activity {ActivityId}",
                    productId,
                    activityId);
                throw;
            }
        }

        public async Task<bool> DeleteProductFromActivityAsync(Guid activityId, Guid productId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validar que el producto pertenece a la actividad
                var isValid = await ValidateProductBelongsToActivityAsync(activityId, productId);
                if (!isValid)
                {
                    _logger.LogWarning(
                        "Cannot delete: Product {ProductId} does not belong to activity {ActivityId}",
                        productId,
                        activityId);
                    return false;
                }

                var productIdLong = MapGuidToLong(productId);
                var product = await _context.CategoryProducts.FindAsync(productIdLong);

                if (product == null)
                {
                    return false;
                }

                // Soft delete
                product.Active = false;
                product.UpdatedAt = DateTime.UtcNow;
                product.SyncVersion++;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Product soft deleted: {ProductId} from activity {ActivityId}",
                    productId,
                    activityId);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(
                    ex,
                    "Error deleting product {ProductId} from activity {ActivityId}",
                    productId,
                    activityId);
                throw;
            }
        }

        // ============================================
        // GESTIÓN DE CATEGORÍAS DE ACTIVIDAD
        // ============================================

        public async Task<BatchOperationResultDto> AssignCategoriesToActivityAsync(
            AssignCategoriesToActivityRequest request)
        {
            var result = new BatchOperationResultDto
            {
                TotalRequested = request.ServiceCategoryIds.Count
            };

            try
            {
                var activityIdLong = MapGuidToLong(request.ActivityId);

                // Validar que la actividad existe
                var activityExists = await _context.Activities
                    .AnyAsync(a => a.Id == activityIdLong);

                if (!activityExists)
                {
                    result.Errors.Add(new BatchErrorDto
                    {
                        ItemId = request.ActivityId.ToString(),
                        ErrorMessage = $"Activity {request.ActivityId} not found"
                    });
                    return result;
                }

                // Obtener categorías ya asignadas
                var existingCategories = await _context.ActivityCategories
                    .Where(ac => ac.ActivityId == activityIdLong)
                    .Select(ac => ac.ServiceCategoryId)
                    .ToListAsync();

                foreach (var serviceCategoryId in request.ServiceCategoryIds)
                {
                    try
                    {
                        var serviceCategoryIdLong = MapGuidToLong(serviceCategoryId);

                        // Verificar si ya existe
                        if (existingCategories.Contains(serviceCategoryIdLong))
                        {
                            result.Errors.Add(new BatchErrorDto
                            {
                                ItemId = serviceCategoryId.ToString(),
                                ErrorMessage = "Category already assigned to this activity"
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // Validar que la categoría existe
                        var categoryExists = await _context.ServiceCategories
                            .AnyAsync(sc => sc.Id == serviceCategoryIdLong && sc.Active);

                        if (!categoryExists)
                        {
                            result.Errors.Add(new BatchErrorDto
                            {
                                ItemId = serviceCategoryId.ToString(),
                                ErrorMessage = "Service category not found or inactive"
                            });
                            result.FailureCount++;
                            continue;
                        }

                        // Crear la asignación
                        var activityCategory = new ActivityCategory
                        {
                            ActivityId = activityIdLong,
                            ServiceCategoryId = serviceCategoryIdLong,
                            CreatedAt = DateTime.UtcNow,
                            SyncVersion = 1
                        };

                        _context.ActivityCategories.Add(activityCategory);
                        await _context.SaveChangesAsync();

                        result.SuccessIds.Add(serviceCategoryId.ToString());
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error assigning category {CategoryId} to activity {ActivityId}",
                            serviceCategoryId,
                            request.ActivityId);

                        result.Errors.Add(new BatchErrorDto
                        {
                            ItemId = serviceCategoryId.ToString(),
                            ErrorMessage = ex.Message
                        });
                        result.FailureCount++;
                    }
                }

                _logger.LogInformation(
                    "Assigned {SuccessCount}/{TotalCount} categories to activity {ActivityId}",
                    result.SuccessCount,
                    result.TotalRequested,
                    request.ActivityId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in batch assign categories to activity {ActivityId}",
                    request.ActivityId);
                throw;
            }
        }

        public async Task<List<ServiceCategoryDto>> GetAvailableCategoriesForActivityAsync(Guid activityId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);

                // Obtener IDs de categorías ya asignadas
                var assignedCategoryIds = await _context.ActivityCategories
                    .Where(ac => ac.ActivityId == activityIdLong)
                    .Select(ac => ac.ServiceCategoryId)
                    .ToListAsync();

                // Obtener categorías no asignadas
                var availableCategories = await _context.ServiceCategories
                    .AsNoTracking()
                    .Include(sc => sc.Organization)
                    .Where(sc => sc.Active && !assignedCategoryIds.Contains(sc.Id))
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

                _logger.LogInformation(
                    "Found {Count} available categories for activity {ActivityId}",
                    availableCategories.Count,
                    activityId);

                return availableCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting available categories for activity {ActivityId}",
                    activityId);
                throw;
            }
        }

        public async Task<List<ActivityCategoryDto>> GetActivityCategoriesAsync(Guid activityId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);

                var categories = await _context.ActivityCategories
                    .AsNoTracking()
                    .Include(ac => ac.Activity)
                    .Include(ac => ac.ServiceCategory)
                    .Where(ac => ac.ActivityId == activityIdLong)
                    .OrderBy(ac => ac.ServiceCategory.Name)
                    .Select(ac => new ActivityCategoryDto
                    {
                        Id = MapLongToGuid(ac.Id),
                        ActivityId = activityId,
                        ActivityName = ac.Activity.Name,
                        ServiceCategoryId = MapLongToGuid(ac.ServiceCategoryId),
                        ServiceCategoryName = ac.ServiceCategory.Name,
                        CreatedAt = ac.CreatedAt
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {Count} categories for activity {ActivityId}",
                    categories.Count,
                    activityId);

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting categories for activity {ActivityId}",
                    activityId);
                throw;
            }
        }

        // ============================================
        // VALIDACIONES
        // ============================================

        public async Task<bool> ValidateActivityCategoryBelongsToActivityAsync(
            Guid activityId,
            Guid activityCategoryId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);
                var activityCategoryIdLong = MapGuidToLong(activityCategoryId);

                var exists = await _context.ActivityCategories
                    .AnyAsync(ac => ac.Id == activityCategoryIdLong 
                                 && ac.ActivityId == activityIdLong);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error validating ActivityCategory {ActivityCategoryId} for Activity {ActivityId}",
                    activityCategoryId,
                    activityId);
                return false;
            }
        }

        public async Task<bool> ValidateProductBelongsToActivityAsync(
            Guid activityId,
            Guid productId)
        {
            try
            {
                var activityIdLong = MapGuidToLong(activityId);
                var productIdLong = MapGuidToLong(productId);

                var exists = await _context.CategoryProducts
                    .Include(p => p.ActivityCategory)
                    .AnyAsync(p => p.Id == productIdLong 
                                && p.ActivityCategory.ActivityId == activityIdLong);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error validating Product {ProductId} for Activity {ActivityId}",
                    productId,
                    activityId);
                return false;
            }
        }

        // ============================================
        // MÉTODOS HELPER PARA MAPEO
        // ============================================

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, 8);
            bytes[8] = 0xAC;
            bytes[9] = 0x71;
            bytes[10] = 0x00;
            bytes[11] = 0x00;
            bytes[12] = 0x00;
            bytes[13] = 0x00;
            bytes[14] = 0x00;
            bytes[15] = 0x01;
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }

        private static CategoryProductDetailedDto MapToDetailedDto(
            CategoryProduct product, 
            Guid activityId)
        {
            return new CategoryProductDetailedDto
            {
                Id = MapLongToGuid(product.Id),
                ActivityCategoryId = MapLongToGuid(product.ActivityCategoryId),
                ActivityCategoryName = product.ActivityCategory?.ServiceCategory?.Name,
                ActivityId = activityId,
                ActivityName = product.ActivityCategory?.Activity?.Name,
                ServiceCategoryId = product.ActivityCategory != null 
                    ? MapLongToGuid(product.ActivityCategory.ServiceCategoryId) 
                    : Guid.Empty,
                ServiceCategoryName = product.ActivityCategory?.ServiceCategory?.Name,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                UnitPrice = product.UnitPrice,
                InitialQuantity = product.InitialQuantity,
                CurrentQuantity = product.CurrentQuantity,
                AlertQuantity = product.AlertQuantity,
                Active = product.Active,
                CreatedAt = product.CreatedAt
            };
        }
    }
}