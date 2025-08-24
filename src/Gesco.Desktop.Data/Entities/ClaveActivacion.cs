using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class ClaveActivacion
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CodigoActivacion { get; set; }
        
        public int TipoLicenciaId { get; set; }
        public int OrganizacionId { get; set; }
        public DateTime FechaActivacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activa { get; set; }
        public string DispositivoId { get; set; }
        public string NombreDispositivo { get; set; }
        public int MaxUsuarios { get; set; }
        public DateTime? UltimaVerificacion { get; set; }
    }
}
