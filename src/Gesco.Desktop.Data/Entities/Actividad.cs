using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class Actividad
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }
        
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int EstadoId { get; set; }
        public int OrganizacionId { get; set; }
        public decimal? PrecioEntrada { get; set; }
        public int? CapacidadMaxima { get; set; }
        
        // Sincronización
        public string Uuid { get; set; }
        public int VersionSincronizacion { get; set; }
        public DateTime? UltimaSincronizacion { get; set; }
        public bool PendienteSincronizar { get; set; }
        
        // Navegación
        public virtual Organizacion Organizacion { get; set; }
        public virtual ICollection<ArticuloActividad> Articulos { get; set; }
        public virtual ICollection<Caja> Cajas { get; set; }
    }
}
