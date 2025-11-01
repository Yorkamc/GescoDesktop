import React, { useState, useMemo, useEffect } from 'react';
import { useActivityProducts } from '../hooks/useActivityProducts';
import { useActivityCategories } from '../hooks/useActivityCategories';
import { useProducts } from '../hooks/useProducts';
import { LoadingSpinner } from './LoadingSpinner';
import { Alert } from './Alert';

interface ActivityProductManagerProps {
  activityId: string;
  activityName: string;
  onClose: () => void;
}

export const ActivityProductManager: React.FC<ActivityProductManagerProps> = ({
  activityId,
  activityName,
  onClose,
}) => {
  const {
    products: assignedProducts,
    isLoading: loadingAssigned,
    error: assignedError,
    assignProduct,
    removeProduct,
    clearError: clearAssignedError,
  } = useActivityProducts(activityId);

  const {
    categories: activityCategories,
    isLoading: loadingCategories,
    error: categoriesError,
    refreshCategories, // ← Asegúrate que tu hook tenga este método
  } = useActivityCategories(activityId);

  const {
    products: allProducts,
    isLoading: loadingAll,
    error: allError,
    refreshProducts: refreshAllProducts, // ← Asegúrate que tu hook tenga este método
  } = useProducts();

  const [isAssigning, setIsAssigning] = useState(false);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedCategoryId, setSelectedCategoryId] = useState('');

  // Filtrar solo productos SIN ASIGNAR
  const availableProducts = useMemo(() => {
    const available = allProducts.filter(product => 
      product.active && 
      (product.activityCategoryId === null || product.activityCategoryId === undefined)
    );
    console.log('🔍 Productos disponibles actualizados:', available.length);
    return available;
  }, [allProducts]);

  // Auto-seleccionar primera categoría si solo hay una
  useEffect(() => {
    if (activityCategories.length === 1 && !selectedCategoryId) {
      setSelectedCategoryId(activityCategories[0].id);
      console.log('✅ Auto-seleccionada categoría:', activityCategories[0].id);
    }
  }, [activityCategories, selectedCategoryId]);

  const handleAssign = async () => {
    if (!selectedProductId || !selectedCategoryId) {
      alert('Por favor selecciona un producto y una categoría');
      return;
    }

    console.log('🚀 Asignando producto:', {
      activityId,
      productId: selectedProductId,
      activityCategoryId: selectedCategoryId
    });

    setIsAssigning(true);
    try {
      // 1. Asignar producto
      await assignProduct(activityId, selectedProductId, selectedCategoryId);
      
      // 2. Refrescar lista completa de productos (esto actualizará availableProducts)
      console.log('🔄 Refrescando lista completa de productos...');
      await refreshAllProducts();
      
      // 3. Limpiar selección
      setSelectedProductId('');
      
      console.log('✅ Todo actualizado correctamente');
    } catch (err: any) {
      console.error('❌ Error assigning product:', err);
      alert(`Error: ${err.message}`);
    } finally {
      setIsAssigning(false);
    }
  };

  const handleRemove = async (productId: string, productName: string) => {
    if (!confirm(`¿Estás seguro de que quieres desasignar "${productName}" de esta actividad?`)) {
      return;
    }

    try {
      // 1. Desasignar producto
      await removeProduct(activityId, productId);
      
      // 2. Refrescar lista completa (el producto volverá a estar disponible)
      console.log('🔄 Refrescando lista completa de productos...');
      await refreshAllProducts();
      
      console.log('✅ Producto desasignado y lista actualizada');
    } catch (err) {
      console.error('❌ Error removing product:', err);
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

  const isLoading = loadingAssigned || loadingAll || loadingCategories;
  const error = assignedError || allError || categoriesError;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-6xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                Gestionar Productos
              </h2>
              <p className="text-gray-600 mt-1">
                Actividad: <span className="font-medium">{activityName}</span>
              </p>
              {/* Debug info */}
              <div className="text-xs text-gray-500 mt-1">
                <span className="mr-3">📦 Disponibles: {availableProducts.length}</span>
                <span className="mr-3">✓ Asignados: {assignedProducts.length}</span>
                <span>📂 Categorías: {activityCategories.length}</span>
              </div>
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

          {error && (
            <Alert
              type="error"
              message={error}
              onDismiss={clearAssignedError}
            />
          )}

          {isLoading ? (
            <div className="py-12">
              <LoadingSpinner size="medium" message="Cargando productos..." />
            </div>
          ) : (
            <>
              {/* Asignar nuevo producto */}
              <div className="mb-6 p-4 bg-green-50 rounded-lg border border-green-200">
                <h3 className="text-lg font-semibold text-gray-900 mb-3">
                  Asignar Producto Existente
                </h3>
                
                {activityCategories.length === 0 ? (
                  <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                    <div className="flex items-start gap-3">
                      <svg className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                      </svg>
                      <div>
                        <p className="text-yellow-800 text-sm font-medium">
                          Esta actividad no tiene categorías asignadas
                        </p>
                        <p className="text-yellow-700 text-xs mt-1">
                          Por favor, asigna categorías primero usando el botón "Gestionar Categorías"
                        </p>
                      </div>
                    </div>
                  </div>
                ) : availableProducts.length === 0 ? (
                  <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                    <div className="flex items-start gap-3">
                      <svg className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <div>
                        <p className="text-blue-800 text-sm font-medium">
                          No hay productos sin asignar disponibles
                        </p>
                        <p className="text-blue-700 text-xs mt-1">
                          Todos los productos activos ya están asignados a alguna actividad. Puedes crear nuevos productos sin asignar en la sección de Productos.
                        </p>
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {/* Selector de Categoría */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        1. Selecciona la categoría <span className="text-red-500">*</span>
                      </label>
                      <select
                        value={selectedCategoryId}
                        onChange={(e) => setSelectedCategoryId(e.target.value)}
                        disabled={isAssigning}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                                 focus:ring-green-500 focus:border-green-500 disabled:bg-gray-100"
                      >
                        <option value="">Selecciona una categoría...</option>
                        {activityCategories.map((category) => (
                          <option key={category.id} value={category.id}>
                            {category.serviceCategoryName}
                          </option>
                        ))}
                      </select>
                      <p className="text-xs text-gray-500 mt-1">
                        El producto se asignará a esta categoría dentro de la actividad
                      </p>
                    </div>

                    {/* Selector de Producto */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        2. Selecciona el producto <span className="text-red-500">*</span>
                      </label>
                      <div className="flex gap-3">
                        <select
                          value={selectedProductId}
                          onChange={(e) => setSelectedProductId(e.target.value)}
                          disabled={isAssigning || !selectedCategoryId}
                          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                                   focus:ring-green-500 focus:border-green-500 disabled:bg-gray-100"
                        >
                          <option value="">
                            {selectedCategoryId 
                              ? 'Selecciona un producto...' 
                              : 'Primero selecciona una categoría'}
                          </option>
                          {availableProducts.map((product) => (
                            <option key={product.id} value={product.id}>
                              {product.name} - {product.code} ({formatCurrency(product.unitPrice)})
                            </option>
                          ))}
                        </select>
                        <button
                          onClick={handleAssign}
                          disabled={!selectedProductId || !selectedCategoryId || isAssigning}
                          className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 
                                   disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                                   flex items-center gap-2 whitespace-nowrap"
                        >
                          {isAssigning ? (
                            <>
                              <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                              </svg>
                              Asignando...
                            </>
                          ) : (
                            <>
                              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                              </svg>
                              Asignar
                            </>
                          )}
                        </button>
                      </div>
                      {availableProducts.length > 0 && (
                        <p className="text-xs text-green-600 mt-1">
                          ✓ {availableProducts.length} producto(s) sin asignar disponible(s)
                        </p>
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Productos asignados */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-3">
                  Productos Asignados ({assignedProducts.length})
                </h3>

                {assignedProducts.length === 0 ? (
                  <div className="text-center py-12 bg-gray-50 rounded-lg">
                    <svg
                      className="mx-auto h-12 w-12 text-gray-400 mb-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
                      />
                    </svg>
                    <h3 className="text-lg font-medium text-gray-900 mb-2">
                      No hay productos asignados
                    </h3>
                    <p className="text-gray-600">
                      Asigna un producto desde el menú de arriba
                    </p>
                  </div>
                ) : (
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {assignedProducts.map((product) => {
                      const stockStatus = getStockStatus(product.currentQuantity, product.alertQuantity);
                      const stockPercentage = product.initialQuantity > 0 
                        ? (product.currentQuantity / product.initialQuantity) * 100 
                        : 0;

                      return (
                        <div
                          key={product.id}
                          className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow bg-white"
                        >
                          <div className="flex items-start justify-between mb-3">
                            <div className="flex-1">
                              <h4 className="font-medium text-gray-900 mb-1">{product.name}</h4>
                              <p className="text-xs text-gray-500">Código: {product.code}</p>
                            </div>
                            <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${stockStatus.color}`}>
                              {stockStatus.label}
                            </span>
                          </div>

                          {product.description && (
                            <p className="text-sm text-gray-600 mb-3 line-clamp-2">
                              {product.description}
                            </p>
                          )}

                          <div className="mb-3">
                            <p className="text-xl font-bold text-blue-600">
                              {formatCurrency(product.unitPrice)}
                            </p>
                            <p className="text-xs text-gray-500">Precio unitario</p>
                          </div>

                          <div className="mb-3">
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

                          <button
                            onClick={() => handleRemove(product.id, product.name)}
                            className="w-full p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors
                                     flex items-center justify-center gap-2"
                            title="Desasignar producto de la actividad"
                          >
                            <svg
                              className="w-5 h-5"
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
                            <span className="text-sm font-medium">Desasignar</span>
                          </button>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </>
          )}

          {/* Footer */}
          <div className="mt-6 pt-6 border-t border-gray-200 flex justify-end gap-3">
            {/* Botón de refrescar manual (útil para debugging) */}
            <button
              onClick={async () => {
                console.log('🔄 Refrescando manualmente...');
                await Promise.all([
                  refreshAllProducts(),
                  refreshCategories && refreshCategories()
                ]);
                console.log('✅ Refrescado completado');
              }}
              className="px-4 py-2 text-gray-600 hover:text-gray-800 transition-colors flex items-center gap-2"
              title="Refrescar listas"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              <span className="text-sm">Refrescar</span>
            </button>
            
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