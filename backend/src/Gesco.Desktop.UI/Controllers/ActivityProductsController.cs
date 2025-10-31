using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Controllers
{
    /// <summary>
    /// Controlador para gestión de productos específicos de actividades
    /// </summary>
    [ApiController]
    [Route("api/activities/{activityId:guid}/products")]
    [Produces("application/json")]
    public class ActivityProductsController : ControllerBase
    {
        private readonly IActivityProductService _activityProductService;
        private readonly ILogger<ActivityProductsController> _logger;

        public ActivityProductsController(
            IActivityProductService activityProductService,
            ILogger<ActivityProductsController> logger)
        {
            _activityProductService = activityProductService;
            _logger = logger;
        }

        // ============================================
        // CONSULTAS DE PRODUCTOS
        // ============================================

        /// <summary>
        /// Obtener todos los productos de una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <returns>Lista de productos con información completa</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryProductDetailedDto>>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivityProducts(Guid activityId)
        {
            try
            {
                _logger.LogInformation("Getting products for activity {ActivityId}", activityId);

                var products = await _activityProductService.GetProductsByActivityAsync(activityId);

                return Ok(new ApiResponse<List<CategoryProductDetailedDto>>
                {
                    Success = true,
                    Data = products,
                    Message = $"Se encontraron {products.Count} productos para la actividad"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Activity {ActivityId} not found", activityId);
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for activity {ActivityId}", activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener productos de la actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener resumen de productos por actividad con agrupación por categoría
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <returns>Resumen detallado con estadísticas y productos agrupados</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ActivityProductsSummaryDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivityProductsSummary(Guid activityId)
        {
            try
            {
                _logger.LogInformation("Getting products summary for activity {ActivityId}", activityId);

                var summary = await _activityProductService.GetActivityProductsSummaryAsync(activityId);

                return Ok(new ApiResponse<ActivityProductsSummaryDto>
                {
                    Success = true,
                    Data = summary,
                    Message = "Resumen generado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Activity {ActivityId} not found", activityId);
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary for activity {ActivityId}", activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener resumen de productos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener productos de una categoría específica dentro de una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="categoryId">ID de la categoría de servicio</param>
        /// <returns>Lista de productos de la categoría especificada</returns>
        [HttpGet("by-category/{categoryId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryProductDetailedDto>>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetProductsByCategory(
            Guid activityId,
            Guid categoryId)
        {
            try
            {
                _logger.LogInformation(
                    "Getting products for activity {ActivityId} and category {CategoryId}",
                    activityId,
                    categoryId);

                var products = await _activityProductService
                    .GetProductsByActivityCategoryAsync(activityId, categoryId);

                return Ok(new ApiResponse<List<CategoryProductDetailedDto>>
                {
                    Success = true,
                    Data = products,
                    Message = $"Se encontraron {products.Count} productos en la categoría"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting products for activity {ActivityId} and category {CategoryId}",
                    activityId,
                    categoryId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener productos de la categoría",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ============================================
        // GESTIÓN DE PRODUCTOS
        // ============================================

        /// <summary>
        /// Crear un nuevo producto para una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="request">Datos del producto a crear</param>
        /// <returns>Producto creado con información completa</returns>
        /// <remarks>
        /// El producto se crea y se asigna automáticamente a la actividad especificada.
        /// Si no se proporciona ActivityCategoryId, el producto se creará sin asignar a una categoría específica.
        /// </remarks>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CategoryProductDetailedDto>), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateProduct(
            Guid activityId,
            [FromBody] CreateProductForActivityRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Creating product for activity {ActivityId}: {ProductName}",
                    activityId,
                    request.Name);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de producto inválidos",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var product = await _activityProductService
                    .CreateProductForActivityAsync(activityId, request);

                return CreatedAtAction(
                    nameof(GetActivityProducts),
                    new { activityId = activityId },
                    new ApiResponse<CategoryProductDetailedDto>
                    {
                        Success = true,
                        Data = product,
                        Message = "Producto creado exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating product for activity {ActivityId}", activityId);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product for activity {ActivityId}", activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear producto",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Asignar un producto existente a una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="request">Datos de asignación (productId y activityCategoryId)</param>
        /// <returns>Producto asignado con información actualizada</returns>
        /// <remarks>
        /// Permite asignar un producto que ya existe en el sistema a una actividad específica.
        /// El producto debe estar disponible (no asignado a otra actividad).
        /// </remarks>
        [HttpPost("assign")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CategoryProductDetailedDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)] // Conflict - Ya asignado a otra actividad
        [ProducesResponseType(500)]
        public async Task<IActionResult> AssignProduct(
            Guid activityId,
            [FromBody] AssignProductToActivityRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Assigning product {ProductId} to activity {ActivityId}",
                    request.ProductId,
                    activityId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de asignación inválidos",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var product = await _activityProductService
                    .AssignProductToActivityAsync(activityId, request.ProductId, request.ActivityCategoryId);

                if (product == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                return Ok(new ApiResponse<CategoryProductDetailedDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Producto asignado exitosamente a la actividad"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Conflict assigning product {ProductId} to activity {ActivityId}",
                    request.ProductId,
                    activityId);
                return Conflict(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Validation error assigning product {ProductId} to activity {ActivityId}",
                    request.ProductId,
                    activityId);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error assigning product {ProductId} to activity {ActivityId}",
                    request.ProductId,
                    activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al asignar producto a la actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar un producto existente de una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="productId">ID del producto</param>
        /// <param name="request">Datos actualizados del producto</param>
        /// <returns>Producto actualizado</returns>
        [HttpPut("{productId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CategoryProductDetailedDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateProduct(
            Guid activityId,
            Guid productId,
            [FromBody] CreateProductForActivityRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Updating product {ProductId} for activity {ActivityId}",
                    productId,
                    activityId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de producto inválidos",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var product = await _activityProductService
                    .UpdateProductForActivityAsync(activityId, productId, request);

                if (product == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado o no pertenece a esta actividad"
                    });
                }

                return Ok(new ApiResponse<CategoryProductDetailedDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Producto actualizado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Validation error updating product {ProductId} for activity {ActivityId}",
                    productId,
                    activityId);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating product {ProductId} for activity {ActivityId}",
                    productId,
                    activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar producto",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Desasignar un producto de una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="productId">ID del producto</param>
        /// <returns>Resultado de la operación</returns>
        /// <remarks>
        /// ⚠️ IMPORTANTE: Este endpoint DESASIGNA el producto de la actividad, 
        /// NO lo elimina del sistema. El producto quedará disponible para ser 
        /// asignado a otras actividades. Para eliminar definitivamente un producto, 
        /// use el endpoint DELETE /api/products/{id}.
        /// </remarks>
        [HttpDelete("{productId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UnassignProduct(
            Guid activityId,
            Guid productId)
        {
            try
            {
                _logger.LogInformation(
                    "Unassigning product {ProductId} from activity {ActivityId}",
                    productId,
                    activityId);

                var unassigned = await _activityProductService
                    .DeleteProductFromActivityAsync(activityId, productId);

                if (!unassigned)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado o no pertenece a esta actividad"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Producto desasignado de la actividad exitosamente. El producto sigue disponible en el sistema."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error unassigning product {ProductId} from activity {ActivityId}",
                    productId,
                    activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al desasignar producto de la actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // ============================================
        // GESTIÓN DE CATEGORÍAS DE ACTIVIDAD
        // ============================================

        /// <summary>
        /// Obtener las categorías asignadas a una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <returns>Lista de categorías asignadas</returns>
        [HttpGet("~/api/activities/{activityId:guid}/categories")]
        [ProducesResponseType(typeof(ApiResponse<List<ActivityCategoryDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActivityCategories(Guid activityId)
        {
            try
            {
                _logger.LogInformation("Getting categories for activity {ActivityId}", activityId);

                var categories = await _activityProductService
                    .GetActivityCategoriesAsync(activityId);

                return Ok(new ApiResponse<List<ActivityCategoryDto>>
                {
                    Success = true,
                    Data = categories,
                    Message = $"Se encontraron {categories.Count} categorías asignadas"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories for activity {ActivityId}", activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener categorías de la actividad",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener categorías disponibles para asignar a una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <returns>Lista de categorías no asignadas</returns>
        [HttpGet("~/api/activities/{activityId:guid}/categories/available")]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceCategoryDto>>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAvailableCategories(Guid activityId)
        {
            try
            {
                _logger.LogInformation(
                    "Getting available categories for activity {ActivityId}",
                    activityId);

                var categories = await _activityProductService
                    .GetAvailableCategoriesForActivityAsync(activityId);

                return Ok(new ApiResponse<List<ServiceCategoryDto>>
                {
                    Success = true,
                    Data = categories,
                    Message = $"Se encontraron {categories.Count} categorías disponibles"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting available categories for activity {ActivityId}",
                    activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener categorías disponibles",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Asignar múltiples categorías a una actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="request">Lista de IDs de categorías a asignar</param>
        /// <returns>Resultado de la operación batch</returns>
        [HttpPost("~/api/activities/{activityId:guid}/categories/assign")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BatchOperationResultDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AssignCategories(
            Guid activityId,
            [FromBody] AssignCategoriesToActivityRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Assigning {Count} categories to activity {ActivityId}",
                    request.ServiceCategoryIds.Count,
                    activityId);

                // Asegurar que el ActivityId del request coincide con el de la ruta
                request.ActivityId = activityId;

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de asignación inválidos",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _activityProductService
                    .AssignCategoriesToActivityAsync(request);

                var success = result.SuccessCount > 0;
                var statusCode = success ? 200 : 400;

                return StatusCode(statusCode, new ApiResponse<BatchOperationResultDto>
                {
                    Success = success,
                    Data = result,
                    Message = $"Asignadas {result.SuccessCount} de {result.TotalRequested} categorías"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning categories to activity {ActivityId}", activityId);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al asignar categorías",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}