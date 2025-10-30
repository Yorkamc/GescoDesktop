import React, { useState, useMemo } from 'react';
import { useActivityProducts } from '../hooks/useActivityProducts';
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
    products: allProducts,
    isLoading: loadingAll,
    error: allError,
  } = useProducts();

  const [isAssigning, setIsAssigning] = useState(false);
  const [selectedProductId, setSelectedProductId] = useState('');

  // Productos disponibles para asignar (que no están ya asignados)
  const availableProducts = useMemo(() => {
    const assignedIds = new Set(assignedProducts.map(p => p.id));
    return allProducts.filter(product => product.active && !assignedIds.has(product.id));
  }, [allProducts, assignedProducts]);

  const handleAssign = async () => {
    if (!selectedProductId) return;

    setIsAssigning(true);
    try {
      await assignProduct(activityId, selectedProductId);
      setSelectedProductId('');
    } catch (err) {
      console.error('Error assigning product:', err);
    } finally {
      setIsAssigning(false);
    }
  };

  const handleRemove = async (productId: string, productName: string) => {
    if (!confirm(`¿Estás seguro de que quieres eliminar "${productName}" de esta actividad?`)) {
      return;
    }

    try {
      await removeProduct(activityId, productId);
    } catch (err) {
      console.error('Error removing product:', err);
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

  const isLoading = loadingAssigned || loadingAll;
  const error = assignedError || allError;

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
                
                {availableProducts.length === 0 ? (
                  <p className="text-gray-600 text-sm">
                    No hay productos disponibles para asignar. Todos los productos activos ya están asignados a esta actividad.
                  </p>
                ) : (
                  <div className="flex gap-3">
                    <select
                      value={selectedProductId}
                      onChange={(e) => setSelectedProductId(e.target.value)}
                      disabled={isAssigning}
                      className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                               focus:ring-green-500 focus:border-green-500 disabled:bg-gray-100"
                    >
                      <option value="">Selecciona un producto...</option>
                      {availableProducts.map((product) => (
                        <option key={product.id} value={product.id}>
                          {product.name} - {product.code} ({formatCurrency(product.unitPrice)})
                        </option>
                      ))}
                    </select>
                    <button
                      onClick={handleAssign}
                      disabled={!selectedProductId || isAssigning}
                      className="px-6 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 
                               disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                               flex items-center gap-2"
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
                          {/* Header */}
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

                          {/* Delete Button */}
                          <button
                            onClick={() => handleRemove(product.id, product.name)}
                            className="w-full p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors
                                     flex items-center justify-center gap-2"
                            title="Eliminar producto de la actividad"
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
                                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                              />
                            </svg>
                            <span className="text-sm font-medium">Eliminar de la actividad</span>
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
          <div className="mt-6 pt-6 border-t border-gray-200 flex justify-end">
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