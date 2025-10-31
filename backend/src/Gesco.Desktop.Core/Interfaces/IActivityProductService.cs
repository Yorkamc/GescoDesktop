using Gesco.Desktop.Shared.DTOs;

namespace Gesco.Desktop.Core.Interfaces
{
    /// <summary>
    /// Interfaz extendida para gestión de productos por actividad
    /// </summary>
    public interface IActivityProductService
    {
        // ============================================
        // CONSULTAS POR ACTIVIDAD
        // ============================================
        
        /// <summary>
        /// Obtiene todos los productos de una actividad específica
        /// </summary>
        Task<List<CategoryProductDetailedDto>> GetProductsByActivityAsync(Guid activityId);
        
        /// <summary>
        /// Obtiene resumen de productos por actividad con agrupación por categoría
        /// </summary>
        Task<ActivityProductsSummaryDto> GetActivityProductsSummaryAsync(Guid activityId);
        
        /// <summary>
        /// Obtiene productos de una categoría específica dentro de una actividad
        /// </summary>
        Task<List<CategoryProductDetailedDto>> GetProductsByActivityCategoryAsync(
            Guid activityId, 
            Guid serviceCategoryId);
        
        // ============================================
        // GESTIÓN DE PRODUCTOS
        // ============================================
        
        /// <summary>
        /// Crea un producto para una actividad específica
        /// Valida que la ActivityCategory pertenezca a la actividad
        /// </summary>
        Task<CategoryProductDetailedDto> CreateProductForActivityAsync(
            Guid activityId,
            CreateProductForActivityRequest request);
        
        /// <summary>
        /// Asigna un producto existente a una actividad
        /// Valida que la ActivityCategory pertenezca a la actividad
        /// y que el producto no esté ya asignado a otra actividad
        /// </summary>
        /// <param name="activityId">ID de la actividad</param>
        /// <param name="productId">ID del producto existente a asignar</param>
        /// <param name="activityCategoryId">ID de la categoría de actividad</param>
        /// <returns>Producto asignado con información completa</returns>
        Task<CategoryProductDetailedDto?> AssignProductToActivityAsync(
            Guid activityId,
            Guid productId,
            Guid activityCategoryId);
        
        /// <summary>
        /// Actualiza un producto existente
        /// Valida que el producto pertenezca a la actividad
        /// </summary>
        Task<CategoryProductDetailedDto?> UpdateProductForActivityAsync(
            Guid activityId,
            Guid productId,
            CreateProductForActivityRequest request);
        
        /// <summary>
        /// Elimina un producto de una actividad
        /// </summary>
        Task<bool> DeleteProductFromActivityAsync(Guid activityId, Guid productId);
        
        // ============================================
        // GESTIÓN DE CATEGORÍAS DE ACTIVIDAD
        // ============================================
        
        /// <summary>
        /// Asigna múltiples categorías a una actividad
        /// </summary>
        Task<BatchOperationResultDto> AssignCategoriesToActivityAsync(
            AssignCategoriesToActivityRequest request);
        
        /// <summary>
        /// Obtiene las categorías disponibles para asignar a una actividad
        /// (categorías que aún no están asignadas)
        /// </summary>
        Task<List<ServiceCategoryDto>> GetAvailableCategoriesForActivityAsync(
            Guid activityId);
        
        /// <summary>
        /// Obtiene las categorías asignadas a una actividad
        /// </summary>
        Task<List<ActivityCategoryDto>> GetActivityCategoriesAsync(Guid activityId);
        
        // ============================================
        // VALIDACIONES
        // ============================================
        
        /// <summary>
        /// Valida que una ActivityCategory pertenezca a la actividad especificada
        /// </summary>
        Task<bool> ValidateActivityCategoryBelongsToActivityAsync(
            Guid activityId, 
            Guid activityCategoryId);
        
        /// <summary>
        /// Valida que un producto pertenezca a la actividad especificada
        /// </summary>
        Task<bool> ValidateProductBelongsToActivityAsync(
            Guid activityId, 
            Guid productId);
    }
}