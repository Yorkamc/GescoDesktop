using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Gesco.Desktop.Core.Audit;

namespace Gesco.Desktop.Core.Middleware
{
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
            // Agregar headers de seguridad
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
            
            // Remover header del servidor
            context.Response.Headers.Remove("Server");
            
            await _next(context);
        }
    }

    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IAuditService _auditService;
        private static readonly Dictionary<string, List<DateTime>> _requests = new();
        private readonly int _maxRequestsPerMinute = 60;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IAuditService auditService)
        {
            _next = next;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIP = GetClientIPAddress(context);
            var now = DateTime.UtcNow;
            
            if (_requests.ContainsKey(clientIP))
            {
                // Limpiar requests antiguos (más de 1 minuto)
                _requests[clientIP] = _requests[clientIP]
                    .Where(time => now.Subtract(time).TotalMinutes < 1)
                    .ToList();
                
                // Verificar rate limit
                if (_requests[clientIP].Count >= _maxRequestsPerMinute)
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {IP}", clientIP);
                    
                    await _auditService.LogSecurityViolationAsync(
                        "Rate Limit Exceeded",
                        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        $"IP: {clientIP}, Requests: {_requests[clientIP].Count}"
                    );
                    
                    context.Response.StatusCode = 429; // Too Many Requests
                    await context.Response.WriteAsync("Rate limit exceeded");
                    return;
                }
            }
            else
            {
                _requests[clientIP] = new List<DateTime>();
            }

            _requests[clientIP].Add(now);
            await _next(context);
        }

        private string GetClientIPAddress(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                   ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                   ?? context.Connection.RemoteIpAddress?.ToString()
                   ?? "unknown";
        }
    }
}
