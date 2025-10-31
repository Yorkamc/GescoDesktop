using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    public interface IProductService
    {
        Task<List<CategoryProductDto>> GetProductsAsync(int? activityCategoryId = null);
        Task<CategoryProductDto?> GetProductByIdAsync(Guid id);
        Task<CategoryProductDto> CreateProductAsync(CreateProductRequest request);
        Task<CategoryProductDto?> UpdateProductAsync(Guid id, CreateProductRequest request);
        Task<bool> DeleteProductAsync(Guid id);
        Task<List<CategoryProductDto>> GetLowStockProductsAsync();
        Task<bool> UpdateStockAsync(Guid productId, int newQuantity, string? reason = null);
        Task<List<InventoryMovementDto>> GetInventoryMovementsAsync(Guid? productId = null);
        Task<List<CategoryProductDto>> GetUnassignedProductsAsync();
    }
}