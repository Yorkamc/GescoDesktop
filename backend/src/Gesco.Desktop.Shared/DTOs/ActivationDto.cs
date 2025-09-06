using System;

namespace Gesco.Desktop.Shared.DTOs
{
    public class ActivationResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public int DiasRestantes { get; set; }
    }

    public class ActivationRequest
    {
        public string? CodigoActivacion { get; set; }
        public int OrganizacionId { get; set; }
        public string? DispositivoId { get; set; }
        public string? NombreDispositivo { get; set; }
    }

    public class LicenseStatusDto
    {
        public bool IsActive { get; set; }
        public string? Message { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public int DiasRestantes { get; set; }
        public int MaxUsuarios { get; set; }
        public int? OrganizacionId { get; set; }
    }

}
