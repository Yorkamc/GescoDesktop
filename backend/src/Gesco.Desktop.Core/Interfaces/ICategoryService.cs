using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    public interface ICategoryService
    {
        // Service Categories (Catálogos globales)
        Task<List<ServiceCategoryDto>> GetServiceCategoriesAsync(Guid? organizationId = null);
        Task<ServiceCategoryDto?> GetServiceCategoryByIdAsync(Guid id);
        Task<ServiceCategoryDto> CreateServiceCategoryAsync(CreateServiceCategoryRequest request);
        Task<ServiceCategoryDto?> UpdateServiceCategoryAsync(Guid id, CreateServiceCategoryRequest request);
        Task<bool> DeleteServiceCategoryAsync(Guid id);

        // Activity Categories (Relación Actividad-Categoría)
        Task<List<ActivityCategoryDto>> GetActivityCategoriesAsync(Guid? activityId = null);
        Task<ActivityCategoryDto?> GetActivityCategoryByIdAsync(Guid id);
        Task<ActivityCategoryDto> CreateActivityCategoryAsync(CreateActivityCategoryRequest request);
        Task<bool> DeleteActivityCategoryAsync(Guid id);
    }
}