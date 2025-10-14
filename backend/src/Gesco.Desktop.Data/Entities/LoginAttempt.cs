using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Gesco.Desktop.Data.Entities.Base;

namespace Gesco.Desktop.Data.Entities
{
  /// <summary>
    /// Registro de intentos de inicio de sesión
    /// Permite: detectar ataques de fuerza bruta, bloquear cuentas temporalmente
    /// </summary>
    [Table("login_attempts")]
    public class LoginAttempt : BaseEntityLong
    {
        [Column("attempted_email")]
        [Required]
        [MaxLength(255)]
        public string AttemptedEmail { get; set; } = string.Empty;

        [Column("result")]
        [Required]
        [MaxLength(20)]
        public string Result { get; set; } = string.Empty; // successful, failed, blocked

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("attempt_date")]
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        // Métodos de utilidad
        public bool IsSuccessful() => Result == "successful";
        public bool IsFailed() => Result == "failed";
        public bool IsBlocked() => Result == "blocked";

        // Constantes para los resultados
        public const string RESULT_SUCCESSFUL = "successful";
        public const string RESULT_FAILED = "failed";
        public const string RESULT_BLOCKED = "blocked";
    }

    /// <summary>
    /// DTO para estadísticas de intentos de login
    /// </summary>
    public class LoginAttemptStats
    {
        public string Email { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public int SuccessfulAttempts { get; set; }
        public int BlockedAttempts { get; set; }
        public DateTime? LastAttempt { get; set; }
        public string? LastIpAddress { get; set; }
        public bool ShouldBlock { get; set; }
    }
}