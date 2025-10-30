import api from './api';
import type { Product, CreateProductRequest, ApiResponse } from '../types';

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
   * Obtener todos los productos de una actividad
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
   * Obtener productos de una actividad por categoría
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
   * Crear un producto para una actividad
   */
  async createActivityProduct(
    activityId: string,
    product: CreateProductRequest
  ): Promise<Product> {
    try {
      console.log('📝 Creando producto para actividad:', activityId, product);
      
      const response = await api.post<ApiResponse<Product>>(
        `/activities/${activityId}/products`,
        product
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
      
    } catch (error: any) {
      console.error('❌ Error creating activity product:', error);
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      if (error.response?.status === 404) {
        throw new Error('Actividad no encontrada');
      }
      
      throw new Error('Error al crear el producto');
    }
  },

  /**
   * Actualizar un producto de una actividad
   */
  async updateActivityProduct(
    activityId: string,
    productId: string,
    product: CreateProductRequest
  ): Promise<Product> {
    try {
      console.log('📝 Actualizando producto:', productId, product);
      
      const response = await api.put<ApiResponse<Product>>(
        `/activities/${activityId}/products/${productId}`,
        product
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
      
    } catch (error: any) {
      console.error('❌ Error updating activity product:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Producto no encontrado');
      }
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      throw new Error('Error al actualizar el producto');
    }
  },

  /**
   * Eliminar un producto de una actividad
   */
  async deleteActivityProduct(activityId: string, productId: string): Promise<void> {
    try {
      console.log('🗑️ Eliminando producto:', productId);
      
      await api.delete(`/activities/${activityId}/products/${productId}`);
      
    } catch (error: any) {
      console.error('❌ Error deleting activity product:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Producto no encontrado');
      }
      
      throw new Error('Error al eliminar el producto');
    }
  }
};