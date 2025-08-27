using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
 public class Modulo
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime? CreadoEn { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
    }

}