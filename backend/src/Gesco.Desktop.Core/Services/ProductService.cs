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

                // CORREGIDO: Manejar filtro por categoría correctamente
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
                        Id = GenerateGuidFromInt(p.Id), // Mapear int ID a Guid para DTO
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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(id);
                
                var product = await _context.CategoryProducts
                    .Include(p => p.ActivityCategory)
                    .FirstOrDefaultAsync(p => p.Id == intId);

                if (product == null)
                {
                    return null;
                }

                var allCategories = await _context.ActivityCategories.OrderBy(ac => ac.CreatedAt).ToListAsync();

                return new CategoryProductDto
                {
                    Id = id, // Mantener el Guid del parámetro
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
                // CORREGIDO: Convertir order a ID de categoría
                var categoryId = await GetActivityCategoryIdByOrder(request.ActivityCategoryId);
                if (!categoryId.HasValue)
                {
                    throw new ArgumentException($"Activity category with order {request.ActivityCategoryId} not found");
                }

                var product = new CategoryProduct
                {
                    // Id se genera automáticamente como int
                    ActivityCategoryId = categoryId.Value, // int -> int (correcto)
                    Code = request.Code,
                    Name = request.Name,
                    Description = request.Description,
                    UnitPrice = request.UnitPrice,
                    InitialQuantity = request.InitialQuantity,
                    CurrentQuantity = request.InitialQuantity, // Start with initial quantity
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
                            // Id se genera automáticamente como int
                            ProductId = product.Id, // int -> int (correcto)
                            MovementTypeId = stockInType.Id, // int -> int (correcto)
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

                // Generar Guid para el DTO de respuesta
                var responseGuid = GenerateGuidFromInt(product.Id);
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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(id);
                
                var product = await _context.CategoryProducts.FindAsync(intId);
                if (product == null)
                {
                    return null;
                }

                // CORREGIDO: Convertir order a ID de categoría
                var categoryId = await GetActivityCategoryIdByOrder(request.ActivityCategoryId);
                if (!categoryId.HasValue)
                {
                    throw new ArgumentException($"Activity category with order {request.ActivityCategoryId} not found");
                }

                var oldQuantity = product.CurrentQuantity;

                product.ActivityCategoryId = categoryId.Value; // int -> int (correcto)
                product.Code = request.Code;
                product.Name = request.Name;
                product.Description = request.Description;
                product.UnitPrice = request.UnitPrice;
                product.AlertQuantity = request.AlertQuantity;
                product.UpdatedAt = DateTime.UtcNow;

                // If initial quantity changed, update current quantity proportionally
                if (request.InitialQuantity != product.InitialQuantity)
                {
                    product.InitialQuantity = request.InitialQuantity;
                    // You might want to implement logic here for updating current quantity
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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(id);
                
                var product = await _context.CategoryProducts.FindAsync(intId);
                if (product == null)
                {
                    return false;
                }

                // Soft delete - just mark as inactive
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
                        Id = GenerateGuidFromInt(p.Id), // Mapear int ID a Guid
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
                // Convertir Guid del DTO a int de la entidad
                var intId = ExtractIntFromGuid(productId);
                
                var product = await _context.CategoryProducts.FindAsync(intId);
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
                        // Id se genera automáticamente como int
                        ProductId = intId, // int -> int (correcto)
                        MovementTypeId = adjustmentType.Id, // int -> int (correcto)
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
                    // Convertir Guid del DTO a int de la entidad
                    var intId = ExtractIntFromGuid(productId.Value);
                    query = query.Where(m => m.ProductId == intId);
                }

                var movements = await query
                    .OrderByDescending(m => m.MovementDate)
                    .Take(100) // Limit to last 100 movements
                    .ToListAsync();

                var allMovementTypes = await _context.InventoryMovementTypes.OrderBy(mt => mt.CreatedAt).ToListAsync();
                var result = new List<InventoryMovementDto>();

                foreach (var m in movements)
                {
                    result.Add(new InventoryMovementDto
                    {
                        Id = GenerateGuidFromInt(m.Id), // Mapear int ID a Guid
                        ProductId = GenerateGuidFromInt(m.ProductId), // Mapear int ID a Guid
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

        // MÉTODOS HELPER PARA MAPEO DE TIPOS Y IDs
        private async Task<int?> GetActivityCategoryIdByOrder(int order)
        {
            var categories = await _context.ActivityCategories
                .OrderBy(ac => ac.CreatedAt)
                .ToListAsync();

            return order > 0 && order <= categories.Count 
                ? categories[order - 1].Id 
                : null;
        }

        private static int GetCategoryOrder(int categoryId, List<ActivityCategory> categories)
        {
            var index = categories.FindIndex(c => c.Id == categoryId);
            return index >= 0 ? index + 1 : 0;
        }

        private static int GetMovementTypeOrder(int movementTypeId, List<InventoryMovementType> types)
        {
            var index = types.FindIndex(t => t.Id == movementTypeId);
            return index >= 0 ? index + 1 : 0;
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