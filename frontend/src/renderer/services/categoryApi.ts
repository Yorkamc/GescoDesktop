// frontend/src/renderer/services/categoryApi.ts
import api from './api';
import type {
  ServiceCategory,
  CreateServiceCategoryRequest,
  ActivityCategory,
  CreateActivityCategoryRequest,
  CategoryApiResponse
} from '../types';

// =====================================================
// SERVICE CATEGORIES
// =====================================================
export const serviceCategoriesService = {
  /**
   * Obtener todas las categorías de servicio
   */
  async getServiceCategories(organizationId?: string): Promise<ServiceCategory[]> {
    try {
      console.log('🔍 Obteniendo categorías de servicio...');
      const params = organizationId ? { organizationId } : {};
      
      const response = await api.get<CategoryApiResponse<ServiceCategory[]>>(
        '/service-categories',
        { params }
      );
      
      console.log('✅ Categorías obtenidas:', response.data);
      
      // Verificar si la respuesta tiene la estructura correcta
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      // Si la respuesta es directamente un array
      if (Array.isArray(response.data)) {
        return response.data;
      }
      
      console.warn('⚠️ Estructura de respuesta inesperada:', response.data);
      return [];
      
    } catch (error: any) {
      console.error('❌ Error fetching service categories:', error);
      
      // Manejo específico de errores
      if (error.response?.status === 404) {
        console.error('❌ Endpoint no encontrado - Verifica que el backend tenga el endpoint /api/service-categories');
        throw new Error('El endpoint de categorías no está disponible. Contacta al administrador.');
      }
      
      if (error.response?.status === 401) {
        console.error('❌ No autorizado - Token inválido o expirado');
        throw new Error('Sesión expirada. Por favor, inicia sesión nuevamente.');
      }
      
      if (error.response?.status === 403) {
        console.error('❌ Prohibido - No tienes permisos');
        throw new Error('No tienes permisos para ver las categorías.');
      }
      
      if (error.code === 'ERR_NETWORK') {
        throw new Error('Error de conexión con el servidor. Verifica que el backend esté corriendo.');
      }
      
      throw error;
    }
  },

  /**
   * Obtener una categoría de servicio por ID
   */
  async getServiceCategoryById(id: string): Promise<ServiceCategory> {
    try {
      const response = await api.get<CategoryApiResponse<ServiceCategory>>(
        `/service-categories/${id}`
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
    } catch (error) {
      console.error('Error fetching service category:', error);
      throw error;
    }
  },

  /**
   * Crear una nueva categoría de servicio
   */
  async createServiceCategory(data: CreateServiceCategoryRequest): Promise<ServiceCategory> {
    try {
      console.log('📝 Creando categoría:', data);
      
      const response = await api.post<CategoryApiResponse<ServiceCategory>>(
        '/service-categories',
        data
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
    } catch (error: any) {
      console.error('Error creating service category:', error);
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      throw error;
    }
  },

  /**
   * Actualizar una categoría de servicio
   */
  async updateServiceCategory(
    id: string,
    data: Partial<CreateServiceCategoryRequest>
  ): Promise<ServiceCategory> {
    try {
      console.log('📝 Actualizando categoría:', id, data);
      
      const response = await api.put<CategoryApiResponse<ServiceCategory>>(
        `/service-categories/${id}`,
        data
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
    } catch (error) {
      console.error('Error updating service category:', error);
      throw error;
    }
  },

  /**
   * Eliminar una categoría de servicio
   */
  async deleteServiceCategory(id: string): Promise<void> {
    try {
      await api.delete(`/service-categories/${id}`);
    } catch (error) {
      console.error('Error deleting service category:', error);
      throw error;
    }
  },

  /**
   * Activar/Desactivar una categoría de servicio
   */
  async toggleServiceCategoryStatus(id: string, active: boolean): Promise<ServiceCategory> {
    try {
      const response = await api.patch<CategoryApiResponse<ServiceCategory>>(
        `/service-categories/${id}/status`,
        { active }
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
    } catch (error) {
      console.error('Error toggling service category status:', error);
      throw error;
    }
  }
};

// =====================================================
// ACTIVITY CATEGORIES
// =====================================================
export const activityCategoriesService = {
  /**
   * Obtener todas las categorías de actividad
   */
  async getActivityCategories(activityId?: string): Promise<ActivityCategory[]> {
    try {
      const params = activityId ? { activityId } : {};
      const response = await api.get<CategoryApiResponse<ActivityCategory[]>>(
        '/activity-categories',
        { params }
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      if (Array.isArray(response.data)) {
        return response.data;
      }
      
      return [];
    } catch (error) {
      console.error('Error fetching activity categories:', error);
      throw error;
    }
  },

  /**
   * Obtener una categoría de actividad por ID
   */
  async getActivityCategoryById(id: string): Promise<ActivityCategory> {
    try {
      const response = await api.get<CategoryApiResponse<ActivityCategory>>(
        `/activity-categories/${id}`
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
    } catch (error) {
      console.error('Error fetching activity category:', error);
      throw error;
    }
  },

  /**
   * Crear una nueva categoría de actividad
   */
  async createActivityCategory(data: CreateActivityCategoryRequest): Promise<ActivityCategory> {
    try {
      const response = await api.post<CategoryApiResponse<ActivityCategory>>(
        '/activity-categories',
        data
      );
      
      if (response.data && response.data.data) {
        return response.data.data;
      }
      
      return response.data as any;
    } catch (error) {
      console.error('Error creating activity category:', error);
      throw error;
    }
  },

  /**
   * Eliminar una categoría de actividad
   */
  async deleteActivityCategory(id: string): Promise<void> {
    try {
      await api.delete(`/activity-categories/${id}`);
    } catch (error) {
      console.error('Error deleting activity category:', error);
      throw error;
    }
  }
};