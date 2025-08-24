using System.Diagnostics;

namespace Gesco.Desktop.API.Middleware
{
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
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N");
            
            context.Items["RequestId"] = requestId;
            
            _logger.LogInformation(
                "Request {RequestId} starting {Method} {Path} from {IP}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "Request {RequestId} completed with {StatusCode} in {ElapsedMs}ms",
                    requestId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
