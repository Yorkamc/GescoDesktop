using System.Collections.Concurrent;

namespace Gesco.Desktop.API.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();
        private readonly TimeSpan _minTimeBetweenRequests = TimeSpan.FromSeconds(1);
        private readonly int _maxRequestsPerMinute = 60;
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requestHistory = new();

        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            
            // Check rate limit
            if (!IsRequestAllowed(clientId))
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Demasiadas solicitudes. Por favor espere un momento.");
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get user ID from claims
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
                return $"user_{userId}";

            // Fall back to IP address
            return $"ip_{context.Connection.RemoteIpAddress}";
        }

        private bool IsRequestAllowed(string clientId)
        {
            var now = DateTime.UtcNow;
            
            // Get or create request history for this client
            var history = _requestHistory.GetOrAdd(clientId, new List<DateTime>());
            
            lock (history)
            {
                // Remove requests older than 1 minute
                history.RemoveAll(dt => now - dt > TimeSpan.FromMinutes(1));
                
                // Check if limit exceeded
                if (history.Count >= _maxRequestsPerMinute)
                {
                    return false;
                }
                
                // Add current request
                history.Add(now);
                return true;
            }
        }
    }
}
