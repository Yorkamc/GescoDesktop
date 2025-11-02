using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.Core.Services
{
    public class SalesComboService : ISalesComboService
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<SalesComboService> _logger;

        public SalesComboService(LocalDbContext context, ILogger<SalesComboService> logger)
        {
            _context = context;
            _logger = logger;
        }

       public async Task<List<SalesComboDto>> GetCombosAsync(Guid? activityId = null)
{
    try
    {
        var query = _context.SalesCombos.AsQueryable();

        if (activityId.HasValue)
        {
            var longActivityId = MapGuidToLong(activityId.Value);
            query = query.Where(c => c.ActivityId == longActivityId);
        }

        // ✅ Cargar datos sin includes problemáticos
        var combos = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        // ✅ Mapear manualmente para evitar lazy loading
        var result = new List<SalesComboDto>();
        
        foreach (var combo in combos)
        {
            // Cargar actividad separadamente
            var activity = await _context.Activities.FindAsync(combo.ActivityId);
            
            // Cargar items del combo
            var comboItems = await _context.ComboItems
                .Where(ci => ci.ComboId == combo.Id)
                .ToListAsync();
            
            var items = new List<ComboItemDto>();
            foreach (var ci in comboItems)
            {
                // Cargar producto
                var product = await _context.CategoryProducts.FindAsync(ci.ProductId);
                
                items.Add(new ComboItemDto
                {
                    Id = MapLongToGuid(ci.Id),
                    ProductId = MapLongToGuid(ci.ProductId),
                    ProductName = product?.Name ?? "Producto no encontrado",
                    ProductPrice = product?.UnitPrice ?? 0m,
                    Quantity = ci.Quantity
                });
            }
            
            result.Add(new SalesComboDto
            {
                Id = MapLongToGuid(combo.Id),
                ActivityId = MapLongToGuid(combo.ActivityId),
                ActivityName = activity?.Name ?? "Actividad no encontrada",
                Name = combo.Name,
                Description = combo.Description,
                ComboPrice = combo.ComboPrice,
                Active = combo.Active,
                CreatedAt = combo.CreatedAt,
                Items = items
            });
        }

        _logger.LogInformation("Retrieved {Count} combos", result.Count);
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving combos: {Message}", ex.Message);
        throw;
    }
}

public async Task<SalesComboDto?> GetComboByIdAsync(Guid id)
{
    try
    {
        var longId = MapGuidToLong(id);
        var combo = await _context.SalesCombos.FindAsync(longId);
        
        if (combo == null) return null;

        // Cargar actividad
        var activity = await _context.Activities.FindAsync(combo.ActivityId);
        
        // Cargar items
        var comboItems = await _context.ComboItems
            .Where(ci => ci.ComboId == combo.Id)
            .ToListAsync();
        
        var items = new List<ComboItemDto>();
        foreach (var ci in comboItems)
        {
            var product = await _context.CategoryProducts.FindAsync(ci.ProductId);
            
            items.Add(new ComboItemDto
            {
                Id = MapLongToGuid(ci.Id),
                ProductId = MapLongToGuid(ci.ProductId),
                ProductName = product?.Name ?? "Producto no encontrado",
                ProductPrice = product?.UnitPrice ?? 0m,
                Quantity = ci.Quantity
            });
        }
        
        return new SalesComboDto
        {
            Id = MapLongToGuid(combo.Id),
            ActivityId = MapLongToGuid(combo.ActivityId),
            ActivityName = activity?.Name ?? "Actividad no encontrada",
            Name = combo.Name,
            Description = combo.Description,
            ComboPrice = combo.ComboPrice,
            Active = combo.Active,
            CreatedAt = combo.CreatedAt,
            Items = items
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving combo {Id}: {Message}", id, ex.Message);
        throw;
    }
}

        public async Task<SalesComboDto> CreateComboAsync(CreateSalesComboRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating new combo: {Name}", request.Name);

                // Validar actividad
                var activityId = MapGuidToLong(request.ActivityId);
                var activity = await _context.Activities.FindAsync(activityId);
                
                if (activity == null)
                {
                    throw new ArgumentException($"Activity {request.ActivityId} not found");
                }

                // Validar productos
                var productIds = request.Items.Select(i => MapGuidToLong(i.ProductId)).ToList();
                var products = await _context.CategoryProducts
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    throw new ArgumentException("One or more products not found");
                }

                // Verificar que todos los productos estén activos
                var inactiveProducts = products.Where(p => !p.Active).ToList();
                if (inactiveProducts.Any())
                {
                    throw new ArgumentException($"Products not active: {string.Join(", ", inactiveProducts.Select(p => p.Name))}");
                }

                // Crear combo
                var combo = new SalesCombo
                {
                    ActivityId = activityId,
                    Name = request.Name,
                    Description = request.Description,
                    ComboPrice = request.ComboPrice,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SalesCombos.Add(combo);
                await _context.SaveChangesAsync(); // Guardar para obtener el ID

                // Crear items del combo
                foreach (var item in request.Items)
                {
                    var comboItem = new ComboItem
                    {
                        ComboId = combo.Id,
                        ProductId = MapGuidToLong(item.ProductId),
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ComboItems.Add(comboItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Combo created: {Id} - {Name}", combo.Id, combo.Name);

                return await GetComboByIdAsync(MapLongToGuid(combo.Id))
                    ?? throw new InvalidOperationException("Failed to retrieve created combo");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating combo");
                throw;
            }
        }

        public async Task<SalesComboDto?> UpdateComboAsync(Guid id, CreateSalesComboRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var longId = MapGuidToLong(id);
                var combo = await _context.SalesCombos
                    .Include(c => c.ComboItems)
                    .FirstOrDefaultAsync(c => c.Id == longId);

                if (combo == null)
                    return null;

                _logger.LogInformation("Updating combo: {Id} - {Name}", id, request.Name);

                // Validar productos
                var productIds = request.Items.Select(i => MapGuidToLong(i.ProductId)).ToList();
                var products = await _context.CategoryProducts
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    throw new ArgumentException("One or more products not found");
                }

                // Actualizar datos del combo
                combo.Name = request.Name;
                combo.Description = request.Description;
                combo.ComboPrice = request.ComboPrice;
                combo.UpdatedAt = DateTime.UtcNow;

                // Eliminar items existentes
                _context.ComboItems.RemoveRange(combo.ComboItems);

                // Agregar nuevos items
                foreach (var item in request.Items)
                {
                    var comboItem = new ComboItem
                    {
                        ComboId = combo.Id,
                        ProductId = MapGuidToLong(item.ProductId),
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ComboItems.Add(comboItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Combo updated: {Id}", id);
                return await GetComboByIdAsync(id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating combo {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteComboAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                var combo = await _context.SalesCombos
                    .Include(c => c.ComboItems)
                    .Include(c => c.TransactionDetails)
                    .FirstOrDefaultAsync(c => c.Id == longId);

                if (combo == null)
                    return false;

                // Verificar si tiene ventas asociadas
                if (combo.TransactionDetails.Any())
                {
                    throw new InvalidOperationException("Cannot delete combo with associated sales. Consider deactivating instead.");
                }

                _context.ComboItems.RemoveRange(combo.ComboItems);
                _context.SalesCombos.Remove(combo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Combo deleted: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting combo {Id}", id);
                throw;
            }
        }

        public async Task<bool> ToggleComboActiveAsync(Guid id)
        {
            try
            {
                var longId = MapGuidToLong(id);
                var combo = await _context.SalesCombos.FindAsync(longId);

                if (combo == null)
                    return false;

                combo.Active = !combo.Active;
                combo.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Combo {Id} active status changed to: {Active}", id, combo.Active);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling combo active status {Id}", id);
                throw;
            }
        }

        public async Task<List<SalesComboDto>> GetActiveCombosByActivityAsync(Guid activityId)
        {
            try
            {
                var longActivityId = MapGuidToLong(activityId);
                
                var combos = await _context.SalesCombos
                    .Include(c => c.Activity)
                    .Include(c => c.ComboItems)
                        .ThenInclude(ci => ci.Product)
                    .Where(c => c.ActivityId == longActivityId && c.Active)
                    .OrderBy(c => c.Name)
                    .Select(c => MapToDto(c))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} active combos for activity {ActivityId}", combos.Count, activityId);
                return combos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active combos for activity {ActivityId}", activityId);
                throw;
            }
        }

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================

        private SalesComboDto MapToDto(SalesCombo combo)
        {
            return new SalesComboDto
            {
                Id = MapLongToGuid(combo.Id),
                ActivityId = MapLongToGuid(combo.ActivityId),
                ActivityName = combo.Activity?.Name ?? string.Empty,
                Name = combo.Name,
                Description = combo.Description,
                ComboPrice = combo.ComboPrice,
                Active = combo.Active,
                CreatedAt = combo.CreatedAt,
                Items = combo.ComboItems?.Select(ci => new ComboItemDto
                {
                    Id = MapLongToGuid(ci.Id),
                    ProductId = MapLongToGuid(ci.ProductId),
                    ProductName = ci.Product?.Name ?? string.Empty,
                    ProductPrice = ci.Product?.UnitPrice ?? 0m,
                    Quantity = ci.Quantity
                }).ToList() ?? new List<ComboItemDto>()
            };
        }

        private static Guid MapLongToGuid(long longId)
        {
            var bytes = new byte[16];
            var longBytes = BitConverter.GetBytes(longId);
            Array.Copy(longBytes, 0, bytes, 0, 8);
             bytes[8] = 0xC0; bytes[9] = 0xBB;
            return new Guid(bytes);
        }

        private static long MapGuidToLong(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}