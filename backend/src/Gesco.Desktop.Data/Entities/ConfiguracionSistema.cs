using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gesco.Desktop.Data.Entities
{
   public class ConfiguracionSistema
    {
        public int Id { get; set; }
        public int? OrganizacionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Clave { get; set; }

        [Required]
        public string Valor { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoValor { get; set; }

        [MaxLength(100)]
        public string Categoria { get; set; }

        public string Descripcion { get; set; }
        public string ValorPorDefecto { get; set; }
        public bool EsEditable { get; set; } = true;
        public bool RequiereReinicio { get; set; } = false;

        [MaxLength(50)]
        public string NivelAcceso { get; set; } = "admin";

        public DateTime? ActualizadoEn { get; set; }
        public int? ActualizadoPor { get; set; }

        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual Usuario ActualizadoPorUsuario { get; set; }
    }
}