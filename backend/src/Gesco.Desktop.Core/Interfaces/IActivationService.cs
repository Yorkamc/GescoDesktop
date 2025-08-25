using System.Threading.Tasks;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    public interface IActivationService
    {
        Task<ActivationResultDto> ActivateAsync(string codigoActivacion, int organizacionId);
        Task<LicenseStatusDto> GetLicenseStatusAsync();
        Task<bool> CheckLicenseStatusAsync();
        Task<bool> ValidateLicenseAsync();
    }
}
