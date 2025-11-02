using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    public interface ISalesComboService
    {
        Task<List<SalesComboDto>> GetCombosAsync(Guid? activityId = null);
        Task<SalesComboDto?> GetComboByIdAsync(Guid id);
        Task<SalesComboDto> CreateComboAsync(CreateSalesComboRequest request);
        Task<SalesComboDto?> UpdateComboAsync(Guid id, CreateSalesComboRequest request);
        Task<bool> DeleteComboAsync(Guid id);
        Task<bool> ToggleComboActiveAsync(Guid id);
        Task<List<SalesComboDto>> GetActiveCombosByActivityAsync(Guid activityId);
    }
}