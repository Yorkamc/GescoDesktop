using System;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class SesionLocal
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }


        [Required]
        public string Token { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Activa { get; set; }

        // Navegación

        public virtual Usuario Usuario { get; set; }
    }
}
