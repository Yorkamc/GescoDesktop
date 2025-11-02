import { useState, useEffect, useCallback } from 'react';
import { salesService } from '../services/salesApi';
import type {
  SalesTransaction,
  CreateSaleRequest,
  CompleteSaleRequest,
  SalesSummary,
  PaymentMethod,
  SalesStatus,
} from '../types/sales';

export interface UseSalesReturn {
  sales: SalesTransaction[];
  paymentMethods: PaymentMethod[];
  salesStatuses: SalesStatus[];
  isLoading: boolean;
  error: string | null;
  createSale: (data: CreateSaleRequest) => Promise<SalesTransaction | null>;
  updateSale: (id: string, data: CreateSaleRequest) => Promise<SalesTransaction | null>;
  cancelSale: (id: string, reason?: string) => Promise<boolean>;
  completeSale: (id: string, data: CompleteSaleRequest) => Promise<SalesTransaction | null>;
  getSalesSummary: (cashRegisterId?: string, date?: string) => Promise<SalesSummary | null>;
  refreshSales: () => Promise<void>;
  clearError: () => void;
}

export const useSales = (cashRegisterId?: string): UseSalesReturn => {
  const [sales, setSales] = useState<SalesTransaction[]>([]);
  const [paymentMethods, setPaymentMethods] = useState<PaymentMethod[]>([]);
  const [salesStatuses, setSalesStatuses] = useState<SalesStatus[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadSales = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await salesService.getSales(cashRegisterId);
      console.log('✅ Ventas cargadas:', data.length);
      setSales(data);
    } catch (err: any) {
      const errorMessage = err.message || 'Error al cargar las ventas';
      setError(errorMessage);
      console.error('❌ Error loading sales:', err);
    } finally {
      setIsLoading(false);
    }
  }, [cashRegisterId]);

  const loadPaymentMethods = useCallback(async () => {
    try {
      const data = await salesService.getPaymentMethods();
      console.log('✅ Métodos de pago cargados:', data.length);
      setPaymentMethods(data);
    } catch (err: any) {
      console.error('❌ Error loading payment methods:', err);
    }
  }, []);

  const loadSalesStatuses = useCallback(async () => {
    try {
      const data = await salesService.getSalesStatuses();
      console.log('✅ Estados de venta cargados:', data.length);
      setSalesStatuses(data);
    } catch (err: any) {
      console.error('❌ Error loading sales statuses:', err);
    }
  }, []);

  useEffect(() => {
    loadSales();
    loadPaymentMethods();
    loadSalesStatuses();
  }, [loadSales, loadPaymentMethods, loadSalesStatuses]);

  const createSale = useCallback(async (data: CreateSaleRequest): Promise<SalesTransaction | null> => {
    try {
      setError(null);
      const newSale = await salesService.createSale(data);
      setSales(prev => [newSale, ...prev]);
      console.log('✅ Venta creada:', newSale);
      return newSale;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al crear la venta';
      setError(errorMessage);
      console.error('❌ Error creating sale:', err);
      return null;
    }
  }, []);

  const updateSale = useCallback(async (id: string, data: CreateSaleRequest): Promise<SalesTransaction | null> => {
    try {
      setError(null);
      const updated = await salesService.updateSale(id, data);
      setSales(prev => prev.map(s => s.id === id ? updated : s));
      console.log('✅ Venta actualizada:', updated);
      return updated;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al actualizar la venta';
      setError(errorMessage);
      console.error('❌ Error updating sale:', err);
      return null;
    }
  }, []);

  const cancelSale = useCallback(async (id: string, reason?: string): Promise<boolean> => {
    try {
      setError(null);
      await salesService.cancelSale(id, reason);
      await loadSales(); // Recargar para actualizar el estado
      console.log('✅ Venta cancelada');
      return true;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al cancelar la venta';
      setError(errorMessage);
      console.error('❌ Error cancelling sale:', err);
      return false;
    }
  }, [loadSales]);

  const completeSale = useCallback(async (id: string, data: CompleteSaleRequest): Promise<SalesTransaction | null> => {
    try {
      setError(null);
      const completed = await salesService.completeSale(id, data);
      setSales(prev => prev.map(s => s.id === id ? completed : s));
      console.log('✅ Venta completada:', completed);
      return completed;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al completar la venta';
      setError(errorMessage);
      console.error('❌ Error completing sale:', err);
      return null;
    }
  }, []);

  const getSalesSummary = useCallback(async (cashRegisterId?: string, date?: string): Promise<SalesSummary | null> => {
    try {
      return await salesService.getSalesSummary(cashRegisterId, date);
    } catch (err: any) {
      console.error('❌ Error getting sales summary:', err);
      return null;
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    sales,
    paymentMethods,
    salesStatuses,
    isLoading,
    error,
    createSale,
    updateSale,
    cancelSale,
    completeSale,
    getSalesSummary,
    refreshSales: loadSales,
    clearError,
  };
};