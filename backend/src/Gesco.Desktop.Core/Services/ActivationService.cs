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
                // Verificar si ya existe una activación para este código
                var existingActivation = await _context.ClavesActivacion
                    .FirstOrDefaultAsync(ca => ca.CodigoActivacion == codigoActivacion);

                if (existingActivation != null && existingActivation.Utilizada)
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
                    .Where(ca => ca.Utilizada && !ca.Expirada && !ca.Revocada)
                    .OrderByDescending(ca => ca.FechaGeneracion)
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

                // Corregir el manejo de nullable - verificar si FechaExpiracion tiene valor
                int diasRestantes = 0;
                bool isActive = false;

                if (activation.FechaExpiracion.HasValue)
                {
                    var timeSpan = activation.FechaExpiracion.Value - DateTime.Now;
                    diasRestantes = Math.Max(0, (int)timeSpan.TotalDays);
                    isActive = DateTime.Now <= activation.FechaExpiracion.Value;
                }

                return new LicenseStatusDto
                {
                    IsActive = isActive,
                    Message = isActive ? "Licencia activa" : "Licencia expirada",
                    FechaActivacion = activation.FechaUtilizacion,
                    FechaExpiracion = activation.FechaExpiracion,
                    DiasRestantes = diasRestantes,
                    MaxUsuarios = 10, // Valor por defecto
                    OrganizacionId = activation.UtilizadaPorOrganizacionId
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

        private async Task SaveActivationAsync(string codigoActivacion, int organizacionId, DateTime fechaExpiracion)
        {
            // Verificar si ya existe una entrada para este código
            var existingActivation = await _context.ClavesActivacion
                .FirstOrDefaultAsync(ca => ca.CodigoActivacion == codigoActivacion);

            if (existingActivation != null)
            {
                // Actualizar activación existente
                existingActivation.FechaUtilizacion = DateTime.Now;
                existingActivation.FechaExpiracion = fechaExpiracion;
                existingActivation.Utilizada = true;
                existingActivation.Expirada = false;
                existingActivation.Revocada = false;
                existingActivation.UtilizadaPorOrganizacionId = organizacionId;
                existingActivation.UsosActuales = 1;
            }
            else
            {
                // Crear nueva activación
                var activation = new Data.Entities.ClaveActivacion
                {
                    CodigoActivacion = codigoActivacion,
                    FechaGeneracion = DateTime.Now,
                    FechaExpiracion = fechaExpiracion,
                    FechaUtilizacion = DateTime.Now,
                    Utilizada = true,
                    Expirada = false,
                    Revocada = false,
                    UsosMaximos = 1,
                    UsosActuales = 1,
                    UtilizadaPorOrganizacionId = organizacionId,
                    SuscripcionesId = 1 // Valor por defecto
                };

                _context.ClavesActivacion.Add(activation);
            }

            await _context.SaveChangesAsync();
        }
    }
}