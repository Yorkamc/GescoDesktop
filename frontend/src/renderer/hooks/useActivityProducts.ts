import { useState, useEffect, useCallback } from 'react';
import { activityProductsService } from '../services/activityProductsApi';
import type { Product, CreateProductRequest } from '../types';

export interface UseActivityProductsReturn {
  products: Product[];
  isLoading: boolean;
  error: string | null;
  createProduct: (activityId: string, product: CreateProductRequest) => Promise<void>;
  updateProduct: (activityId: string, productId: string, product: CreateProductRequest) => Promise<void>;
  deleteProduct: (activityId: string, productId: string) => Promise<void>;
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

  const createProduct = async (activityId: string, product: CreateProductRequest): Promise<void> => {
    try {
      setError(null);
      const newProduct = await activityProductsService.createActivityProduct(activityId, product);
      setProducts(prev => [...prev, newProduct]);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al crear el producto';
      setError(errorMessage);
      throw err;
    }
  };

  const updateProduct = async (
    activityId: string,
    productId: string,
    product: CreateProductRequest
  ): Promise<void> => {
    try {
      setError(null);
      const updatedProduct = await activityProductsService.updateActivityProduct(
        activityId,
        productId,
        product
      );
      setProducts(prev =>
        prev.map(p => (p.id === productId ? updatedProduct : p))
      );
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al actualizar el producto';
      setError(errorMessage);
      throw err;
    }
  };

  const deleteProduct = async (activityId: string, productId: string): Promise<void> => {
    try {
      setError(null);
      await activityProductsService.deleteActivityProduct(activityId, productId);
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
    createProduct,
    updateProduct,
    deleteProduct,
    refreshProducts,
    clearError,
  };
};