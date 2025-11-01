import { useState, useEffect, useCallback } from 'react';
import { activityProductsService } from '../services/activityProductsApi';
import type { Product } from '../types';

export interface UseActivityProductsReturn {
  products: Product[];
  isLoading: boolean;
  error: string | null;
  assignProduct: (activityId: string, productId: string, activityCategoryId: string) => Promise<void>;
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
      console.log('✅ Productos de actividad cargados:', data.length);
      setProducts(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar los productos';
      setError(errorMessage);
      console.error('❌ Error fetching activity products:', err);
    } finally {
      setIsLoading(false);
    }
  }, [activityId]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const assignProduct = async (
    activityId: string, 
    productId: string, 
    activityCategoryId: string
  ): Promise<void> => {
    try {
      setError(null);
      console.log('⏳ Asignando producto...');
      
      await activityProductsService.assignProductToActivity(
        activityId, 
        productId, 
        activityCategoryId
      );
      
      console.log('✅ Producto asignado, recargando lista...');
      
      // Recargar productos de la actividad
      await fetchProducts();
      
      console.log('✅ Lista actualizada');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al asignar el producto';
      setError(errorMessage);
      console.error('❌ Error en assignProduct:', err);
      throw err;
    }
  };

  const removeProduct = async (activityId: string, productId: string): Promise<void> => {
    try {
      setError(null);
      console.log('⏳ Desasignando producto...');
      
      await activityProductsService.removeProductFromActivity(activityId, productId);
      
      // Actualizar estado local inmediatamente
      setProducts(prev => prev.filter(p => p.id !== productId));
      
      console.log('✅ Producto desasignado');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al eliminar el producto';
      setError(errorMessage);
      console.error('❌ Error en removeProduct:', err);
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