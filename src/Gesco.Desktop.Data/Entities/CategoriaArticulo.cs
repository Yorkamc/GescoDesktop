using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class CategoriaArticulo
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }
        
        public string Descripcion { get; set; }
        public bool Activo { get; set; } = true;
        
        // Navegación
        public virtual ICollection<ArticuloActividad> Articulos { get; set; }
    }
}
