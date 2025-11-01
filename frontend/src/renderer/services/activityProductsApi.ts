import api from './api';
import type { Product, ApiResponse } from '../types';

export interface ActivityProductSummary {
  activityId: string;
  activityName: string;
  totalCategories: number;
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  totalInventoryValue: number;
  categoriesWithProducts: {
    activityCategoryId: string;
    serviceCategoryId: string;
    serviceCategoryName: string;
    productCount: number;
    activeProductCount: number;
    products: Product[];
  }[];
}

export const activityProductsService = {
  /**
   * Obtener todos los productos asignados a una actividad
   */
  async getActivityProducts(activityId: string): Promise<Product[]> {
    try {
      console.log('🔍 Obteniendo productos de actividad:', activityId);
      
      const response = await api.get<ApiResponse<Product[]>>(
        `/activities/${activityId}/products`
      );
      
      console.log('✅ Productos obtenidos:', response.data);
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      if (Array.isArray(response.data)) {
        return response.data;
      }
      
      return [];
      
    } catch (error: any) {
      console.error('❌ Error fetching activity products:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Actividad no encontrada');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cargar los productos de la actividad');
    }
  },

  /**
   * Asignar un producto existente a una actividad
   * ✅ CORREGIDO: Ahora usa el endpoint y estructura correctos
   */
  async assignProductToActivity(
    activityId: string, 
    productId: string,
    activityCategoryId: string  // ← ✅ NUEVO PARÁMETRO REQUERIDO
  ): Promise<void> {
    try {
      console.log('➕ Asignando producto a actividad:', { 
        activityId, 
        productId, 
        activityCategoryId 
      });
      
      // ✅ CORRECCIÓN: Usar /assign y enviar body
      await api.post(`/activities/${activityId}/products/assign`, {
        productId,
        activityCategoryId
      });
      
      console.log('✅ Producto asignado exitosamente');
      
    } catch (error: any) {
      console.error('❌ Error assigning product to activity:', error);
      
      if (error.response?.status === 409) {
        throw new Error('Este producto ya está asignado a otra actividad');
      }
      
      if (error.response?.status === 404) {
        throw new Error('Actividad o producto no encontrado');
      }
      
      if (error.response?.status === 400) {
        const message = error.response?.data?.message || 'Datos inválidos';
        throw new Error(message);
      }
      
      throw new Error('Error al asignar el producto a la actividad');
    }
  },

  /**
   * Eliminar un producto de una actividad (desasignar)
   * Nota: Esto NO elimina el producto de la base de datos, solo lo desasocia de la actividad
   */
  async removeProductFromActivity(activityId: string, productId: string): Promise<void> {
    try {
      console.log('➖ Eliminando producto de actividad:', { activityId, productId });
      
      await api.delete(`/activities/${activityId}/products/${productId}`);
      
      console.log('✅ Producto eliminado de la actividad exitosamente');
      
    } catch (error: any) {
      console.error('❌ Error removing product from activity:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Producto no encontrado en la actividad');
      }
      
      throw new Error('Error al eliminar el producto de la actividad');
    }
  },

  /**
   * Obtener resumen de productos de una actividad
   */
  async getActivityProductsSummary(activityId: string): Promise<ActivityProductSummary> {
    try {
      console.log('🔍 Obteniendo resumen de productos:', activityId);
      
      const response = await api.get<ApiResponse<ActivityProductSummary>>(
        `/activities/${activityId}/products/summary`
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
      
    } catch (error: any) {
      console.error('❌ Error fetching products summary:', error);
      throw new Error('Error al cargar el resumen de productos');
    }
  },

  /**
   * Obtener productos de una actividad filtrados por categoría
   */
  async getActivityProductsByCategory(
    activityId: string, 
    categoryId: string
  ): Promise<Product[]> {
    try {
      const response = await api.get<ApiResponse<Product[]>>(
        `/activities/${activityId}/products/by-category/${categoryId}`
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      if (Array.isArray(response.data)) {
        return response.data;
      }
      
      return [];
      
    } catch (error: any) {
      console.error('❌ Error fetching products by category:', error);
      throw new Error('Error al cargar productos por categoría');
    }
  },

  /**
   * ✅ NUEVO: Obtener productos no asignados a ninguna actividad
   */
  async getUnassignedProducts(): Promise<Product[]> {
    try {
      console.log('🔍 Obteniendo productos sin asignar');
      
      const response = await api.get<ApiResponse<Product[]>>('/products/unassigned');
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      if (Array.isArray(response.data)) {
        return response.data;
      }
      
      return [];
      
    } catch (error: any) {
      console.error('❌ Error fetching unassigned products:', error);
      throw new Error('Error al cargar productos sin asignar');
    }
  }
};