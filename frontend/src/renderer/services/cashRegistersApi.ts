import api from './api';
import type { 
  CashRegister, 
  CreateCashRegisterRequest, 
  CloseCashRegisterRequest,
  CashRegisterClosure 
} from '../types/cashRegister';
import type { ApiResponse } from '../types';

export const cashRegistersService = {
  /**
   * Obtener todas las cajas registradoras
   */
  async getCashRegisters(activityId?: string): Promise<CashRegister[]> {
    try {
      const params = activityId ? { activityId } : {};
      const response = await api.get<ApiResponse<CashRegister[]>>('/CashRegisters', { params });
      
      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo cajas');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo cajas:', error);
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cargar las cajas registradoras');
    }
  },

  /**
   * Obtener una caja por ID
   */
  async getCashRegister(id: string): Promise<CashRegister> {
    try {
      const response = await api.get<ApiResponse<CashRegister>>(`/CashRegisters/${id}`);
      
      if (response.data.success && response.data.data) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Caja no encontrada');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo caja:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Caja no encontrada');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cargar la caja');
    }
  },

  /**
   * Crear nueva caja registradora
   */
  async createCashRegister(data: CreateCashRegisterRequest): Promise<CashRegister> {
    try {
      console.log('📝 Creando caja registradora:', data);

      const response = await api.post<ApiResponse<CashRegister>>('/CashRegisters', data);
      
      if (response.data.success && response.data.data) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error creando caja');
      }
    } catch (error: any) {
      console.error('❌ Error creando caja:', error);
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al crear la caja');
    }
  },

  /**
   * Actualizar caja registradora
   */
  async updateCashRegister(id: string, data: CreateCashRegisterRequest): Promise<CashRegister> {
    try {
      console.log('📝 Actualizando caja:', data);

      const response = await api.put<ApiResponse<CashRegister>>(`/CashRegisters/${id}`, data);
      
      if (response.data.success && response.data.data) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error actualizando caja');
      }
    } catch (error: any) {
      console.error('❌ Error actualizando caja:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Caja no encontrada');
      }
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al actualizar la caja');
    }
  },

  /**
   * Eliminar caja registradora
   */
  async deleteCashRegister(id: string): Promise<void> {
    try {
      const response = await api.delete<ApiResponse<string>>(`/CashRegisters/${id}`);
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Error eliminando caja');
      }
    } catch (error: any) {
      console.error('❌ Error eliminando caja:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Caja no encontrada');
      }
      
      if (error.response?.status === 409) {
        throw new Error('No se puede eliminar la caja. Tiene transacciones asociadas.');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al eliminar la caja');
    }
  },

  /**
   * Abrir caja registradora
   */
  async openCashRegister(id: string): Promise<CashRegister> {
    try {
      console.log('🔓 Abriendo caja:', id);

      const response = await api.post<ApiResponse<CashRegister>>(`/CashRegisters/${id}/open`);
      
      if (response.data.success && response.data.data) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error abriendo caja');
      }
    } catch (error: any) {
      console.error('❌ Error abriendo caja:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Caja no encontrada');
      }
      
      if (error.response?.status === 400) {
        throw new Error(error.response.data.message || 'La caja ya está abierta');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al abrir la caja');
    }
  },

  /**
   * Cerrar caja registradora
   */
  async closeCashRegister(id: string, data: CloseCashRegisterRequest): Promise<CashRegister> {
    try {
      console.log('🔒 Cerrando caja:', id, data);

      const response = await api.post<ApiResponse<CashRegister>>(`/CashRegisters/${id}/close`, data);
      
      if (response.data.success && response.data.data) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error cerrando caja');
      }
    } catch (error: any) {
      console.error('❌ Error cerrando caja:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Caja no encontrada');
      }
      
      if (error.response?.status === 400) {
        throw new Error(error.response.data.message || 'La caja ya está cerrada');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cerrar la caja');
    }
  },

  /**
   * Obtener cajas abiertas
   */
  async getOpenCashRegisters(): Promise<CashRegister[]> {
    try {
      const response = await api.get<ApiResponse<CashRegister[]>>('/CashRegisters/open');
      
      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo cajas abiertas');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo cajas abiertas:', error);
      throw new Error('Error al cargar las cajas abiertas');
    }
  },

  /**
   * Obtener último cierre de caja
   */
  async getLastClosure(id: string): Promise<CashRegisterClosure | null> {
    try {
      const response = await api.get<ApiResponse<CashRegisterClosure>>(`/CashRegisters/${id}/last-closure`);
      
      if (response.data.success && response.data.data) {
        return response.data.data;
      }
      
      return null;
    } catch (error: any) {
      console.error('❌ Error obteniendo último cierre:', error);
      
      if (error.response?.status === 404) {
        return null; // No hay cierres previos
      }
      
      throw new Error('Error al cargar el último cierre');
    }
  }
};