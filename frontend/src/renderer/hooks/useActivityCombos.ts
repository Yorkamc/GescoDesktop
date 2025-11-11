import { useState, useEffect, useCallback } from 'react';
import { combosService } from '../services/combosApi';
import type { SalesCombo } from '../types/combo';

export interface UseActivityCombosReturn {
  combos: SalesCombo[];
  isLoading: boolean;
  error: string | null;
  refreshCombos: () => Promise<void>;
  clearError: () => void;
}

export const useActivityCombos = (activityId?: string): UseActivityCombosReturn => {
  const [combos, setCombos] = useState<SalesCombo[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadCombos = useCallback(async () => {
    if (!activityId) {
      setCombos([]);
      return;
    }

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

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    combos,
    isLoading,
    error,
    refreshCombos: loadCombos,
    clearError,
  };
};