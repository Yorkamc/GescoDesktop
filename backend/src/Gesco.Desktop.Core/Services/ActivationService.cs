using System;
using System.Linq;
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
                var orgGuid = await GetOrganizationGuidById(organizacionId);
                if (orgGuid == null)
                {
                    return new ActivationResultDto
                    {
                        Success = false,
                        Message = "Organizacion no encontrada"
                    };
                }

                var existingActivation = await _context.ActivationKeys
                    .FirstOrDefaultAsync(ak => ak.ActivationCode == codigoActivacion);

                if (existingActivation != null && existingActivation.IsUsed)
                {
                    return new ActivationResultDto
                    {
                        Success = false,
                        Message = "Este codigo de activacion ya esta en uso"
                    };
                }

                try
                {
                    var remoteResult = await _laravelApiClient.ActivateAsync(codigoActivacion, organizacionId);
                    if (remoteResult.Success)
                    {
                        await SaveActivationAsync(codigoActivacion, orgGuid.Value, remoteResult.FechaExpiracion ?? DateTime.UtcNow.AddYears(1));
                        return remoteResult;
                    }
                }
                catch
                {
                    // Continue with offline activation
                }

                var expirationDate = DateTime.UtcNow.AddMonths(3);
                await SaveActivationAsync(codigoActivacion, orgGuid.Value, expirationDate);

                return new ActivationResultDto
                {
                    Success = true,
                    Message = "Licencia activada correctamente (modo offline)",
                    FechaExpiracion = expirationDate,
                    DiasRestantes = (int)(expirationDate - DateTime.UtcNow).TotalDays
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
                var activation = await _context.ActivationKeys
                    .Where(ak => ak.IsUsed && !ak.IsExpired && !ak.IsRevoked)
                    .OrderByDescending(ak => ak.GeneratedDate)
                    .FirstOrDefaultAsync();

                if (activation == null)
                {
                    return new LicenseStatusDto
                    {
                        IsActive = false,
                        Message = "No hay licencia activa",
                        DiasRestantes = 0,
                        MaxUsuarios = 0
                    };
                }

                int diasRestantes = 0;
                bool isActive = false;

                if (activation.ExpirationDate.HasValue)
                {
                    var timeSpan = activation.ExpirationDate.Value - DateTime.UtcNow;
                    diasRestantes = Math.Max(0, (int)timeSpan.TotalDays);
                    isActive = DateTime.UtcNow <= activation.ExpirationDate.Value;
                }

                return new LicenseStatusDto
                {
                    IsActive = isActive,
                    Message = isActive ? "Licencia activa" : "Licencia expirada",
                    FechaActivacion = activation.UsedDate,
                    FechaExpiracion = activation.ExpirationDate,
                    DiasRestantes = diasRestantes,
                    MaxUsuarios = 10,
                    OrganizacionId = await GetOrganizationIdByGuid(activation.UsedByOrganizationId)
                };
            }
            catch (Exception ex)
            {
                return new LicenseStatusDto
                {
                    IsActive = false,
                    Message = $"Error al verificar licencia: {ex.Message}",
                    DiasRestantes = 0,
                    MaxUsuarios = 0
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

        private async Task SaveActivationAsync(string codigoActivacion, Guid organizacionId, DateTime fechaExpiracion)
        {
            var existingActivation = await _context.ActivationKeys
                .FirstOrDefaultAsync(ak => ak.ActivationCode == codigoActivacion);

            if (existingActivation != null)
            {
                existingActivation.UsedDate = DateTime.UtcNow;
                existingActivation.ExpirationDate = fechaExpiracion;
                existingActivation.IsUsed = true;
                existingActivation.IsExpired = false;
                existingActivation.IsRevoked = false;
                existingActivation.UsedByOrganizationId = organizacionId;
                existingActivation.CurrentUses = 1;
                existingActivation.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var defaultSubscription = await _context.Subscriptions.FirstOrDefaultAsync();
                var subscriptionId = defaultSubscription?.Id ?? 1;

                var activation = new Gesco.Desktop.Data.Entities.ActivationKey
                {
                    ActivationCode = codigoActivacion,
                    SubscriptionId = subscriptionId,
                    GeneratedDate = DateTime.UtcNow,
                    ExpirationDate = fechaExpiracion,
                    UsedDate = DateTime.UtcNow,
                    IsGenerated = true,
                    IsUsed = true,
                    IsExpired = false,
                    IsRevoked = false,
                    MaxUses = 1,
                    CurrentUses = 1,
                    UsedByOrganizationId = organizacionId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ActivationKeys.Add(activation);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Guid?> GetOrganizationGuidById(int organizacionId)
        {
            var org = await _context.Organizations.FirstOrDefaultAsync();
            return org?.Id;
        }

        private Task<int?> GetOrganizationIdByGuid(Guid? organizationGuid)
        {
            if (!organizationGuid.HasValue) 
                return Task.FromResult<int?>(null);
            
            return Task.FromResult<int?>(1);
        }
    }
}