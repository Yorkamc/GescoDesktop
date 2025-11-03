import api from './api';
import type {
  SalesTransaction,
  CreateSaleRequest,
  CompleteSaleRequest,
  SalesSummary,
  PaymentMethod,
  SalesStatus,
} from '../types/sales';

export const salesService = {
  /**
   * Obtener todas las ventas
   */
  async getSales(
    cashRegisterId?: string,
    startDate?: string,
    endDate?: string
  ): Promise<SalesTransaction[]> {
    try {
      const params: any = {};
      if (cashRegisterId) params.cashRegisterId = cashRegisterId;
      if (startDate) params.startDate = startDate;
      if (endDate) params.endDate = endDate;

      const response = await api.get('/sales', { params });

      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo ventas');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo ventas:', error);

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al cargar las ventas');
    }
  },

  /**
   * Obtener una venta por ID
   */
  async getSale(id: string): Promise<SalesTransaction> {
    try {
      const response = await api.get(`/sales/${id}`);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Venta no encontrada');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo venta:', error);

      if (error.response?.status === 404) {
        throw new Error('Venta no encontrada');
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al cargar la venta');
    }
  },

  /**
   * Crear nueva venta
   */
  async createSale(data: CreateSaleRequest): Promise<SalesTransaction> {
    try {
      console.log('📝 Creando venta:', data);

      const response = await api.post('/sales', data);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error creando venta');
      }
    } catch (error: any) {
      console.error('❌ Error creando venta:', error);

      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al crear la venta');
    }
  },

  /**
   * Actualizar venta
   */
  async updateSale(id: string, data: CreateSaleRequest): Promise<SalesTransaction> {
    try {
      console.log('📝 Actualizando venta:', data);

      const response = await api.put(`/sales/${id}`, data);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error actualizando venta');
      }
    } catch (error: any) {
      console.error('❌ Error actualizando venta:', error);

      if (error.response?.status === 404) {
        throw new Error('Venta no encontrada');
      }

      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al actualizar la venta');
    }
  },

  /**
   * Cancelar venta
   */
  async cancelSale(id: string, reason?: string): Promise<void> {
    try {
      const response = await api.post(`/sales/${id}/cancel`, { reason });

      if (!response.data.success) {
        throw new Error(response.data.message || 'Error cancelando venta');
      }
    } catch (error: any) {
      console.error('❌ Error cancelando venta:', error);

      if (error.response?.status === 404) {
        throw new Error('Venta no encontrada');
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al cancelar la venta');
    }
  },

  /**
   * Completar venta (procesar pagos)
   */
  async completeSale(id: string, data: CompleteSaleRequest): Promise<SalesTransaction> {
    try {
      console.log('💳 Completando venta:', id, data);

      const response = await api.post(`/sales/${id}/complete`, data);

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error completando venta');
      }
    } catch (error: any) {
      console.error('❌ Error completando venta:', error);

      if (error.response?.status === 404) {
        throw new Error('Venta no encontrada');
      }

      if (error.response?.status === 400) {
        throw new Error(error.response.data.message || 'Datos de pago inválidos');
      }

      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }

      throw new Error('Error al completar la venta');
    }
  },

  /**
   * Obtener resumen de ventas
   */
  async getSalesSummary(cashRegisterId?: string, date?: string): Promise<SalesSummary> {
    try {
      const params: any = {};
      if (cashRegisterId) params.cashRegisterId = cashRegisterId;
      if (date) params.date = date;

      const response = await api.get('/sales/summary', { params });

      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error obteniendo resumen');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo resumen:', error);
      throw new Error('Error al cargar el resumen de ventas');
    }
  },

/**
 * Obtener métodos de pago
 */
async getPaymentMethods(): Promise<PaymentMethod[]> {
  try {
    const response = await api.get('/paymentmethods');

    if (response.data.success) {
      return response.data.data || [];
    } else {
      throw new Error('Error obteniendo métodos de pago');
    }
  } catch (error: any) {
    console.error('❌ Error obteniendo métodos de pago:', error);
    throw new Error('Error al cargar métodos de pago');
  }
},

/**
 * Obtener estados de venta
 */
async getSalesStatuses(): Promise<SalesStatus[]> {
  try {
    const response = await api.get('/salesstatuses');

    if (response.data.success) {
      return response.data.data || [];
    } else {
      throw new Error('Error obteniendo estados');
    }
  } catch (error: any) {
    console.error('❌ Error obteniendo estados:', error);
    throw new Error('Error al cargar estados de venta');
  }
},
};