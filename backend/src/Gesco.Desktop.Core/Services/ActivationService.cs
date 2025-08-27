using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Sync.LaravelApi;

namespace Gesco.Desktop.Core.Services
{
    public class ActivationService : IActivationService
    {
        private readonly LocalDbContext _context;
        private readonly ILaravelApiClient _laravelApiClient;

        public ActivationService(LocalDbContext context, ILaravelApiClient laravelApiClient)
        {
            _context = context;
            _laravelApiClient = laravelApiClient;
        }

        public async Task<ActivationResultDto> ActivateAsync(string codigoActivacion, int organizacionId)
        {
            try
            {
                // Verificar si ya existe una activación para este código
                var existingActivation = await _context.ClavesActivacion
                    .FirstOrDefaultAsync(ca => ca.CodigoActivacion == codigoActivacion);

                if (existingActivation != null && existingActivation.Expirada)
                {
                    return new ActivationResultDto
                    {
                        Success = false,
                        Message = "Este código de activación ya está en uso"
                    };
                }

                // Intentar activar con el servidor Laravel (si está disponible)
                try
                {
                    var remoteResult = await _laravelApiClient.ActivateAsync(codigoActivacion, organizacionId);
                    if (remoteResult.Success)
                    {
                        // Guardar activación local
                        await SaveActivationAsync(codigoActivacion, organizacionId, remoteResult.FechaExpiracion ?? DateTime.Now.AddYears(1));
                        return remoteResult;
                    }
                }
                catch
                {
                    // Si falla la conexión remota, continuar con activación offline
                }

                // Activación offline (para desarrollo)
                var expirationDate = DateTime.Now.AddMonths(3); // 3 meses de prueba
                await SaveActivationAsync(codigoActivacion, organizacionId, expirationDate);

                return new ActivationResultDto
                {
                    Success = true,
                    Message = "Licencia activada correctamente (modo offline)",
                    FechaExpiracion = expirationDate,
                    DiasRestantes = (int)(expirationDate - DateTime.Now).TotalDays
                };
            }
            catch (Exception ex)
            {
                return new ActivationResultDto
                {
                    Success = false,
                    Message = $"Error al activar licencia: {ex.Message}"
                };
            }
        }

        public async Task<LicenseStatusDto> GetLicenseStatusAsync()
        {
            try
            {
                var activation = await _context.ClavesActivacion
                    .Where(ca => ca.Expirada)
                    .OrderByDescending(ca => ca.FechaGeneracion)
                    .FirstOrDefaultAsync();

                if (activation == null)
                {
                    return new LicenseStatusDto
                    {
                        IsActive = false,
                        Message = "No hay licencia activa",
                        DiasRestantes = 0
                    };
                }

                var diasRestantes = (int)(activation.FechaExpiracion - DateTime.Now).TotalDays;
                var isActive = DateTime.Now <= activation.FechaExpiracion;

                return new LicenseStatusDto
                {
                    IsActive = isActive,
                    Message = isActive ? "Licencia activa" : "Licencia expirada",
                    FechaActivacion = activation.FechaGeneracion,
                    FechaExpiracion = activation.FechaExpiracion,
                    DiasRestantes = Math.Max(0, diasRestantes),
                    OrganizacionId = activation.GeneradaPorUsuario?.OrganizacionId
                };
            }
            catch (Exception ex)
            {
                return new LicenseStatusDto
                {
                    IsActive = false,
                    Message = $"Error al verificar licencia: {ex.Message}"
                };
            }
        }

        public async Task<bool> CheckLicenseStatusAsync()
        {
            var status = await GetLicenseStatusAsync();
            return status.IsActive;
        }

        public async Task<bool> ValidateLicenseAsync()
        {
            return await CheckLicenseStatusAsync();
        }

        private async Task SaveActivationAsync(string codigoActivacion, int organizacionId, DateTime fechaExpiracion)
        {
            var activation = new Data.Entities.ClaveActivacion
            {
                CodigoActivacion = codigoActivacion,
                FechaExpiracion = fechaExpiracion
            };

            _context.ClavesActivacion.Add(activation);
            await _context.SaveChangesAsync();
        }
    }
}
