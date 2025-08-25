using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class Organizacion
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        [MaxLength(200)]
        public string CorreoContacto { get; set; }

        [MaxLength(50)]
        public string TelefonoContacto { get; set; }

        public string Direccion { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.Now;

        // Navegación
        public virtual ICollection<Usuario> Usuarios { get; set; }
        public virtual ICollection<Actividad> Actividades { get; set; }
    }
}
