using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Core.Services;
using Gesco.Desktop.Shared.DTOs;
using System.Security.Claims;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CashRegistersController : ControllerBase
    {
        private readonly ICashRegisterService _cashRegisterService;
        private readonly ILogger<CashRegistersController> _logger;

        public CashRegistersController(ICashRegisterService cashRegisterService, ILogger<CashRegistersController> logger)
        {
            _cashRegisterService = cashRegisterService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las cajas registradoras
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CashRegisterDto>>), 200)]
        public async Task<IActionResult> GetCashRegisters([FromQuery] Guid? activityId = null)
        {
            try
            {
                var registers = await _cashRegisterService.GetCashRegistersAsync(activityId);
                
                return Ok(new ApiResponse<List<CashRegisterDto>>
                {
                    Success = true,
                    Data = registers,
                    Message = $"Se encontraron {registers.Count} cajas registradoras"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cash registers");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener cajas registradoras",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener caja registradora por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CashRegisterDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCashRegister(Guid id)
        {
            try
            {
                var register = await _cashRegisterService.GetCashRegisterByIdAsync(id);
                
                if (register == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Caja registradora no encontrada"
                    });
                }

                return Ok(new ApiResponse<CashRegisterDto>
                {
                    Success = true,
                    Data = register
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cash register {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener caja registradora",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nueva caja registradora
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CashRegisterDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCashRegister([FromBody] CreateCashRegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de caja registradora inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var register = await _cashRegisterService.CreateCashRegisterAsync(request);
                
                return CreatedAtAction(
                    nameof(GetCashRegister),
                    new { id = register.Id },
                    new ApiResponse<CashRegisterDto>
                    {
                        Success = true,
                        Data = register,
                        Message = "Caja registradora creada exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating cash register");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cash register");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear caja registradora",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar caja registradora existente
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CashRegisterDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCashRegister(Guid id, [FromBody] CreateCashRegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de caja registradora inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var register = await _cashRegisterService.UpdateCashRegisterAsync(id, request);
                
                if (register == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Caja registradora no encontrada"
                    });
                }

                return Ok(new ApiResponse<CashRegisterDto>
                {
                    Success = true,
                    Data = register,
                    Message = "Caja registradora actualizada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot update cash register {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cash register {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar caja registradora",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Eliminar caja registradora
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> DeleteCashRegister(Guid id)
        {
            try
            {
                var deleted = await _cashRegisterService.DeleteCashRegisterAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Caja registradora no encontrada"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Caja registradora eliminada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete cash register {Id}", id);
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cash register {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al eliminar caja registradora",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Abrir caja registradora
        /// </summary>
        [HttpPost("{id:guid}/open")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CashRegisterDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> OpenCashRegister(Guid id)
        {
            try
            {
                var operatorUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(operatorUserId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var register = await _cashRegisterService.OpenCashRegisterAsync(id, operatorUserId);
                
                if (register == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Caja registradora no encontrada"
                    });
                }

                return Ok(new ApiResponse<CashRegisterDto>
                {
                    Success = true,
                    Data = register,
                    Message = "Caja registradora abierta exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot open cash register {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening cash register {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al abrir caja registradora",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Cerrar caja registradora
        /// </summary>
        [HttpPost("{id:guid}/close")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CashRegisterDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CloseCashRegister(Guid id, [FromBody] CloseCashRegisterRequest request)
        {
            try
            {
                var closedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(closedBy))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                request.ClosedBy = closedBy;

                var register = await _cashRegisterService.CloseCashRegisterAsync(id, request);
                
                if (register == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Caja registradora no encontrada"
                    });
                }

                return Ok(new ApiResponse<CashRegisterDto>
                {
                    Success = true,
                    Data = register,
                    Message = "Caja registradora cerrada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot close cash register {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing cash register {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al cerrar caja registradora",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener cajas registradoras abiertas
        /// </summary>
        [HttpGet("open")]
        [ProducesResponseType(typeof(ApiResponse<List<CashRegisterDto>>), 200)]
        public async Task<IActionResult> GetOpenCashRegisters()
        {
            try
            {
                var registers = await _cashRegisterService.GetOpenCashRegistersAsync();
                
                return Ok(new ApiResponse<List<CashRegisterDto>>
                {
                    Success = true,
                    Data = registers,
                    Message = $"Se encontraron {registers.Count} cajas registradoras abiertas"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open cash registers");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener cajas registradoras abiertas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener último cierre de caja
        /// </summary>
        [HttpGet("{id:guid}/last-closure")]
        [ProducesResponseType(typeof(ApiResponse<CashRegisterClosureDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLastClosure(Guid id)
        {
            try
            {
                var closure = await _cashRegisterService.GetLastClosureAsync(id);
                
                if (closure == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "No se encontró ningún cierre para esta caja"
                    });
                }

                return Ok(new ApiResponse<CashRegisterClosureDto>
                {
                    Success = true,
                    Data = closure
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving last closure for cash register {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener último cierre",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}