using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gesco.Desktop.Data.Entities
{
    public class Caja
    {
        public int Id { get; set; }
        public int ActividadId { get; set; }
        public int NumeroCaja { get; set; }
        
        [MaxLength(100)]
        public string Nombre { get; set; }
        
        public bool Abierta { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? OperadorUsuarioId { get; set; }
        
        // Navegación
        public virtual Actividad Actividad { get; set; }
        public virtual Usuario OperadorUsuario { get; set; }
        public virtual ICollection<TransaccionVenta> Transacciones { get; set; }
    }
}
