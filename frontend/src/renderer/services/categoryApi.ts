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
      const params = organizationId ? { organizationId } : {};
      const response = await api.get<CategoryApiResponse<ServiceCategory[]>>(
        '/service-categories',
        { params }
      );
      return response.data.data;
    } catch (error) {
      console.error('Error fetching service categories:', error);
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
      return response.data.data;
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
      const response = await api.post<CategoryApiResponse<ServiceCategory>>(
        '/service-categories',
        data
      );
      return response.data.data;
    } catch (error) {
      console.error('Error creating service category:', error);
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
      const response = await api.put<CategoryApiResponse<ServiceCategory>>(
        `/service-categories/${id}`,
        data
      );
      return response.data.data;
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
      return response.data.data;
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
      return response.data.data;
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
      return response.data.data;
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
      return response.data.data;
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