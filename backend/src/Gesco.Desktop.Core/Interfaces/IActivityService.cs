
using Gesco.Desktop.Shared.DTOs;
namespace Gesco.Desktop.Core.Interfaces
{
    public interface IActivityService
    {
        Task<List<ActivityDto>> GetActivitiesAsync(Guid? organizationId = null);
        Task<ActivityDto?> GetActivityByIdAsync(Guid id);
        Task<ActivityDto> CreateActivityAsync(CreateActivityRequest request);
        Task<ActivityDto?> UpdateActivityAsync(Guid id, CreateActivityRequest request);
        Task<bool> DeleteActivityAsync(Guid id);
        Task<List<ActivityDto>> GetActiveActivitiesAsync();
        Task<DashboardStatsDto> GetActivityStatsAsync();
    }
}