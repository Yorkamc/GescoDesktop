import api from './api';
import type { SalesCombo, CreateComboRequest } from '../types/combo';

export const combosService = {
  /**
   * Obtener todos los combos
   */
  async getCombos(activityId?: string): Promise<SalesCombo[]> {
    try {
      const params = activityId ? { activityId } : {};
      const response = await api.get('/salescombos', { params });

      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo combos');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo combos:', error);

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al cargar los combos');
    }
  },

  /**
   * Obtener un combo por ID
   */
  async getCombo(id: string): Promise<SalesCombo> {
    try {
      const response = await api.get(`/salescombos/${id}`);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Combo no encontrado');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo combo:', error);

      if (error.response?.status === 404) {
        throw new Error('Combo no encontrado');
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al cargar el combo');
    }
  },

  /**
   * Crear nuevo combo
   */
  async createCombo(data: CreateComboRequest): Promise<SalesCombo> {
    try {
      console.log('📝 Creando combo:', data);

      const response = await api.post('/salescombos', data);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error creando combo');
      }
    } catch (error: any) {
      console.error('❌ Error creando combo:', error);

      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al crear el combo');
    }
  },

  /**
   * Actualizar combo
   */
  async updateCombo(id: string, data: CreateComboRequest): Promise<SalesCombo> {
    try {
      console.log('📝 Actualizando combo:', data);

      const response = await api.put(`/salescombos/${id}`, data);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error actualizando combo');
      }
    } catch (error: any) {
      console.error('❌ Error actualizando combo:', error);

      if (error.response?.status === 404) {
        throw new Error('Combo no encontrado');
      }

      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al actualizar el combo');
    }
  },

  /**
   * Eliminar combo
   */
  async deleteCombo(id: string): Promise<void> {
    try {
      const response = await api.delete(`/salescombos/${id}`);

      if (!response.data.success) {
        throw new Error(response.data.message || 'Error eliminando combo');
      }
    } catch (error: any) {
      console.error('❌ Error eliminando combo:', error);

      if (error.response?.status === 404) {
        throw new Error('Combo no encontrado');
      }

      if (error.response?.status === 409) {
        throw new Error('No se puede eliminar el combo porque tiene ventas asociadas');
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al eliminar el combo');
    }
  },

  /**
   * Activar/Desactivar combo
   */
  async toggleComboActive(id: string): Promise<void> {
    try {
      console.log('🔄 Cambiando estado del combo:', id);

      const response = await api.post(`/salescombos/${id}/toggle-active`);

      if (!response.data.success) {
        throw new Error(response.data.message || 'Error cambiando estado');
      }
    } catch (error: any) {
      console.error('❌ Error cambiando estado del combo:', error);

      if (error.response?.status === 404) {
        throw new Error('Combo no encontrado');
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al cambiar estado del combo');
    }
  },

  /**
   * Obtener combos activos por actividad
   */
  async getActiveCombos(activityId: string): Promise<SalesCombo[]> {
    try {
      const response = await api.get(`/salescombos/active/${activityId}`);

      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo combos activos');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo combos activos:', error);
      throw new Error('Error al cargar combos activos');
    }
  },
};