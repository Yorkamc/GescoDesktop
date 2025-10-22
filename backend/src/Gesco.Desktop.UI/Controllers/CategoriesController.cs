using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // ============================================
        // SERVICE CATEGORIES (Catálogos globales)
        // ============================================

        /// <summary>
        /// Obtener todas las categorías de servicio
        /// </summary>
        [HttpGet("service")]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceCategoryDto>>), 200)]
        public async Task<IActionResult> GetServiceCategories([FromQuery] Guid? organizationId = null)
        {
            try
            {
                var categories = await _categoryService.GetServiceCategoriesAsync(organizationId);
                
                return Ok(new ApiResponse<List<ServiceCategoryDto>>
                {
                    Success = true,
                    Data = categories,
                    Message = $"Se encontraron {categories.Count} categorías de servicio"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service categories");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener categorías de servicio",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener categoría de servicio por ID
        /// </summary>
        [HttpGet("service/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetServiceCategory(Guid id)
        {
            try
            {
                var category = await _categoryService.GetServiceCategoryByIdAsync(id);
                
                if (category == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Categoría de servicio no encontrada"
                    });
                }

                return Ok(new ApiResponse<ServiceCategoryDto>
                {
                    Success = true,
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service category {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener categoría de servicio",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nueva categoría de servicio
        /// </summary>
        [HttpPost("service")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateServiceCategory([FromBody] CreateServiceCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de categoría inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var category = await _categoryService.CreateServiceCategoryAsync(request);
                
                return CreatedAtAction(
                    nameof(GetServiceCategory),
                    new { id = category.Id },
                    new ApiResponse<ServiceCategoryDto>
                    {
                        Success = true,
                        Data = category,
                        Message = "Categoría de servicio creada exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating service category");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service category");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear categoría de servicio",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar categoría de servicio
        /// </summary>
        [HttpPut("service/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ServiceCategoryDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateServiceCategory(Guid id, [FromBody] CreateServiceCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de categoría inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var category = await _categoryService.UpdateServiceCategoryAsync(id, request);
                
                if (category == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Categoría de servicio no encontrada"
                    });
                }

                return Ok(new ApiResponse<ServiceCategoryDto>
                {
                    Success = true,
                    Data = category,
                    Message = "Categoría de servicio actualizada exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating service category {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service category {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar categoría de servicio",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Eliminar categoría de servicio (soft delete)
        /// </summary>
        [HttpDelete("service/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteServiceCategory(Guid id)
        {
            try
            {
                var deleted = await _categoryService.DeleteServiceCategoryAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Categoría de servicio no encontrada"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Categoría de servicio eliminada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete service category {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service category {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al eliminar categoría de servicio",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ============================================
        // ACTIVITY CATEGORIES (Relación Actividad-Categoría)
        // ============================================

        /// <summary>
        /// Obtener todas las categorías de actividad
        /// </summary>
        [HttpGet("activity")]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityCategoryDto>>), 200)]
        public async Task<IActionResult> GetActivityCategories([FromQuery] Guid? activityId = null)
        {
            try
            {
                var categories = await _categoryService.GetActivityCategoriesAsync(activityId);
                
                return Ok(new ApiResponse<List<ActivityCategoryDto>>
                {
                    Success = true,
                    Data = categories,
                    Message = $"Se encontraron {categories.Count} categorías de actividad"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity categories");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener categorías de actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener categoría de actividad por ID
        /// </summary>
        [HttpGet("activity/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ActivityCategoryDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetActivityCategory(Guid id)
        {
            try
            {
                var category = await _categoryService.GetActivityCategoryByIdAsync(id);
                
                if (category == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Categoría de actividad no encontrada"
                    });
                }

                return Ok(new ApiResponse<ActivityCategoryDto>
                {
                    Success = true,
                    Data = category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity category {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener categoría de actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nueva categoría de actividad (asociar categoría a actividad)
        /// </summary>
        [HttpPost("activity")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ActivityCategoryDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateActivityCategory([FromBody] CreateActivityCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de categoría inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var category = await _categoryService.CreateActivityCategoryAsync(request);
                
                return CreatedAtAction(
                    nameof(GetActivityCategory),
                    new { id = category.Id },
                    new ApiResponse<ActivityCategoryDto>
                    {
                        Success = true,
                        Data = category,
                        Message = "Categoría asociada a la actividad exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating activity category");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity category");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al asociar categoría a actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Eliminar categoría de actividad (desasociar categoría de actividad)
        /// </summary>
        [HttpDelete("activity/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteActivityCategory(Guid id)
        {
            try
            {
                var deleted = await _categoryService.DeleteActivityCategoryAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Categoría de actividad no encontrada"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Categoría desasociada de la actividad exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete activity category {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity category {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al desasociar categoría de actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}