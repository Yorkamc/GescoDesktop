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
                    query = query.Where(p => p.ActivityCategoryId == activityCategoryId);
                }

                var products = await query
                    .Where(p => p.Active)
                    .OrderBy(p => p.Name)
                    .Select(p => new CategoryProductDto
                    {
                        Id = p.Id,
                        ActivityCategoryId = p.ActivityCategoryId,
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

                _logger.LogInformation("Retrieved {Count} products", products.Count);
                return products;
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
                var product = await _context.CategoryProducts
                    .Include(p => p.ActivityCategory)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return null;
                }

                return new CategoryProductDto
                {
                    Id = product.Id,
                    ActivityCategoryId = product.ActivityCategoryId,
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
                var product = new CategoryProduct
                {
                    Id = Guid.NewGuid(),
                    ActivityCategoryId = request.ActivityCategoryId,
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
                            Id = Guid.NewGuid(),
                            ProductId = (int)product.Id.GetHashCode(), // Convert Guid to int (temporary)
                            MovementTypeId = (int)stockInType.Id.GetHashCode(),
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

                return await GetProductByIdAsync(product.Id) ?? 
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
                var product = await _context.CategoryProducts.FindAsync(id);
                if (product == null)
                {
                    return null;
                }

                var oldQuantity = product.CurrentQuantity;

                product.ActivityCategoryId = request.ActivityCategoryId;
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
                var product = await _context.CategoryProducts.FindAsync(id);
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
                    .Select(p => new CategoryProductDto
                    {
                        Id = p.Id,
                        ActivityCategoryId = p.ActivityCategoryId,
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

                _logger.LogInformation("Found {Count} low stock products", products.Count);
                return products;
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
                var product = await _context.CategoryProducts.FindAsync(productId);
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
                        Id = Guid.NewGuid(),
                        ProductId = (int)productId.GetHashCode(), // Convert Guid to int (temporary)
                        MovementTypeId = (int)adjustmentType.Id.GetHashCode(),
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
                    var productIntId = (int)productId.Value.GetHashCode(); // Convert Guid to int
                    query = query.Where(m => m.ProductId == productIntId);
                }

                var movements = await query
                    .OrderByDescending(m => m.MovementDate)
                    .Select(m => new InventoryMovementDto
                    {
                        Id = m.Id,
                        ProductId = m.ProductId,
                        ProductName = m.Product.Name,
                        MovementTypeId = m.MovementTypeId,
                        MovementTypeName = m.MovementType.Name,
                        Quantity = m.Quantity,
                        PreviousQuantity = m.PreviousQuantity,
                        NewQuantity = m.NewQuantity,
                        UnitCost = m.UnitCost,
                        TotalValue = m.TotalValue,
                        Justification = m.Justification,
                        MovementDate = m.MovementDate
                    })
                    .Take(100) // Limit to last 100 movements
                    .ToListAsync();

                return movements;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory movements");
                throw;
            }
        }
        
    }
}