using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gesco.Desktop.Core.Audit
{
    public enum AuditEventType
    {
        Login,
        Logout,
        LicenseActivation,
        DataAccess,
        ConfigurationChange,
        SecurityViolation,
        DatabaseOperation
    }

    public class AuditEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public AuditEventType EventType { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string AdditionalData { get; set; }
        public string UserAgent { get; set; }
    }

    public interface IAuditService
    {
        Task LogEventAsync(AuditEvent auditEvent);
        Task LogLoginAttemptAsync(string username, bool success, string ipAddress, string errorMessage = null);
        Task LogLicenseActivationAsync(string activationCode, bool success, string userId);
        Task LogDataAccessAsync(string resource, string userId, string action);
        Task LogSecurityViolationAsync(string violation, string userId, string details);
    }

    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly string _auditFilePath;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
            _auditFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "audit", $"audit-{DateTime.Now:yyyy-MM}.log");
            
            // Crear directorio si no existe
            var directory = Path.GetDirectoryName(_auditFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task LogEventAsync(AuditEvent auditEvent)
        {
            try
            {
                var auditJson = JsonConvert.SerializeObject(auditEvent);
                _logger.LogInformation("AUDIT: {AuditEvent}", auditJson);
                
                // Escribir también a archivo dedicado de auditoría
                await File.AppendAllTextAsync(_auditFilePath, $"{auditJson}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing audit event");
            }
        }

        public async Task LogLoginAttemptAsync(string username, bool success, string ipAddress, string errorMessage = null)
        {
            await LogEventAsync(new AuditEvent
            {
                EventType = AuditEventType.Login,
                UserName = username,
                IpAddress = ipAddress,
                Action = "Login Attempt",
                Success = success,
                ErrorMessage = errorMessage,
                AdditionalData = JsonConvert.SerializeObject(new { LoginMethod = "Local" })
            });
        }

        public async Task LogLicenseActivationAsync(string activationCode, bool success, string userId)
        {
            await LogEventAsync(new AuditEvent
            {
                EventType = AuditEventType.LicenseActivation,
                UserId = userId,
                Action = "License Activation",
                Resource = activationCode?.Substring(0, Math.Min(5, activationCode.Length)) + "***",
                Success = success
            });
        }

        public async Task LogDataAccessAsync(string resource, string userId, string action)
        {
            await LogEventAsync(new AuditEvent
            {
                EventType = AuditEventType.DataAccess,
                UserId = userId,
                Action = action,
                Resource = resource,
                Success = true
            });
        }

        public async Task LogSecurityViolationAsync(string violation, string userId, string details)
        {
            await LogEventAsync(new AuditEvent
            {
                EventType = AuditEventType.SecurityViolation,
                UserId = userId,
                Action = violation,
                Success = false,
                ErrorMessage = "Security violation detected",
                AdditionalData = details
            });
        }
    }
}
