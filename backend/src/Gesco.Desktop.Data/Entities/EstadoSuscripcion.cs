using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
 public class EstadoSuscripcion
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public bool PermiteUsoSistema { get; set; } = false;
        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Suscripcion> Suscripciones { get; set; }
    }
}