using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(LocalDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CategoryProductDto>> GetProductsAsync(int? activityCategoryId = null)
        {
            try
            {
                var query = _context.CategoryProducts
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.ServiceCategory)
                    .Include(p => p.ActivityCategory)
                        .ThenInclude(ac => ac.Activity)
                    .AsQueryable();

                if (activityCategoryId.HasValue)
                {
                    var categoryId = await GetActivityCategoryIdByOrder(activityCategoryId.Value);
                    if (categoryId.HasValue)
                    {
                        query = query.Where(p => p.ActivityCategoryId == categoryId.Value);
                    }
                }

                var products = await query
                    .Where(p => p.Active)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                var result = new List<CategoryProductDto>();
                var allCategories = await _context.ActivityCategories.OrderBy(ac => ac.CreatedAt).ToListAsync();

                foreach (var p in products)
                {
                    result.Add(new CategoryProductDto
                    {
                        Id = GenerateGuidFromLong(p.Id), // ✅ ACTUALIZADO: long -> Guid
                        ActivityCategoryId = GetCategoryOrder(p.ActivityCategoryId, allCategories),
                        Code = p.Code,
                        Name = p.Name,
                        Description = p.Description,
                        UnitPrice = p.UnitPrice,
                        InitialQuantity = p.InitialQuantity,
                        CurrentQuantity = p.CurrentQuantity,
                        AlertQuantity = p.AlertQuantity,
                        Active = p.Active,
                        CreatedAt = p.CreatedAt
                    });
                }

                _logger.LogInformation("Retrieved {Count} products", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                throw;
            }
        }

        public async Task<CategoryProductDto?> GetProductByIdAsync(Guid id)
        {
            try
            {
                // ✅ ACTUALIZADO: Convertir Guid del DTO a long de la entidad
                var longId = ExtractLongFromGuid(id);
                
                var product = await _context.CategoryProducts
                    .Include(p => p.ActivityCategory)
                    .FirstOrDefaultAsync(p => p.Id == longId);

                if (product == null)
                {
                    return null;
                }

                var allCategories = await _context.ActivityCategories.OrderBy(ac => ac.CreatedAt).ToListAsync();

                return new CategoryProductDto
                {
                    Id = id,
                    ActivityCategoryId = GetCategoryOrder(product.ActivityCategoryId, allCategories),
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                throw;
            }
        }

        public async Task<CategoryProductDto> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                var categoryId = await GetActivityCategoryIdByOrder(request.ActivityCategoryId);
                if (!categoryId.HasValue)
                {
                    throw new ArgumentException($"Activity category with order {request.ActivityCategoryId} not found");
                }

                var product = new CategoryProduct
                {
                    ActivityCategoryId = categoryId.Value, // long -> long
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    UnitPrice = request.UnitPrice,
                    InitialQuantity = request.InitialQuantity,
                    CurrentQuantity = request.InitialQuantity,
                    AlertQuantity = request.AlertQuantity,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CategoryProducts.Add(product);

                // Create initial stock movement if there's initial quantity
                if (request.InitialQuantity > 0)
                {
                    var stockInType = await _context.InventoryMovementTypes
                        .FirstOrDefaultAsync(t => t.Name == "Stock In");

                    if (stockInType != null)
                    {
                        var movement = new InventoryMovement
                        {
                            ProductId = product.Id, // long -> long
                            MovementTypeId = stockInType.Id, // long -> long
                            Quantity = request.InitialQuantity,
                            PreviousQuantity = 0,
                            NewQuantity = request.InitialQuantity,
                            MovementDate = DateTime.UtcNow,
                            Justification = "Initial stock",
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.InventoryMovements.Add(movement);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new product: {ProductName} with ID {ProductId}", 
                    product.Name, product.Id);

                // ✅ ACTUALIZADO: Generar Guid para el DTO de respuesta
                var responseGuid = GenerateGuidFromLong(product.Id);
                return await GetProductByIdAsync(responseGuid) ?? 
                    throw new InvalidOperationException("Failed to retrieve created product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<CategoryProductDto?> UpdateProductAsync(Guid id, CreateProductRequest request)
        {
            try
            {
                // ✅ ACTUALIZADO: Convertir Guid del DTO a long de la entidad
                var longId = ExtractLongFromGuid(id);
                
                var product = await _context.CategoryProducts.FindAsync(longId);
                if (product == null)
                {
                    return null;
                }

                var categoryId = await GetActivityCategoryIdByOrder(request.ActivityCategoryId);
                if (!categoryId.HasValue)
                {
                    throw new ArgumentException($"Activity category with order {request.ActivityCategoryId} not found");
                }

                var oldQuantity = product.CurrentQuantity;

                product.ActivityCategoryId = categoryId.Value; // long -> long
                product.Code = request.Code;
                product.Name = request.Name;
                product.Description = request.Description;
                product.UnitPrice = request.UnitPrice;
                product.AlertQuantity = request.AlertQuantity;
                product.UpdatedAt = DateTime.UtcNow;

                if (request.InitialQuantity != product.InitialQuantity)
                {
                    product.InitialQuantity = request.InitialQuantity;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated product: {ProductId}", id);

                return await GetProductByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            try
            {
                // ✅ ACTUALIZADO: Convertir Guid del DTO a long de la entidad
                var longId = ExtractLongFromGuid(id);
                
                var product = await _context.CategoryProducts.FindAsync(longId);
                if (product == null)
                {
                    return false;
                }

                // Soft delete
                product.Active = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted product: {ProductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                throw;
            }
        }

        public async Task<List<CategoryProductDto>> GetLowStockProductsAsync()
        {
            try
            {
                var products = await _context.CategoryProducts
                    .Where(p => p.Active && p.CurrentQuantity <= p.AlertQuantity)
                    .OrderBy(p => p.CurrentQuantity)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                var allCategories = await _context.ActivityCategories.OrderBy(ac => ac.CreatedAt).ToListAsync();
                var result = new List<CategoryProductDto>();

                foreach (var p in products)
                {
                    result.Add(new CategoryProductDto
                    {
                        Id = GenerateGuidFromLong(p.Id), // ✅ ACTUALIZADO: long -> Guid
                        ActivityCategoryId = GetCategoryOrder(p.ActivityCategoryId, allCategories),
                        Code = p.Code,
                        Name = p.Name,
                        Description = p.Description,
                        UnitPrice = p.UnitPrice,
                        InitialQuantity = p.InitialQuantity,
                        CurrentQuantity = p.CurrentQuantity,
                        AlertQuantity = p.AlertQuantity,
                        Active = p.Active,
                        CreatedAt = p.CreatedAt
                    });
                }

                _logger.LogInformation("Found {Count} low stock products", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock products");
                throw;
            }
        }

        public async Task<bool> UpdateStockAsync(Guid productId, int newQuantity, string? reason = null)
        {
            try
            {
                // ✅ ACTUALIZADO: Convertir Guid del DTO a long de la entidad
                var longId = ExtractLongFromGuid(productId);
                
                var product = await _context.CategoryProducts.FindAsync(longId);
                if (product == null)
                {
                    return false;
                }

                var oldQuantity = product.CurrentQuantity;
                var difference = newQuantity - oldQuantity;

                product.CurrentQuantity = newQuantity;
                product.UpdatedAt = DateTime.UtcNow;

                // Create inventory movement record
                var adjustmentType = await _context.InventoryMovementTypes
                    .FirstOrDefaultAsync(t => t.Name == "Adjustment");

                if (adjustmentType != null)
                {
                    var movement = new InventoryMovement
                    {
                        ProductId = longId, // long -> long
                        MovementTypeId = adjustmentType.Id, // long -> long
                        Quantity = difference,
                        PreviousQuantity = oldQuantity,
                        NewQuantity = newQuantity,
                        MovementDate = DateTime.UtcNow,
                        Justification = reason ?? "Manual stock adjustment",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(movement);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated stock for product {ProductId}: {OldQuantity} -> {NewQuantity}", 
                    productId, oldQuantity, newQuantity);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
                throw;
            }
        }

        public async Task<List<InventoryMovementDto>> GetInventoryMovementsAsync(Guid? productId = null)
        {
            try
            {
                var query = _context.InventoryMovements
                    .Include(m => m.Product)
                    .Include(m => m.MovementType)
                    .AsQueryable();

                if (productId.HasValue)
                {
                    // ✅ ACTUALIZADO: Convertir Guid del DTO a long de la entidad
                    var longId = ExtractLongFromGuid(productId.Value);
                    query = query.Where(m => m.ProductId == longId);
                }

                var movements = await query
                    .OrderByDescending(m => m.MovementDate)
                    .Take(100)
                    .ToListAsync();

                var allMovementTypes = await _context.InventoryMovementTypes.OrderBy(mt => mt.CreatedAt).ToListAsync();
                var result = new List<InventoryMovementDto>();

                foreach (var m in movements)
                {
                    result.Add(new InventoryMovementDto
                    {
                        Id = GenerateGuidFromLong(m.Id), // ✅ ACTUALIZADO: long -> Guid
                        ProductId = GenerateGuidFromLong(m.ProductId), // ✅ ACTUALIZADO: long -> Guid
                        ProductName = m.Product.Name,
                        MovementTypeId = GetMovementTypeOrder(m.MovementTypeId, allMovementTypes),
                        MovementTypeName = m.MovementType.Name,
                        Quantity = m.Quantity,
                        PreviousQuantity = m.PreviousQuantity,
                        NewQuantity = m.NewQuantity,
                        UnitCost = m.UnitCost,
                        TotalValue = m.TotalValue,
                        Justification = m.Justification,
                        MovementDate = m.MovementDate
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory movements");
                throw;
            }
        }

        // ============================================
        // MÉTODOS HELPER PARA MAPEO DE TIPOS Y IDs
        // ============================================

        /// <summary>
        /// ✅ ACTUALIZADO: Retorna long en lugar de int
        /// </summary>
        private async Task<long?> GetActivityCategoryIdByOrder(int order)
        {
            var categories = await _context.ActivityCategories
                .OrderBy(ac => ac.CreatedAt)
                .ToListAsync();

            return order > 0 && order <= categories.Count 
                ? categories[order - 1].Id 
                : null;
        }

        /// <summary>
        /// ✅ ACTUALIZADO: Acepta long en lugar de int
        /// </summary>
        private static int GetCategoryOrder(long categoryId, List<ActivityCategory> categories)
        {
            var index = categories.FindIndex(c => c.Id == categoryId);
            return index >= 0 ? index + 1 : 0;
        }

        /// <summary>
        /// ✅ ACTUALIZADO: Acepta long en lugar de int
        /// </summary>
        private static int GetMovementTypeOrder(long movementTypeId, List<InventoryMovementType> types)
        {
            var index = types.FindIndex(t => t.Id == movementTypeId);
            return index >= 0 ? index + 1 : 0;
        }

        // ============================================
        // MÉTODOS HELPER PARA MAPEO LONG <-> GUID
        // ============================================

        /// <summary>
        /// ✅ ACTUALIZADO: Convierte long a Guid determinístico
        /// </summary>
        private static Guid GenerateGuidFromLong(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            
            // Copiar los 8 bytes del long a los primeros 8 bytes del Guid
            Array.Copy(longBytes, 0, bytes, 0, 8);
            
            // Llenar el resto con un patrón determinístico
            for (int i = 8; i < 16; i++)
            {
                bytes[i] = (byte)((longId >> ((i - 8) * 8)) % 256);
            }
            
            return new Guid(bytes);
        }

        /// <summary>
        /// ✅ ACTUALIZADO: Extrae long desde Guid
        /// </summary>
        private static long ExtractLongFromGuid(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}