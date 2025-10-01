using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next, 
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException ex)
            {
                await HandleArgumentExceptionAsync(context, ex);
            }
            catch (InvalidOperationException ex)
            {
                await HandleInvalidOperationExceptionAsync(context, ex);
            }
            catch (DbUpdateException ex)
            {
                await HandleDatabaseExceptionAsync(context, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleUnauthorizedExceptionAsync(context, ex);
            }
            catch (NotFoundException ex)
            {
                await HandleNotFoundExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleUnknownExceptionAsync(context, ex);
            }
        }

        private async Task HandleArgumentExceptionAsync(HttpContext context, ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument validation error");
            
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse
            {
                Success = false,
                Message = "Validation error",
                Errors = new List<string> { ex.Message },
                Timestamp = DateTime.UtcNow
            };
            
            await WriteResponseAsync(context, response);
        }

        private async Task HandleInvalidOperationExceptionAsync(HttpContext context, InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation error");
            
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse
            {
                Success = false,
                Message = "Invalid operation",
                Errors = new List<string> { ex.Message },
                Timestamp = DateTime.UtcNow
            };
            
            await WriteResponseAsync(context, response);
        }

        private async Task HandleDatabaseExceptionAsync(HttpContext context, DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var message = "Error al procesar la operacion en base de datos";
            var errors = new List<string>();

            if (ex.InnerException != null)
            {
                if (ex.InnerException.Message.Contains("UNIQUE constraint"))
                {
                    message = "El registro ya existe en la base de datos";
                    errors.Add("Duplicate entry error");
                }
                else if (ex.InnerException.Message.Contains("FOREIGN KEY constraint"))
                {
                    message = "No se puede completar la operacion debido a referencias existentes";
                    errors.Add("Foreign key constraint error");
                }
            }
            
            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
            
            await WriteResponseAsync(context, response);
        }

        private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse
            {
                Success = false,
                Message = "No autorizado para realizar esta operacion",
                Errors = new List<string> { "Unauthorized" },
                Timestamp = DateTime.UtcNow
            };
            
            await WriteResponseAsync(context, response);
        }

        private async Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { "Not found" },
                Timestamp = DateTime.UtcNow
            };
            
            await WriteResponseAsync(context, response);
        }

        private async Task HandleUnknownExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse
            {
                Success = false,
                Message = "Error interno del servidor",
                Errors = new List<string> { "An unexpected error occurred" },
                Timestamp = DateTime.UtcNow
            };
            
            await WriteResponseAsync(context, response);
        }

        private static async Task WriteResponseAsync(HttpContext context, ApiResponse response)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            
            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}