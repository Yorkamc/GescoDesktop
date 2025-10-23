import { useState, useEffect, useCallback } from 'react';
import { serviceCategoriesService } from '../services/categoryApi';
import type { ServiceCategory, CreateServiceCategoryRequest } from '../types';

export const useServiceCategories = (organizationId?: string) => {
  const [categories, setCategories] = useState<ServiceCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchCategories = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await serviceCategoriesService.getServiceCategories(organizationId);
      setCategories(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cargar las categorías');
      console.error('Error fetching categories:', err);
    } finally {
      setIsLoading(false);
    }
  }, [organizationId]);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  const createCategory = async (data: CreateServiceCategoryRequest): Promise<ServiceCategory> => {
    try {
      setError(null);
      const newCategory = await serviceCategoriesService.createServiceCategory(data);
      setCategories(prev => [...prev, newCategory]);
      return newCategory;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al crear la categoría';
      setError(errorMessage);
      throw err;
    }
  };

  const updateCategory = async (
    id: string,
    data: Partial<CreateServiceCategoryRequest>
  ): Promise<ServiceCategory> => {
    try {
      setError(null);
      const updatedCategory = await serviceCategoriesService.updateServiceCategory(id, data);
      setCategories(prev =>
        prev.map(cat => (cat.id === id ? updatedCategory : cat))
      );
      return updatedCategory;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al actualizar la categoría';
      setError(errorMessage);
      throw err;
    }
  };

  const deleteCategory = async (id: string): Promise<void> => {
    try {
      setError(null);
      await serviceCategoriesService.deleteServiceCategory(id);
      setCategories(prev => prev.filter(cat => cat.id !== id));
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al eliminar la categoría';
      setError(errorMessage);
      throw err;
    }
  };

  const toggleStatus = async (id: string, active: boolean): Promise<void> => {
    try {
      setError(null);
      const updatedCategory = await serviceCategoriesService.toggleServiceCategoryStatus(id, active);
      setCategories(prev =>
        prev.map(cat => (cat.id === id ? updatedCategory : cat))
      );
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cambiar el estado';
      setError(errorMessage);
      throw err;
    }
  };

  const refreshCategories = useCallback(() => {
    fetchCategories();
  }, [fetchCategories]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    categories,
    isLoading,
    error,
    createCategory,
    updateCategory,
    deleteCategory,
    toggleStatus,
    refreshCategories,
    clearError,
  };
};