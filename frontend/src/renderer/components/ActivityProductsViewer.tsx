import React, { useState, useEffect } from 'react';
import { activityCategoriesService } from '../services/categoryApi';
import { productsService } from '../services/api';
import { LoadingSpinner } from './LoadingSpinner';
import { Alert } from './Alert';
import type { Product } from '../types';

interface ProductsByCategory {
  categoryId: string;
  categoryName: string;
  products: Product[];
}

interface ActivityProductsViewerProps {
  activityId: string;
  activityName: string;
  onClose: () => void;
}

export const ActivityProductsViewer: React.FC<ActivityProductsViewerProps> = ({
  activityId,
  activityName,
  onClose,
}) => {
  const [productsByCategory, setProductsByCategory] = useState<ProductsByCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  useEffect(() => {
    loadActivityProducts();
  }, [activityId]);

const loadActivityProducts = async () => {
  try {
    setIsLoading(true);
    setError(null);

    // 1. Obtener categorías de la actividad
    const activityCategories = await activityCategoriesService.getActivityCategories(activityId);

    if (activityCategories.length === 0) {
      setError('Esta actividad no tiene categorías asignadas. Por favor, asigna categorías primero.');
      setIsLoading(false);
      return;
    }

    // 2. Para cada categoría, obtener sus productos
    const productsData: ProductsByCategory[] = [];

    for (const actCat of activityCategories) {
      try {
        // Obtener productos de esta categoría
        const allProducts = await productsService.getProducts();
        
        // ✅ CORREGIDO: Filtrar productos que pertenecen a esta categoría
        // Verificar que activityCategoryId no sea null antes de usar toString()
        const categoryProducts = allProducts.filter(
          p => p.activityCategoryId !== null && 
               p.activityCategoryId.toString() === actCat.serviceCategoryId
        );

        if (categoryProducts.length > 0) {
          productsData.push({
            categoryId: actCat.serviceCategoryId,
            categoryName: actCat.serviceCategoryName,
            products: categoryProducts,
          });
        }
      } catch (err) {
        console.error(`Error loading products for category ${actCat.serviceCategoryId}:`, err);
      }
    }

    setProductsByCategory(productsData);
    
    if (productsData.length === 0) {
      setError('No hay productos disponibles en las categorías asignadas a esta actividad.');
    }
  } catch (err: any) {
    console.error('Error loading activity products:', err);
    setError(err.message || 'Error al cargar los productos de la actividad');
  } finally {
    setIsLoading(false);
  }
};

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('es-CR', {
      style: 'currency',
      currency: 'CRC',
      minimumFractionDigits: 0
    }).format(amount);
  };

  const getStockStatus = (current: number, alert: number): {
    label: string;
    color: string;
  } => {
    if (current === 0) {
      return { label: 'Agotado', color: 'bg-red-100 text-red-800' };
    }
    if (current <= alert) {
      return { label: 'Stock Bajo', color: 'bg-yellow-100 text-yellow-800' };
    }
    return { label: 'Disponible', color: 'bg-green-100 text-green-800' };
  };

  const filteredProducts = selectedCategory === 'all' 
    ? productsByCategory 
    : productsByCategory.filter(cat => cat.categoryId === selectedCategory);

  const totalProducts = productsByCategory.reduce((sum, cat) => sum + cat.products.length, 0);
  const totalValue = productsByCategory.reduce((sum, cat) => 
    sum + cat.products.reduce((catSum, p) => catSum + (p.currentQuantity * p.unitPrice), 0)
  , 0);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-6xl w-full max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-gray-200 flex-shrink-0">
          <div className="flex justify-between items-start">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                Productos Disponibles
              </h2>
              <p className="text-gray-600 mt-1">
                Actividad: <span className="font-medium">{activityName}</span>
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          {/* Stats */}
          {!isLoading && !error && (
            <div className="mt-4 grid grid-cols-3 gap-4">
              <div className="bg-blue-50 p-3 rounded-lg">
                <p className="text-xs text-blue-600 font-medium">Categorías</p>
                <p className="text-2xl font-bold text-blue-900">{productsByCategory.length}</p>
              </div>
              <div className="bg-green-50 p-3 rounded-lg">
                <p className="text-xs text-green-600 font-medium">Productos</p>
                <p className="text-2xl font-bold text-green-900">{totalProducts}</p>
              </div>
              <div className="bg-purple-50 p-3 rounded-lg">
                <p className="text-xs text-purple-600 font-medium">Valor Total</p>
                <p className="text-2xl font-bold text-purple-900">{formatCurrency(totalValue)}</p>
              </div>
            </div>
          )}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {isLoading ? (
            <div className="py-12">
              <LoadingSpinner size="medium" message="Cargando productos..." />
            </div>
          ) : error ? (
            <Alert type="warning" message={error} />
          ) : (
            <>
              {/* Filter by Category */}
              {productsByCategory.length > 1 && (
                <div className="mb-6">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Filtrar por categoría
                  </label>
                  <select
                    value={selectedCategory}
                    onChange={(e) => setSelectedCategory(e.target.value)}
                    className="w-full md:w-64 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                             focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="all">Todas las categorías</option>
                    {productsByCategory.map((cat) => (
                      <option key={cat.categoryId} value={cat.categoryId}>
                        {cat.categoryName} ({cat.products.length})
                      </option>
                    ))}
                  </select>
                </div>
              )}

              {/* Products by Category */}
              <div className="space-y-6">
                {filteredProducts.map((category) => (
                  <div key={category.categoryId} className="border border-gray-200 rounded-lg overflow-hidden">
                    {/* Category Header */}
                    <div className="bg-purple-50 px-4 py-3 border-b border-purple-100">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <svg className="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                  d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
                          </svg>
                          <h3 className="text-lg font-semibold text-purple-900">
                            {category.categoryName}
                          </h3>
                        </div>
                        <span className="text-sm text-purple-700 font-medium">
                          {category.products.length} producto{category.products.length !== 1 ? 's' : ''}
                        </span>
                      </div>
                    </div>

                    {/* Products Grid */}
                    <div className="p-4 bg-white">
                      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                        {category.products.map((product) => {
                          const stockStatus = getStockStatus(product.currentQuantity, product.alertQuantity);
                          const stockPercentage = product.initialQuantity > 0 
                            ? (product.currentQuantity / product.initialQuantity) * 100 
                            : 0;

                          return (
                            <div 
                              key={product.id} 
                              className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
                            >
                              {/* Product Header */}
                              <div className="flex items-start justify-between mb-3">
                                <div className="flex-1">
                                  <h4 className="font-medium text-gray-900 mb-1">{product.name}</h4>
                                  <p className="text-xs text-gray-500">Código: {product.code}</p>
                                </div>
                                <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${stockStatus.color}`}>
                                  {stockStatus.label}
                                </span>
                              </div>

                              {/* Description */}
                              {product.description && (
                                <p className="text-sm text-gray-600 mb-3 line-clamp-2">
                                  {product.description}
                                </p>
                              )}

                              {/* Price */}
                              <div className="mb-3">
                                <p className="text-xl font-bold text-blue-600">
                                  {formatCurrency(product.unitPrice)}
                                </p>
                                <p className="text-xs text-gray-500">Precio unitario</p>
                              </div>

                              {/* Stock Info */}
                              <div>
                                <div className="flex items-center justify-between text-xs mb-1">
                                  <span className="text-gray-600">Stock</span>
                                  <span className="font-medium text-gray-900">
                                    {product.currentQuantity} / {product.initialQuantity}
                                  </span>
                                </div>
                                <div className="w-full bg-gray-200 rounded-full h-2">
                                  <div 
                                    className={`h-2 rounded-full transition-all ${
                                      stockPercentage > 50 ? 'bg-green-500' :
                                      stockPercentage > 20 ? 'bg-yellow-500' :
                                      'bg-red-500'
                                    }`}
                                    style={{ width: `${Math.min(stockPercentage, 100)}%` }}
                                  />
                                </div>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>

        {/* Footer */}
        <div className="p-6 border-t border-gray-200 flex-shrink-0 bg-gray-50">
          <div className="flex justify-between items-center">
            <div className="text-sm text-gray-600">
              <span className="font-medium">Nota:</span> Los productos mostrados están compartidos con otras actividades que usan las mismas categorías.
            </div>
            <button
              onClick={onClose}
              className="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 
                       transition-colors font-medium"
            >
              Cerrar
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};