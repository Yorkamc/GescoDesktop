using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(LocalDbContext context, ILogger<PaymentMethodsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los métodos de pago
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PaymentMethodDto>>), 200)]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var methods = await _context.PaymentMethods
                    .Where(pm => pm.Active)
                    .OrderBy(pm => pm.Name)
                    .Select(pm => new PaymentMethodDto
                    {
                        Id = (int)pm.Id,
                        Name = pm.Name,
                        RequiresReference = pm.RequiresReference,
                        Active = pm.Active
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<PaymentMethodDto>>
                {
                    Success = true,
                    Data = methods,
                    Message = $"Se encontraron {methods.Count} métodos de pago"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener métodos de pago",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}