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
    public class SalesStatusesController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly ILogger<SalesStatusesController> _logger;

        public SalesStatusesController(LocalDbContext context, ILogger<SalesStatusesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los estados de venta
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SalesStatusDto>>), 200)]
        public async Task<IActionResult> GetSalesStatuses()
        {
            try
            {
                var statuses = await _context.SalesStatuses
                    .Where(ss => ss.Active)
                    .OrderBy(ss => ss.Name)
                    .Select(ss => new SalesStatusDto
                    {
                        Id = (int)ss.Id,
                        Name = ss.Name,
                        Description = ss.Description,
                        Active = ss.Active
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<SalesStatusDto>>
                {
                    Success = true,
                    Data = statuses,
                    Message = $"Se encontraron {statuses.Count} estados de venta"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales statuses");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener estados de venta",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}