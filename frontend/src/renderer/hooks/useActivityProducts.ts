import { useState, useEffect, useCallback } from 'react';
import { activityProductsService } from '../services/activityProductsApi';
import type { Product } from '../types';

export interface UseActivityProductsReturn {
  products: Product[];
  isLoading: boolean;
  error: string | null;
  assignProduct: (activityId: string, productId: string) => Promise<void>;
  removeProduct: (activityId: string, productId: string) => Promise<void>;
  refreshProducts: () => Promise<void>;
  clearError: () => void;
}

export const useActivityProducts = (activityId?: string): UseActivityProductsReturn => {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProducts = useCallback(async () => {
    if (!activityId) {
      setProducts([]);
      setIsLoading(false);
      return;
    }

    try {
      setIsLoading(true);
      setError(null);
      const data = await activityProductsService.getActivityProducts(activityId);
      setProducts(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar los productos';
      setError(errorMessage);
      console.error('Error fetching activity products:', err);
    } finally {
      setIsLoading(false);
    }
  }, [activityId]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const assignProduct = async (activityId: string, productId: string): Promise<void> => {
    try {
      setError(null);
      await activityProductsService.assignProductToActivity(activityId, productId);
      // Recargar la lista de productos después de asignar
      await fetchProducts();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al asignar el producto';
      setError(errorMessage);
      throw err;
    }
  };

  const removeProduct = async (activityId: string, productId: string): Promise<void> => {
    try {
      setError(null);
      await activityProductsService.removeProductFromActivity(activityId, productId);
      setProducts(prev => prev.filter(p => p.id !== productId));
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al eliminar el producto';
      setError(errorMessage);
      throw err;
    }
  };

  const refreshProducts = useCallback(() => {
    return fetchProducts();
  }, [fetchProducts]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    products,
    isLoading,
    error,
    assignProduct,
    removeProduct,
    refreshProducts,
    clearError,
  };
};