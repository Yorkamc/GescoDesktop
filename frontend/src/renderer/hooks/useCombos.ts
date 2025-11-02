import { useState, useEffect, useCallback } from 'react';
import { combosService } from '../services/combosApi';
import type { SalesCombo, CreateComboRequest } from '../types/combo';

export interface UseCombosReturn {
  combos: SalesCombo[];
  isLoading: boolean;
  error: string | null;
  createCombo: (data: CreateComboRequest) => Promise<SalesCombo | null>;
  updateCombo: (id: string, data: CreateComboRequest) => Promise<SalesCombo | null>;
  deleteCombo: (id: string) => Promise<boolean>;
  toggleComboActive: (id: string) => Promise<boolean>;
  refreshCombos: () => Promise<void>;
  clearError: () => void;
}

export const useCombos = (activityId?: string): UseCombosReturn => {
  const [combos, setCombos] = useState<SalesCombo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadCombos = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await combosService.getCombos(activityId);
      console.log('✅ Combos cargados:', data.length);
      setCombos(data);
    } catch (err: any) {
      const errorMessage = err.message || 'Error al cargar los combos';
      setError(errorMessage);
      console.error('❌ Error loading combos:', err);
    } finally {
      setIsLoading(false);
    }
  }, [activityId]);

  useEffect(() => {
    loadCombos();
  }, [loadCombos]);

  const createCombo = useCallback(async (data: CreateComboRequest): Promise<SalesCombo | null> => {
    try {
      setError(null);
      const newCombo = await combosService.createCombo(data);
      setCombos(prev => [newCombo, ...prev]);
      console.log('✅ Combo creado:', newCombo);
      return newCombo;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al crear el combo';
      setError(errorMessage);
      console.error('❌ Error creating combo:', err);
      return null;
    }
  }, []);

  const updateCombo = useCallback(async (id: string, data: CreateComboRequest): Promise<SalesCombo | null> => {
    try {
      setError(null);
      const updated = await combosService.updateCombo(id, data);
      setCombos(prev => prev.map(c => c.id === id ? updated : c));
      console.log('✅ Combo actualizado:', updated);
      return updated;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al actualizar el combo';
      setError(errorMessage);
      console.error('❌ Error updating combo:', err);
      return null;
    }
  }, []);

  const deleteCombo = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError(null);
      await combosService.deleteCombo(id);
      setCombos(prev => prev.filter(c => c.id !== id));
      console.log('✅ Combo eliminado');
      return true;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al eliminar el combo';
      setError(errorMessage);
      console.error('❌ Error deleting combo:', err);
      return false;
    }
  }, []);

  const toggleComboActive = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError(null);
      await combosService.toggleComboActive(id);
      setCombos(prev => prev.map(c => c.id === id ? { ...c, active: !c.active } : c));
      console.log('✅ Estado del combo cambiado');
      return true;
    } catch (err: any) {
      const errorMessage = err.message || 'Error al cambiar estado del combo';
      setError(errorMessage);
      console.error('❌ Error toggling combo:', err);
      return false;
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    combos,
    isLoading,
    error,
    createCombo,
    updateCombo,
    deleteCombo,
    toggleComboActive,
    refreshCombos: loadCombos,
    clearError,
  };
};