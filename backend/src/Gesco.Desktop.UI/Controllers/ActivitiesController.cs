using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Shared.DTOs;
using Gesco.Desktop.Core.Interfaces;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<ActivitiesController> _logger;

        public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
        {
            _activityService = activityService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las actividades
        /// </summary>
        /// <param name="organizationId">ID de organización (opcional)</param>
        /// <returns>Lista de actividades</returns>
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
        /// <param name="id">ID de la actividad</param>
        /// <returns>Datos de la actividad</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivity(Guid id)
        {
            try
            {
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
        /// <param name="request">Datos de la actividad</param>
        /// <returns>Actividad creada</returns>
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
                _logger.LogInformation("Creating activity: {Name}", request.Name);

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "El nombre de la actividad es requerido",
                        Errors = new List<string> { "Name is required" }
                    });
                }

                // Validar fecha
                if (request.StartDate == default)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "La fecha de inicio es requerida",
                        Errors = new List<string> { "StartDate is required" }
                    });
                }

                // Convertir string a DateOnly si es necesario
                if (!DateOnly.TryParse(request.StartDate.ToString(), out var startDate))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Formato de fecha inválido",
                        Errors = new List<string> { "Invalid date format" }
                    });
                }

                // Obtener el primer usuario como manager por defecto si no se especifica
                if (!request.ManagerUserId.HasValue || request.ManagerUserId == Guid.Empty)
                {
                    // Obtener el ID del usuario actual del token si está disponible
                    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(currentUserId) && Guid.TryParse(currentUserId, out var userId))
                    {
                        request.ManagerUserId = userId;
                    }
                }

                // Obtener la primera organización como default si no se especifica
                if (!request.OrganizationId.HasValue || request.OrganizationId == Guid.Empty)
                {
                    // Por ahora usar la primera organización disponible
                    // En un escenario real, esto vendría del contexto del usuario
                    request.OrganizationId = null; // El servicio manejará esto
                }

                _logger.LogInformation("Validated request: {@Request}", new { 
                    request.Name, 
                    request.StartDate, 
                    request.ActivityStatusId,
                    request.ManagerUserId,
                    request.OrganizationId 
                });

                var activity = await _activityService.CreateActivityAsync(request);
                
                _logger.LogInformation("Activity created successfully: {ActivityId}", activity.Id);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity: {@Request}", request);
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
        /// <param name="id">ID de la actividad</param>
        /// <param name="request">Datos actualizados</param>
        /// <returns>Actividad actualizada</returns>
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
                _logger.LogInformation("Updating activity {ActivityId}: {Name}", id, request.Name);

                // Validaciones básicas (similar al create)
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

                _logger.LogInformation("Activity updated successfully: {ActivityId}", id);

                return Ok(new ApiResponse<ActivityDto>
                {
                    Success = true,
                    Data = activity,
                    Message = "Actividad actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity {ActivityId}", id);
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
        /// <param name="id">ID de la actividad</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            try
            {
                var deleted = await _activityService.DeleteActivityAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Actividad no encontrada"
                    });
                }

                _logger.LogInformation("Activity deleted successfully: {ActivityId}", id);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Actividad eliminada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity {ActivityId}", id);
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
        /// <returns>Lista de actividades activas</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActiveActivities()
        {
            try
            {
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
        /// <returns>Estadísticas generales</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivityStats()
        {
            try
            {
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