import React, { useState } from 'react';
import { useActivityProducts } from '../hooks/useActivityProducts';
import { LoadingSpinner, InlineSpinner } from './LoadingSpinner';
import { Alert } from './Alert';
import type { Product, CreateProductRequest } from '../types';

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
    products,
    isLoading,
    error,
    createProduct,
    updateProduct,
    deleteProduct,
    clearError,
  } = useActivityProducts(activityId);

  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Estado del formulario
  const [formData, setFormData] = useState<CreateProductRequest>({
    activityCategoryId: 1,
    code: '',
    name: '',
    description: '',
    unitPrice: 0,
    initialQuantity: 0,
    alertQuantity: 10,
  });

  const [formErrors, setFormErrors] = useState<Record<string, string>>({});

  const resetForm = () => {
    setFormData({
      activityCategoryId: 1,
      code: '',
      name: '',
      description: '',
      unitPrice: 0,
      initialQuantity: 0,
      alertQuantity: 10,
    });
    setFormErrors({});
    setEditingProduct(null);
  };

  const handleNewProduct = () => {
    resetForm();
    setShowForm(true);
  };

  const handleEdit = (product: Product) => {
    setEditingProduct(product);
    setFormData({
      activityCategoryId: product.activityCategoryId,
      code: product.code,
      name: product.name,
      description: product.description || '',
      unitPrice: product.unitPrice,
      initialQuantity: product.initialQuantity,
      alertQuantity: product.alertQuantity,
    });
    setShowForm(true);
  };

  const handleDelete = async (product: Product) => {
    if (!confirm(`¿Estás seguro de eliminar el producto "${product.name}"?`)) {
      return;
    }

    try {
      await deleteProduct(activityId, product.id);
    } catch (err) {
      console.error('Error deleting product:', err);
    }
  };

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!formData.name.trim()) {
      errors.name = 'El nombre es requerido';
    }

    if (!formData.code.trim()) {
      errors.code = 'El código es requerido';
    }

    if (formData.unitPrice <= 0) {
      errors.unitPrice = 'El precio debe ser mayor a 0';
    }

    if (formData.initialQuantity < 0) {
      errors.initialQuantity = 'La cantidad no puede ser negativa';
    }

    if (formData.alertQuantity < 0) {
      errors.alertQuantity = 'La cantidad de alerta no puede ser negativa';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      if (editingProduct) {
        await updateProduct(activityId, editingProduct.id, formData);
      } else {
        await createProduct(activityId, formData);
      }
      setShowForm(false);
      resetForm();
    } catch (err) {
      console.error('Error submitting product:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelForm = () => {
    setShowForm(false);
    resetForm();
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

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-6xl w-full max-h-[90vh] overflow-hidden flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-gray-200 flex-shrink-0">
          <div className="flex justify-between items-start">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                Productos de la Actividad
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
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {error && (
            <Alert
              type="error"
              message={error}
              onDismiss={clearError}
            />
          )}

          {isLoading ? (
            <div className="py-12">
              <LoadingSpinner size="medium" message="Cargando productos..." />
            </div>
          ) : (
            <>
              {/* Botón Nuevo Producto */}
              <div className="mb-6">
                <button
                  onClick={handleNewProduct}
                  className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 
                           transition-colors flex items-center gap-2"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  Nuevo Producto
                </button>
              </div>

              {/* Formulario de Producto */}
              {showForm && (
                <div className="mb-6 p-6 bg-blue-50 rounded-lg border border-blue-200">
                  <h3 className="text-lg font-semibold text-gray-900 mb-4">
                    {editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
                  </h3>
                  
                  <form onSubmit={handleSubmit} className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      {/* Nombre */}
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Nombre <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          value={formData.name}
                          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 
                                   focus:ring-blue-500 ${formErrors.name ? 'border-red-500' : 'border-gray-300'}`}
                          disabled={isSubmitting}
                        />
                        {formErrors.name && (
                          <p className="text-sm text-red-600 mt-1">{formErrors.name}</p>
                        )}
                      </div>

                      {/* Código */}
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Código <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          value={formData.code}
                          onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 
                                   focus:ring-blue-500 font-mono ${formErrors.code ? 'border-red-500' : 'border-gray-300'}`}
                          disabled={isSubmitting}
                        />
                        {formErrors.code && (
                          <p className="text-sm text-red-600 mt-1">{formErrors.code}</p>
                        )}
                      </div>

                      {/* Descripción */}
                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Descripción
                        </label>
                        <textarea
                          value={formData.description}
                          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                          rows={2}
                          disabled={isSubmitting}
                        />
                      </div>

                      {/* Precio */}
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Precio Unitario (₡) <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          min="0"
                          value={formData.unitPrice}
                          onChange={(e) => setFormData({ ...formData, unitPrice: Number(e.target.value) })}
                          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 
                                   focus:ring-blue-500 ${formErrors.unitPrice ? 'border-red-500' : 'border-gray-300'}`}
                          disabled={isSubmitting}
                        />
                        {formErrors.unitPrice && (
                          <p className="text-sm text-red-600 mt-1">{formErrors.unitPrice}</p>
                        )}
                      </div>

                      {/* Cantidad Inicial */}
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Cantidad Inicial <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          min="0"
                          value={formData.initialQuantity}
                          onChange={(e) => setFormData({ ...formData, initialQuantity: Number(e.target.value) })}
                          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 
                                   focus:ring-blue-500 ${formErrors.initialQuantity ? 'border-red-500' : 'border-gray-300'}`}
                          disabled={isSubmitting}
                        />
                        {formErrors.initialQuantity && (
                          <p className="text-sm text-red-600 mt-1">{formErrors.initialQuantity}</p>
                        )}
                      </div>

                      {/* Cantidad de Alerta */}
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Cantidad de Alerta <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          min="0"
                          value={formData.alertQuantity}
                          onChange={(e) => setFormData({ ...formData, alertQuantity: Number(e.target.value) })}
                          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 
                                   focus:ring-blue-500 ${formErrors.alertQuantity ? 'border-red-500' : 'border-gray-300'}`}
                          disabled={isSubmitting}
                        />
                        {formErrors.alertQuantity && (
                          <p className="text-sm text-red-600 mt-1">{formErrors.alertQuantity}</p>
                        )}
                      </div>
                    </div>

                    {/* Botones del formulario */}
                    <div className="flex gap-3 pt-4">
                      <button
                        type="button"
                        onClick={handleCancelForm}
                        disabled={isSubmitting}
                        className="px-4 py-2 text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 
                                 rounded-lg transition-colors disabled:opacity-50"
                      >
                        Cancelar
                      </button>
                      <button
                        type="submit"
                        disabled={isSubmitting}
                        className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                                 disabled:opacity-50 flex items-center"
                      >
                        {isSubmitting ? (
                          <>
                            <InlineSpinner className="h-4 w-4 text-white mr-2" />
                            {editingProduct ? 'Actualizando...' : 'Creando...'}
                          </>
                        ) : (
                          editingProduct ? 'Actualizar Producto' : 'Crear Producto'
                        )}
                      </button>
                    </div>
                  </form>
                </div>
              )}

              {/* Lista de Productos */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-3">
                  Productos Asignados ({products.length})
                </h3>

                {products.length === 0 ? (
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
                      Crea un nuevo producto usando el botón de arriba
                    </p>
                  </div>
                ) : (
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {products.map((product) => {
                      const stockStatus = getStockStatus(product.currentQuantity, product.alertQuantity);
                      const stockPercentage = product.initialQuantity > 0 
                        ? (product.currentQuantity / product.initialQuantity) * 100 
                        : 0;

                      return (
                        <div
                          key={product.id}
                          className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
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

                          {/* Actions */}
                          <div className="flex gap-2 pt-3 border-t border-gray-200">
                            <button
                              onClick={() => handleEdit(product)}
                              className="flex-1 bg-blue-50 text-blue-700 px-3 py-2 rounded-lg hover:bg-blue-100 
                                       transition-colors text-sm font-medium flex items-center justify-center gap-1"
                            >
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                              Editar
                            </button>
                            <button
                              onClick={() => handleDelete(product)}
                              className="flex-1 bg-red-50 text-red-700 px-3 py-2 rounded-lg hover:bg-red-100 
                                       transition-colors text-sm font-medium flex items-center justify-center gap-1"
                            >
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                              </svg>
                              Eliminar
                            </button>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </>
          )}
        </div>

        {/* Footer */}
        <div className="p-6 border-t border-gray-200 flex-shrink-0 bg-gray-50 flex justify-end">
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
  );
};