using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Core.Audit;

namespace Gesco.Desktop.UI.Middleware
{
    /// <summary>
    /// Middleware para agregar headers de seguridad HTTP
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Headers de seguridad estándar
                AddSecurityHeaders(context);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SecurityHeadersMiddleware");
                throw;
            }
        }

        private static void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // Prevenir MIME type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Prevenir clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Activar filtro XSS del navegador
            headers["X-XSS-Protection"] = "1; mode=block";

            // Control de referrer
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Content Security Policy básica
            headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self' http://localhost:5100";

            // Strict Transport Security (solo para HTTPS)
            if (context.Request.IsHttps)
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            // Remover headers que revelan información del servidor
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
        }
    }

    /// <summary>
    /// Middleware para implementar rate limiting por IP
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IAuditService? _auditService;
        
        // Configuración de rate limiting
        private readonly int _maxRequestsPerMinute;
        private readonly int _maxRequestsPerHour;
        
        // Almacenamiento thread-safe de requests por IP
        private static readonly Dictionary<string, List<DateTime>> _requestsPerMinute = new();
        private static readonly Dictionary<string, List<DateTime>> _requestsPerHour = new();
        private static readonly object _lockObject = new object();

        public RateLimitingMiddleware(
            RequestDelegate next, 
            ILogger<RateLimitingMiddleware> logger, 
            IAuditService? auditService = null)
        {
            _next = next;
            _logger = logger;
            _auditService = auditService;
            
            // Configuración por defecto
            _maxRequestsPerMinute = 60;  // 60 requests por minuto
            _maxRequestsPerHour = 1000;  // 1000 requests por hora
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIP = GetClientIPAddress(context);
            var now = DateTime.UtcNow;

            // Verificar rate limits
            if (IsRateLimitExceeded(clientIP, now))
            {
                await HandleRateLimitExceeded(context, clientIP);
                return;
            }

            // Registrar request
            RecordRequest(clientIP, now);

            await _next(context);
        }

        private bool IsRateLimitExceeded(string clientIP, DateTime now)
        {
            lock (_lockObject)
            {
                // Limpiar requests antiguos
                CleanOldRequests(clientIP, now);

                // Verificar límites
                var requestsThisMinute = _requestsPerMinute.GetValueOrDefault(clientIP, new List<DateTime>()).Count;
                var requestsThisHour = _requestsPerHour.GetValueOrDefault(clientIP, new List<DateTime>()).Count;

                return requestsThisMinute >= _maxRequestsPerMinute || requestsThisHour >= _maxRequestsPerHour;
            }
        }

        private void RecordRequest(string clientIP, DateTime now)
        {
            lock (_lockObject)
            {
                // Registrar para límite por minuto
                if (!_requestsPerMinute.ContainsKey(clientIP))
                    _requestsPerMinute[clientIP] = new List<DateTime>();
                _requestsPerMinute[clientIP].Add(now);

                // Registrar para límite por hora
                if (!_requestsPerHour.ContainsKey(clientIP))
                    _requestsPerHour[clientIP] = new List<DateTime>();
                _requestsPerHour[clientIP].Add(now);
            }
        }

        private void CleanOldRequests(string clientIP, DateTime now)
        {
            // Limpiar requests de más de 1 minuto
            if (_requestsPerMinute.ContainsKey(clientIP))
            {
                _requestsPerMinute[clientIP] = _requestsPerMinute[clientIP]
                    .Where(time => now.Subtract(time).TotalMinutes < 1)
                    .ToList();
                    
                if (_requestsPerMinute[clientIP].Count == 0)
                    _requestsPerMinute.Remove(clientIP);
            }

            // Limpiar requests de más de 1 hora
            if (_requestsPerHour.ContainsKey(clientIP))
            {
                _requestsPerHour[clientIP] = _requestsPerHour[clientIP]
                    .Where(time => now.Subtract(time).TotalHours < 1)
                    .ToList();
                    
                if (_requestsPerHour[clientIP].Count == 0)
                    _requestsPerHour.Remove(clientIP);
            }
        }

        private async Task HandleRateLimitExceeded(HttpContext context, string clientIP)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {ClientIP}", clientIP);

            // Log de auditoría de seguridad
            if (_auditService != null)
            {
                try
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                    var requestsPerMinute = _requestsPerMinute.GetValueOrDefault(clientIP, new List<DateTime>()).Count;
                    var requestsPerHour = _requestsPerHour.GetValueOrDefault(clientIP, new List<DateTime>()).Count;

                    _ = Task.Run(async () => await _auditService.LogSecurityViolationAsync(
                        "Rate Limit Exceeded",
                        userId,
                        $"IP: {clientIP}, Requests/min: {requestsPerMinute}, Requests/hour: {requestsPerHour}, Path: {context.Request.Path}"
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error logging security violation for rate limit");
                }
            }

            // Respuesta HTTP 429 Too Many Requests
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Rate limit exceeded",
                message = "Too many requests. Please try again later.",
                retryAfter = "60 seconds"
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }

        private static string GetClientIPAddress(HttpContext context)
        {
            // Intentar obtener la IP real desde headers de proxy
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For puede contener múltiples IPs separadas por coma
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIP))
            {
                return realIP;
            }

            // Fallback a la IP de conexión directa
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    /// <summary>
    /// Middleware para logging detallado de requests
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var clientIP = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            try
            {
                // Log del request entrante
                _logger.LogInformation(
                    "Request started: {Method} {Path} from {ClientIP}",
                    context.Request.Method,
                    context.Request.Path,
                    clientIP);

                await _next(context);

                // Log del request completado
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "Request completed: {Method} {Path} {StatusCode} in {Duration}ms from {ClientIP}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration.TotalMilliseconds,
                    clientIP);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex,
                    "Request failed: {Method} {Path} in {Duration}ms from {ClientIP}",
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds,
                    clientIP);
                throw;
            }
        }
    }
}