import { useState, useEffect, useCallback } from 'react';
import { activityCategoriesService } from '../services/categoryApi';
import type { ActivityCategory, CreateActivityCategoryRequest } from '../types';

export interface UseActivityCategoriesReturn {
  categories: ActivityCategory[];
  isLoading: boolean;
  error: string | null;
  assignCategory: (activityId: string, serviceCategoryId: string) => Promise<void>;
  removeCategory: (id: string) => Promise<void>;
  refreshCategories: () => Promise<void>;
  clearError: () => void;
}

export const useActivityCategories = (activityId?: string): UseActivityCategoriesReturn => {
  const [categories, setCategories] = useState<ActivityCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchCategories = useCallback(async () => {
    if (!activityId) {
      setCategories([]);
      setIsLoading(false);
      return;
    }

    try {
      setIsLoading(true);
      setError(null);
      const data = await activityCategoriesService.getActivityCategories(activityId);
      setCategories(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar las categorías';
      setError(errorMessage);
      console.error('Error fetching activity categories:', err);
    } finally {
      setIsLoading(false);
    }
  }, [activityId]);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  const assignCategory = async (activityId: string, serviceCategoryId: string): Promise<void> => {
    try {
      setError(null);
      const data: CreateActivityCategoryRequest = {
        activityId,
        serviceCategoryId
      };
      const newCategory = await activityCategoriesService.createActivityCategory(data);
      setCategories(prev => [...prev, newCategory]);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al asignar la categoría';
      setError(errorMessage);
      throw err;
    }
  };

  const removeCategory = async (id: string): Promise<void> => {
    try {
      setError(null);
      await activityCategoriesService.deleteActivityCategory(id);
      setCategories(prev => prev.filter(cat => cat.id !== id));
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al eliminar la categoría';
      setError(errorMessage);
      throw err;
    }
  };

  const refreshCategories = useCallback(() => {
    return fetchCategories();
  }, [fetchCategories]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    categories,
    isLoading,
    error,
    assignCategory,
    removeCategory,
    refreshCategories,
    clearError,
  };
};