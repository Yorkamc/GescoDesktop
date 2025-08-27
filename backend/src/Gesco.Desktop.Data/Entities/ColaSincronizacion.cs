using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
   public class ColaSincronizacion
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TablaAfectada { get; set; }

        [Required]
        public int RegistroId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Operacion { get; set; }

        [Required]
        public string DatosCambio { get; set; }

        public int Prioridad { get; set; } = 1;
        public bool Procesado { get; set; } = false;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaProcesado { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
    }
}