using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class SalesCombosController : ControllerBase
    {
        private readonly ISalesComboService _comboService;
        private readonly ILogger<SalesCombosController> _logger;

        public SalesCombosController(ISalesComboService comboService, ILogger<SalesCombosController> logger)
        {
            _comboService = comboService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los combos, opcionalmente filtrados por actividad
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SalesComboDto>>), 200)]
        public async Task<IActionResult> GetCombos([FromQuery] Guid? activityId = null)
        {
            try
            {
                var combos = await _comboService.GetCombosAsync(activityId);
                
                return Ok(new ApiResponse<List<SalesComboDto>>
                {
                    Success = true,
                    Data = combos,
                    Message = $"Se encontraron {combos.Count} combos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving combos");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener combos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener combo por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SalesComboDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCombo(Guid id)
        {
            try
            {
                var combo = await _comboService.GetComboByIdAsync(id);
                
                if (combo == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Combo no encontrado"
                    });
                }

                return Ok(new ApiResponse<SalesComboDto>
                {
                    Success = true,
                    Data = combo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving combo {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener combo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nuevo combo
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SalesComboDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCombo([FromBody] CreateSalesComboRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos del combo inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var combo = await _comboService.CreateComboAsync(request);
                
                return CreatedAtAction(
                    nameof(GetCombo),
                    new { id = combo.Id },
                    new ApiResponse<SalesComboDto>
                    {
                        Success = true,
                        Data = combo,
                        Message = "Combo creado exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating combo");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating combo");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear combo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar combo existente
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SalesComboDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCombo(Guid id, [FromBody] CreateSalesComboRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos del combo inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var combo = await _comboService.UpdateComboAsync(id, request);
                
                if (combo == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Combo no encontrado"
                    });
                }

                return Ok(new ApiResponse<SalesComboDto>
                {
                    Success = true,
                    Data = combo,
                    Message = "Combo actualizado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating combo {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating combo {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar combo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Eliminar combo
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> DeleteCombo(Guid id)
        {
            try
            {
                var deleted = await _comboService.DeleteComboAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Combo no encontrado"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Combo eliminado exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete combo {Id}", id);
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting combo {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al eliminar combo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Activar/Desactivar combo
        /// </summary>
        [HttpPost("{id:guid}/toggle-active")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleComboActive(Guid id)
        {
            try
            {
                var toggled = await _comboService.ToggleComboActiveAsync(id);
                
                if (!toggled)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Combo no encontrado"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Estado del combo actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling combo active status {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al cambiar estado del combo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener combos activos por actividad
        /// </summary>
        [HttpGet("active/{activityId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<SalesComboDto>>), 200)]
        public async Task<IActionResult> GetActiveCombos(Guid activityId)
        {
            try
            {
                var combos = await _comboService.GetActiveCombosByActivityAsync(activityId);
                
                return Ok(new ApiResponse<List<SalesComboDto>>
                {
                    Success = true,
                    Data = combos,
                    Message = $"Se encontraron {combos.Count} combos activos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active combos for activity {ActivityId}", activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener combos activos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}