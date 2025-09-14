using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
  [Table("api_activity_logs")]
    public class ApiActivityLog : BaseEntityInt
    {
        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Column("method")]
        [Required]
        [MaxLength(10)]
        public string Method { get; set; } = string.Empty; // GET, POST, PUT, DELETE

        [Column("endpoint")]
        [Required]
        [MaxLength(200)]
        public string Endpoint { get; set; } = string.Empty;

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("request_data")]
        public string? RequestData { get; set; } // JSON

        [Column("response_status")]
        public int ResponseStatus { get; set; }

        [Column("response_time_ms")]
        public decimal? ResponseTimeMs { get; set; }

        [Column("organization_id")]
        public Guid? OrganizationId { get; set; }

        [Column("module")]
        [MaxLength(50)]
        public string? Module { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization? Organization { get; set; }
    }
}