
namespace Gesco.Desktop.Core.Interfaces
{
    public interface  IMigrationService
    {
        Task EnsureDatabaseCreatedAsync();
        Task RunOptimizationScriptAsync();
    }
}