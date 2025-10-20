import { useState, useEffect, useCallback } from 'react';
import { productsService } from '../services/api';
import type { Product, CreateProductRequest, UpdateStockRequest } from '../types';

export interface UseProductsReturn {
  products: Product[];
  isLoading: boolean;
  error: string;
  createProduct: (data: CreateProductRequest) => Promise<Product | null>;
  updateProduct: (id: string, data: CreateProductRequest) => Promise<Product | null>;
  deleteProduct: (id: string) => Promise<boolean>;
  adjustStock: (id: string, adjustment: UpdateStockRequest) => Promise<boolean>;
  getLowStockProducts: () => Promise<Product[]>;
  refreshProducts: () => Promise<void>;
  clearError: () => void;
}

export const useProducts = (categoryId?: number): UseProductsReturn => {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const loadProducts = useCallback(async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await productsService.getProducts(categoryId);
      setProducts(data);
    } catch (err: any) {
      setError(err.message || 'Error al cargar productos');
      console.error('Error loading products:', err);
    } finally {
      setIsLoading(false);
    }
  }, [categoryId]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  const createProduct = useCallback(async (data: CreateProductRequest): Promise<Product | null> => {
    try {
      setError('');
      const newProduct = await productsService.createProduct(data);
      setProducts(prev => [newProduct, ...prev]);
      return newProduct;
    } catch (err: any) {
      setError(err.message || 'Error al crear producto');
      console.error('Error creating product:', err);
      return null;
    }
  }, []);

  const updateProduct = useCallback(async (id: string, data: CreateProductRequest): Promise<Product | null> => {
    try {
      setError('');
      const updatedProduct = await productsService.updateProduct(id, data);
      setProducts(prev => prev.map(p => p.id === id ? updatedProduct : p));
      return updatedProduct;
    } catch (err: any) {
      setError(err.message || 'Error al actualizar producto');
      console.error('Error updating product:', err);
      return null;
    }
  }, []);

  const deleteProduct = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError('');
      await productsService.deleteProduct(id);
      setProducts(prev => prev.filter(p => p.id !== id));
      return true;
    } catch (err: any) {
      setError(err.message || 'Error al eliminar producto');
      console.error('Error deleting product:', err);
      return false;
    }
  }, []);

  const adjustStock = useCallback(async (id: string, adjustment: UpdateStockRequest): Promise<boolean> => {
    try {
      setError('');
      await productsService.adjustStock(id, adjustment);
      
      // Actualizar el producto en la lista con la nueva cantidad
      setProducts(prev => prev.map(p => 
        p.id === id 
          ? { ...p, currentQuantity: adjustment.newQuantity }
          : p
      ));
      
      return true;
    } catch (err: any) {
      setError(err.message || 'Error al ajustar stock');
      console.error('Error adjusting stock:', err);
      return false;
    }
  }, []);

  const getLowStockProducts = useCallback(async (): Promise<Product[]> => {
    try {
      setError('');
      return await productsService.getLowStockProducts();
    } catch (err: any) {
      setError(err.message || 'Error al cargar productos con stock bajo');
      console.error('Error loading low stock products:', err);
      return [];
    }
  }, []);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  return {
    products,
    isLoading,
    error,
    createProduct,
    updateProduct,
    deleteProduct,
    adjustStock,
    getLowStockProducts,
    refreshProducts: loadProducts,
    clearError,
  };
};