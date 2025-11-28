using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Shared.DTOs;
using System.Security.Claims;

namespace Gesco.Desktop.UI.Controllers
{
    /// <summary>
    /// Controlador de Ventas - ACTUALIZADO CON VALIDACIONES PARA COMBOS
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISalesTransactionService _salesService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(ISalesTransactionService salesService, ILogger<SalesController> logger)
        {
            _salesService = salesService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SalesTransactionDto>>), 200)]
        public async Task<IActionResult> GetSales(
            [FromQuery] Guid? cashRegisterId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var sales = await _salesService.GetSalesAsync(cashRegisterId, startDate, endDate);
                
                return Ok(new ApiResponse<List<SalesTransactionDto>>
                {
                    Success = true,
                    Data = sales,
                    Message = $"Se encontraron {sales.Count} ventas"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener ventas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SalesTransactionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSale(Guid id)
        {
            try
            {
                var sale = await _salesService.GetSaleByIdAsync(id);
                
                if (sale == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Venta no encontrada"
                    });
                }

                return Ok(new ApiResponse<SalesTransactionDto>
                {
                    Success = true,
                    Data = sale
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sale {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener venta",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nueva venta - ACTUALIZADO: Ahora valida productos Y combos
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SalesTransactionDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de venta inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                // ✅ NUEVA VALIDACIÓN: Verificar que cada item tenga ProductId O ComboId
                var validationErrors = new List<string>();
                
                for (int i = 0; i < request.Items.Count; i++)
                {
                    var item = request.Items[i];
                    
                    if (!item.ProductId.HasValue && !item.ComboId.HasValue)
                    {
                        validationErrors.Add($"Item {i + 1}: Debe tener ProductId o ComboId");
                    }
                    
                    if (item.ProductId.HasValue && item.ComboId.HasValue)
                    {
                        validationErrors.Add($"Item {i + 1}: No puede tener ProductId y ComboId al mismo tiempo");
                    }

                    if (item.Quantity <= 0)
                    {
                        validationErrors.Add($"Item {i + 1}: La cantidad debe ser mayor a 0");
                    }
                }

                if (validationErrors.Any())
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Errores de validación en los items",
                        Errors = validationErrors
                    });
                }

                // Obtener usuario actual
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                request.CreatedBy = currentUserId;

                var sale = await _salesService.CreateSaleAsync(request);
                
                return CreatedAtAction(
                    nameof(GetSale),
                    new { id = sale.Id },
                    new ApiResponse<SalesTransactionDto>
                    {
                        Success = true,
                        Data = sale,
                        Message = "Venta creada exitosamente (pendiente de pago)"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating sale");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation creating sale");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sale");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear venta",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SalesTransactionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateSale(Guid id, [FromBody] CreateSaleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de venta inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                // ✅ VALIDACIÓN: Igual que en CreateSale
                var validationErrors = new List<string>();
                
                for (int i = 0; i < request.Items.Count; i++)
                {
                    var item = request.Items[i];
                    
                    if (!item.ProductId.HasValue && !item.ComboId.HasValue)
                    {
                        validationErrors.Add($"Item {i + 1}: Debe tener ProductId o ComboId");
                    }
                    
                    if (item.ProductId.HasValue && item.ComboId.HasValue)
                    {
                        validationErrors.Add($"Item {i + 1}: No puede tener ProductId y ComboId al mismo tiempo");
                    }
                }

                if (validationErrors.Any())
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Errores de validación en los items",
                        Errors = validationErrors
                    });
                }

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                request.CreatedBy = currentUserId;

                var sale = await _salesService.UpdateSaleAsync(id, request);
                
                if (sale == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Venta no encontrada"
                    });
                }

                return Ok(new ApiResponse<SalesTransactionDto>
                {
                    Success = true,
                    Data = sale,
                    Message = "Venta actualizada exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot update sale {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sale {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar venta",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelSale(Guid id, [FromBody] CancelSaleRequest request)
        {
            try
            {
                var cancelled = await _salesService.CancelSaleAsync(id, request.Reason ?? "Sin razón especificada");
                
                if (!cancelled)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Venta no encontrada"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Venta cancelada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling sale {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al cancelar venta",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<SalesTransactionDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CompleteSale(Guid id, [FromBody] CompleteSaleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de pago inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                foreach (var payment in request.Payments)
                {
                    payment.ProcessedBy = currentUserId;
                }

                var sale = await _salesService.CompleteSaleAsync(id, request.Payments);
                
                if (sale == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Venta no encontrada"
                    });
                }

                return Ok(new ApiResponse<SalesTransactionDto>
                {
                    Success = true,
                    Data = sale,
                    Message = "Venta completada exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error completing sale {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot complete sale {Id}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sale {Id}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al completar venta",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("by-cash-register/{cashRegisterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<SalesTransactionDto>>), 200)]
        public async Task<IActionResult> GetSalesByCashRegister(Guid cashRegisterId)
        {
            try
            {
                var sales = await _salesService.GetSalesByCashRegisterAsync(cashRegisterId);
                
                return Ok(new ApiResponse<List<SalesTransactionDto>>
                {
                    Success = true,
                    Data = sales,
                    Message = $"Se encontraron {sales.Count} ventas para esta caja"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales for cash register {Id}", cashRegisterId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener ventas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SalesSummaryDto>), 200)]
        public async Task<IActionResult> GetSalesSummary(
            [FromQuery] Guid? cashRegisterId = null,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var summary = await _salesService.GetSalesSummaryAsync(cashRegisterId, date);
                
                return Ok(new ApiResponse<SalesSummaryDto>
                {
                    Success = true,
                    Data = summary,
                    Message = "Resumen de ventas obtenido exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales summary");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener resumen de ventas",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("quick-sale")]
        [ProducesResponseType(typeof(ApiResponse<SalesTransactionDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateQuickSale([FromBody] QuickSaleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de venta inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                // ✅ VALIDACIÓN para quick sale también
                var validationErrors = new List<string>();
                
                for (int i = 0; i < request.Items.Count; i++)
                {
                    var item = request.Items[i];
                    
                    if (!item.ProductId.HasValue && !item.ComboId.HasValue)
                    {
                        validationErrors.Add($"Item {i + 1}: Debe tener ProductId o ComboId");
                    }
                    
                    if (item.ProductId.HasValue && item.ComboId.HasValue)
                    {
                        validationErrors.Add($"Item {i + 1}: No puede tener ProductId y ComboId al mismo tiempo");
                    }
                }

                if (validationErrors.Any())
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Errores de validación en los items",
                        Errors = validationErrors
                    });
                }

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Usuario no autenticado"
                    });
                }

                var createRequest = new CreateSaleRequest
                {
                    CashRegisterId = request.CashRegisterId,
                    Items = request.Items,
                    CreatedBy = currentUserId
                };

                var sale = await _salesService.CreateSaleAsync(createRequest);

                foreach (var payment in request.Payments)
                {
                    payment.ProcessedBy = currentUserId;
                }

                var completedSale = await _salesService.CompleteSaleAsync(sale.Id, request.Payments);

                if (completedSale == null)
                {
                    throw new InvalidOperationException("Failed to complete quick sale");
                }

                return CreatedAtAction(
                    nameof(GetSale),
                    new { id = completedSale.Id },
                    new ApiResponse<SalesTransactionDto>
                    {
                        Success = true,
                        Data = completedSale,
                        Message = "Venta rápida completada exitosamente"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quick sale");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear venta rápida",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }

    // ============================================
    // DTOs LOCALES DEL CONTROLADOR
    // ============================================
    
    public class CancelSaleRequest
    {
        public string? Reason { get; set; }
    }

    public class CompleteSaleRequest
    {
        public List<CreatePaymentRequest> Payments { get; set; } = new();
    }

    public class QuickSaleRequest
    {
        public Guid CashRegisterId { get; set; }
        public List<CreateSaleItemRequest> Items { get; set; } = new();
        public List<CreatePaymentRequest> Payments { get; set; } = new();
    }
}