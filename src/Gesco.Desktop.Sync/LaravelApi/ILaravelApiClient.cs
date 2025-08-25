using System.Threading.Tasks;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Sync.LaravelApi
{
    public interface ILaravelApiClient
    {
        Task<bool> IsConnectedAsync();
        Task<LoginResultDto> LoginAsync(string usuario, string password);
        Task<ActivationResultDto> ActivateAsync(string codigo, int organizacionId);
        Task<bool> ValidateLicenseAsync(string codigo);
    }
}
