using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gesco.Desktop.Core.Interfaces;
using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los productos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryProductDto>>), 200)]
        public async Task<IActionResult> GetProducts([FromQuery] int? categoryId = null)
        {
            try
            {
                var products = await _productService.GetProductsAsync(categoryId);
                
                return Ok(new ApiResponse<List<CategoryProductDto>>
                {
                    Success = true,
                    Data = products,
                    Message = $"Se encontraron {products.Count} productos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener productos",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener producto por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryProductDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                return Ok(new ApiResponse<CategoryProductDto>
                {
                    Success = true,
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener producto",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Crear nuevo producto
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CategoryProductDto>), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de producto inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var product = await _productService.CreateProductAsync(request);
                
                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    new ApiResponse<CategoryProductDto>
                    {
                        Success = true,
                        Data = product,
                        Message = "Producto creado exitosamente"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating product");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al crear producto",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar producto existente
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CategoryProductDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Datos de producto inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var product = await _productService.UpdateProductAsync(id, request);
                
                if (product == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                return Ok(new ApiResponse<CategoryProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Producto actualizado exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating product {ProductId}", id);
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar producto",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Eliminar producto (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(id);
                
                if (!deleted)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Producto eliminado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al eliminar producto",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener productos con stock bajo
        /// </summary>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryProductDto>>), 200)]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync();
                
                return Ok(new ApiResponse<List<CategoryProductDto>>
                {
                    Success = true,
                    Data = products,
                    Message = $"Se encontraron {products.Count} productos con stock bajo"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock products");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener productos con stock bajo",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Actualizar stock de producto
        /// </summary>
        [HttpPut("{id:guid}/stock")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var updated = await _productService.UpdateStockAsync(id, request.NewQuantity, request.Reason);
                
                if (!updated)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Producto no encontrado"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Stock actualizado exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product {ProductId}", id);
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al actualizar stock",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Obtener movimientos de inventario
        /// </summary>
        [HttpGet("inventory-movements")]
        [ProducesResponseType(typeof(ApiResponse<List<InventoryMovementDto>>), 200)]
        public async Task<IActionResult> GetInventoryMovements([FromQuery] Guid? productId = null)
        {
            try
            {
                var movements = await _productService.GetInventoryMovementsAsync(productId);
                
                return Ok(new ApiResponse<List<InventoryMovementDto>>
                {
                    Success = true,
                    Data = movements,
                    Message = $"Se encontraron {movements.Count} movimientos de inventario"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory movements");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "Error al obtener movimientos de inventario",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }

    public class UpdateStockRequest
    {
        public int NewQuantity { get; set; }
        public string? Reason { get; set; }
    }
}