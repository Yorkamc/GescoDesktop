using System.Threading.Tasks;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResultDto> LoginAsync(string usuario, string password);
        Task LogoutAsync();
        Task<bool> ValidateTokenAsync(string token);
        Task<UsuarioDto?> GetCurrentUserAsync();
    }
}
