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
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos inválidos",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var activity = await _activityService.CreateActivityAsync(request);
                
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
                _logger.LogError(ex, "Error creating activity");
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos inválidos",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
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
        [ProducesResponseType(typeof(ApiResponse<Gesco.Desktop.Shared.DTOs.DashboardStatsDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivityStats()
        {
            try
            {
                var stats = await _activityService.GetActivityStatsAsync();
                
                return Ok(new ApiResponse<Gesco.Desktop.Shared.DTOs.DashboardStatsDto>
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