using Microsoft.AspNetCore.Authorization;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.API.Middleware
{
    public class LicenseValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LicenseValidationMiddleware> _logger;

        public LicenseValidationMiddleware(RequestDelegate next, ILogger<LicenseValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IActivationService activationService)
        {
            // Skip validation for certain endpoints
            var skipPaths = new[] { "/api/license/activate", "/api/auth/login", "/swagger", "/health" };
            
            if (skipPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            // Check if endpoint requires authorization
            var endpoint = context.GetEndpoint();
            var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();
            
            if (authorizeAttribute != null)
            {
                // Validate license
                var hasValidLicense = await activationService.CheckLicenseStatusAsync();
                
                if (!hasValidLicense)
                {
                    _logger.LogWarning("Request blocked due to invalid license");
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Licencia no válida o expirada");
                    return;
                }
            }

            await _next(context);
        }
    }
}
