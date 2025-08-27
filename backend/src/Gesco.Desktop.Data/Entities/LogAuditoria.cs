using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
   public class LogAuditoria
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Tabla { get; set; }

        public int? RegistroId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Accion { get; set; }

        public string DatosAnteriores { get; set; }
        public string DatosNuevos { get; set; }
        public int? OrganizacionId { get; set; }

        [MaxLength(100)]
        public string Modulo { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        // Navegación
        public virtual Usuario Usuario { get; set; }
        public virtual Organizacion Organizacion { get; set; }
    }
}