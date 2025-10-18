using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Data.Context;
using System.Security.Claims;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<ActivitiesController> _logger;
        private readonly LocalDbContext _context;

        public ActivitiesController(
            IActivityService activityService, 
            ILogger<ActivitiesController> logger,
            LocalDbContext context)
        {
            _activityService = activityService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Obtener todas las actividades
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivities([FromQuery] Guid? organizationId = null)
        {
            try
            {
                _logger.LogInformation("Getting activities for organization: {OrganizationId}", organizationId);
                
                var activities = await _activityService.GetActivitiesAsync(organizationId);
                
                return Ok(new ApiResponse<List<ActivityDto>>
                {
                    Success = true,
                    Data = activities,
                    Message = $"Se encontraron {activities.Count} actividades"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener actividades",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener actividad por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivity(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting activity: {ActivityId}", id);
                
                var activity = await _activityService.GetActivityByIdAsync(id);
                
                if (activity == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Actividad no encontrada"
                    });
                }

                return Ok(new ApiResponse<ActivityDto>
                {
                    Success = true,
                    Data = activity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity {ActivityId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nueva actividad
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ActivityDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request)
        {
            try
            {
                _logger.LogInformation("=== CREATE ACTIVITY REQUEST ===");
                _logger.LogInformation("User authenticated: {IsAuth}", User.Identity?.IsAuthenticated);
                _logger.LogInformation("Request: {@Request}", new { 
                    request.Name, 
                    request.StartDate,
                    request.ActivityStatusId,
                    request.ManagerUserId,
                    request.OrganizationId
                });

                // Log user claims
                _logger.LogInformation("User claims:");
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("  - {Type}: {Value}", claim.Type, claim.Value);
                }

                // VALIDAR REQUEST
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("Validation failed: Name is required");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "El nombre de la actividad es requerido",
                        Errors = new List<string> { "Name is required" }
                    });
                }

                if (request.StartDate == default)
                {
                    _logger.LogWarning("Validation failed: StartDate is required");
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "La fecha de inicio es requerida",
                        Errors = new List<string> { "StartDate is required" }
                    });
                }

                // ASIGNAR MANAGER USER AUTOMÁTICAMENTE SI NO SE PROPORCIONA
                if (string.IsNullOrEmpty(request.ManagerUserId))
                {
                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    _logger.LogInformation("No ManagerUserId provided, using current user: {UserId}", currentUserId);
                    
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        // Verificar que el usuario existe y está activo
                        var userExists = await _context.Users
                            .AsNoTracking()
                            .AnyAsync(u => u.Id == currentUserId && u.Active);
                        
                        if (userExists)
                        {
                            request.ManagerUserId = currentUserId;
                            _logger.LogInformation("Current user validated and assigned as manager: {UserId}", currentUserId);
                        }
                        else
                        {
                            _logger.LogWarning("Current user not found or inactive: {UserId}", currentUserId);
                        }
                    }
                }

                // MANEJAR ORGANIZATION ID
                if (!request.OrganizationId.HasValue || request.OrganizationId == Guid.Empty)
                {
                    _logger.LogInformation("No OrganizationId provided, setting to null");
                    request.OrganizationId = null;
                }
                else
                {
                    _logger.LogInformation("OrganizationId provided: {OrgId}", request.OrganizationId);
                }

                _logger.LogInformation("Validated request: {@ValidatedRequest}", new { 
                    request.Name, 
                    request.StartDate, 
                    request.ActivityStatusId,
                    request.ManagerUserId,
                    request.OrganizationId 
                });

                // CREAR ACTIVIDAD
                var activity = await _activityService.CreateActivityAsync(request);
                
                _logger.LogInformation("=== ACTIVITY CREATED SUCCESSFULLY ===");
                _logger.LogInformation("Activity: {ActivityId} - {Name}", activity.Id, activity.Name);

                return CreatedAtAction(
                    nameof(GetActivity),
                    new { id = activity.Id },
                    new ApiResponse<ActivityDto>
                    {
                        Success = true,
                        Data = activity,
                        Message = "Actividad creada exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating activity");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { "Validation error" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ERROR CREATING ACTIVITY ===");
                _logger.LogError("Request: {@Request}", request);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar actividad existente
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ActivityDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] CreateActivityRequest request)
        {
            try
            {
                _logger.LogInformation("=== UPDATE ACTIVITY REQUEST ===");
                _logger.LogInformation("ActivityId: {ActivityId}", id);
                _logger.LogInformation("User authenticated: {IsAuth}", User.Identity?.IsAuthenticated);
                _logger.LogInformation("Request: {@Request}", new { 
                    request.Name, 
                    request.StartDate,
                    request.ActivityStatusId,
                    request.ManagerUserId,
                    request.OrganizationId
                });

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "El nombre de la actividad es requerido",
                        Errors = new List<string> { "Name is required" }
                    });
                }

                if (request.StartDate == default)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "La fecha de inicio es requerida",
                        Errors = new List<string> { "StartDate is required" }
                    });
                }

                var activity = await _activityService.UpdateActivityAsync(id, request);
                
                if (activity == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Actividad no encontrada"
                    });
                }

                _logger.LogInformation("=== ACTIVITY UPDATED SUCCESSFULLY ===");
                _logger.LogInformation("Activity: {ActivityId} - {Name}", id, activity.Name);

                return Ok(new ApiResponse<ActivityDto>
                {
                    Success = true,
                    Data = activity,
                    Message = "Actividad actualizada exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating activity {ActivityId}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { "Validation error" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ERROR UPDATING ACTIVITY ===");
                _logger.LogError("ActivityId: {ActivityId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Eliminar actividad
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            try
            {
                _logger.LogInformation("=== DELETE ACTIVITY REQUEST ===");
                _logger.LogInformation("ActivityId: {ActivityId}", id);
                _logger.LogInformation("User authenticated: {IsAuth}", User.Identity?.IsAuthenticated);

                var deleted = await _activityService.DeleteActivityAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Actividad no encontrada"
                    });
                }

                _logger.LogInformation("=== ACTIVITY DELETED SUCCESSFULLY ===");
                _logger.LogInformation("ActivityId: {ActivityId}", id);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Actividad eliminada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete activity {ActivityId}: has related records", id);
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { "Conflict" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== ERROR DELETING ACTIVITY ===");
                _logger.LogError("ActivityId: {ActivityId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al eliminar actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener actividades activas
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActiveActivities()
        {
            try
            {
                _logger.LogInformation("Getting active activities");
                
                var activities = await _activityService.GetActiveActivitiesAsync();
                
                return Ok(new ApiResponse<List<ActivityDto>>
                {
                    Success = true,
                    Data = activities,
                    Message = $"Se encontraron {activities.Count} actividades activas"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active activities");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener actividades activas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener estadísticas de actividades
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivityStats()
        {
            try
            {
                _logger.LogInformation("Getting activity stats");
                
                var stats = await _activityService.GetActivityStatsAsync();
                
                return Ok(new ApiResponse<DashboardStatsDto>
                {
                    Success = true,
                    Data = stats,
                    Message = "Estadísticas obtenidas exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity stats");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener estadísticas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}