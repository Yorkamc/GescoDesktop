import { useState, useEffect, useCallback } from 'react';
import { cashRegistersService } from '../services/cashRegistersApi';
import type { 
  CashRegister, 
  CreateCashRegisterRequest, 
  CloseCashRegisterRequest,
  CashRegisterClosure 
} from '../types/cashRegister';

export interface UseCashRegistersReturn {
  cashRegisters: CashRegister[];
  openCashRegisters: CashRegister[];
  isLoading: boolean;
  error: string | null;
  createCashRegister: (data: CreateCashRegisterRequest) => Promise<CashRegister | null>;
  updateCashRegister: (id: string, data: CreateCashRegisterRequest) => Promise<CashRegister | null>;
  deleteCashRegister: (id: string) => Promise<boolean>;
  openCashRegister: (id: string) => Promise<boolean>;
  closeCashRegister: (id: string, data: CloseCashRegisterRequest) => Promise<boolean>;
  getLastClosure: (id: string) => Promise<CashRegisterClosure | null>;
  refreshCashRegisters: () => Promise<void>;
  refreshOpenCashRegisters: () => Promise<void>;
  clearError: () => void;
}

export const useCashRegisters = (activityId?: string): UseCashRegistersReturn => {
  const [cashRegisters, setCashRegisters] = useState<CashRegister[]>([]);
  const [openCashRegisters, setOpenCashRegisters] = useState<CashRegister[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadCashRegisters = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await cashRegistersService.getCashRegisters(activityId);
      console.log('✅ Cajas cargadas:', data.length);
      setCashRegisters(data);
    } catch (err: any) {
      const errorMessage = err.message || 'Error al cargar las cajas';
      setError(errorMessage);
      console.error('❌ Error loading cash registers:', err);
    } finally {
      setIsLoading(false);
    }
  }, [activityId]);

  const loadOpenCashRegisters = useCallback(async () => {
    try {
      const data = await cashRegistersService.getOpenCashRegisters();
      console.log('✅ Cajas abiertas:', data.length);
      setOpenCashRegisters(data);
    } catch (err: any) {
      console.error('❌ Error loading open cash registers:', err);
    }
  }, []);

  useEffect(() => {
    loadCashRegisters();
    loadOpenCashRegisters();
  }, [loadCashRegisters, loadOpenCashRegisters]);

  const createCashRegister = useCallback(async (data: CreateCashRegisterRequest): Promise<CashRegister | null> => {
    try {
      setError(null);
      const newCashRegister = await cashRegistersService.createCashRegister(data);
      setCashRegisters(prev => [newCashRegister, ...prev]);
      console.log('✅ Caja creada:', newCashRegister);
      return newCashRegister;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al crear la caja';
      setError(errorMessage);
      console.error('❌ Error creating cash register:', err);
      return null;
    }
  }, []);

  const updateCashRegister = useCallback(async (id: string, data: CreateCashRegisterRequest): Promise<CashRegister | null> => {
    try {
      setError(null);
      const updated = await cashRegistersService.updateCashRegister(id, data);
      setCashRegisters(prev => prev.map(cr => cr.id === id ? updated : cr));
      console.log('✅ Caja actualizada:', updated);
      return updated;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al actualizar la caja';
      setError(errorMessage);
      console.error('❌ Error updating cash register:', err);
      return null;
    }
  }, []);

  const deleteCashRegister = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError(null);
      await cashRegistersService.deleteCashRegister(id);
      setCashRegisters(prev => prev.filter(cr => cr.id !== id));
      console.log('✅ Caja eliminada');
      return true;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al eliminar la caja';
      setError(errorMessage);
      console.error('❌ Error deleting cash register:', err);
      return false;
    }
  }, []);

  const openCashRegister = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError(null);
      const opened = await cashRegistersService.openCashRegister(id);
      setCashRegisters(prev => prev.map(cr => cr.id === id ? opened : cr));
      setOpenCashRegisters(prev => [...prev, opened]);
      console.log('✅ Caja abierta:', opened);
      return true;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al abrir la caja';
      setError(errorMessage);
      console.error('❌ Error opening cash register:', err);
      return false;
    }
  }, []);

  const closeCashRegister = useCallback(async (id: string, data: CloseCashRegisterRequest): Promise<boolean> => {
    try {
      setError(null);
      const closed = await cashRegistersService.closeCashRegister(id, data);
      setCashRegisters(prev => prev.map(cr => cr.id === id ? closed : cr));
      setOpenCashRegisters(prev => prev.filter(cr => cr.id !== id));
      console.log('✅ Caja cerrada:', closed);
      return true;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al cerrar la caja';
      setError(errorMessage);
      console.error('❌ Error closing cash register:', err);
      return false;
    }
  }, []);

  const getLastClosure = useCallback(async (id: string): Promise<CashRegisterClosure | null> => {
    try {
      return await cashRegistersService.getLastClosure(id);
    } catch (err: any) {
      console.error('❌ Error getting last closure:', err);
      return null;
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    cashRegisters,
    openCashRegisters,
    isLoading,
    error,
    createCashRegister,
    updateCashRegister,
    deleteCashRegister,
    openCashRegister,
    closeCashRegister,
    getLastClosure,
    refreshCashRegisters: loadCashRegisters,
    refreshOpenCashRegisters: loadOpenCashRegisters,
    clearError,
  };
};