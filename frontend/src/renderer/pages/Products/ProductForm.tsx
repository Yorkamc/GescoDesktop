import React, { useState, useEffect } from 'react';
import { InlineSpinner } from '../../components/LoadingSpinner';
import type { Product, CreateProductRequest } from '../../types';

interface ProductFormProps {
  product: Product | null;
  isSubmitting: boolean;
  onSubmit: (data: CreateProductRequest) => void;
  onCancel: () => void;
}

export const ProductForm: React.FC<ProductFormProps> = ({ 
  product, 
  isSubmitting, 
  onSubmit, 
  onCancel 
}) => {
  const [formData, setFormData] = useState<CreateProductRequest>({
    activityCategoryId: 1,
    code: '',
    name: '',
    description: '',
    unitPrice: 0,
    initialQuantity: 0,
    alertQuantity: 10,
  });

  const [formError, setFormError] = useState('');

  useEffect(() => {
    if (product) {
      setFormData({
        activityCategoryId: product.activityCategoryId,
        code: product.code,
        name: product.name,
        description: product.description || '',
        unitPrice: product.unitPrice,
        initialQuantity: product.initialQuantity,
        alertQuantity: product.alertQuantity,
      });
    }
  }, [product]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    // Validaciones
    if (!formData.name.trim()) {
      setFormError('El nombre del producto es requerido');
      return;
    }

    if (!formData.code.trim()) {
      setFormError('El código del producto es requerido');
      return;
    }

    if (formData.unitPrice <= 0) {
      setFormError('El precio debe ser mayor a 0');
      return;
    }

    if (formData.initialQuantity < 0) {
      setFormError('La cantidad inicial no puede ser negativa');
      return;
    }

    if (formData.alertQuantity < 0) {
      setFormError('La cantidad de alerta no puede ser negativa');
      return;
    }

    onSubmit(formData);
  };

  const handleNumberChange = (field: keyof CreateProductRequest, value: string) => {
    const numValue = value === '' ? 0 : parseFloat(value);
    setFormData(prev => ({ ...prev, [field]: isNaN(numValue) ? 0 : numValue }));
  };

  return (
    <div className="fixed inset-0 bg-gray-900 bg-opacity-50 overflow-y-auto z-50 flex items-center justify-center p-4">
      <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 flex-shrink-0">
          <h3 className="text-xl font-semibold text-gray-900">
            {product ? 'Editar Producto' : 'Nuevo Producto'}
          </h3>
          <button
            onClick={onCancel}
            className="text-gray-400 hover:text-gray-600 transition-colors"
            disabled={isSubmitting}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Form Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {formError && (
            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
              {formError}
            </div>
          )}

          <form onSubmit={handleSubmit} id="productForm" className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Nombre */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre del Producto <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  placeholder="Ej: Entrada General"
                  disabled={isSubmitting}
                />
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
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono"
                  required
                  placeholder="Ej: PROD-001"
                  disabled={isSubmitting}
                />
              </div>

              {/* Categoría */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Categoría <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.activityCategoryId}
                  onChange={(e) => setFormData({ ...formData, activityCategoryId: parseInt(e.target.value) })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={isSubmitting}
                >
                  <option value={1}>General</option>
                  <option value={2}>Entradas</option>
                  <option value={3}>Bebidas</option>
                  <option value={4}>Comida</option>
                  <option value={5}>Merchandising</option>
                </select>
              </div>

              {/* Descripción */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Descripción
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  rows={3}
                  placeholder="Descripción detallada del producto..."
                  disabled={isSubmitting}
                />
              </div>

              {/* Precio Unitario */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Precio Unitario (₡) <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={formData.unitPrice}
                  onChange={(e) => handleNumberChange('unitPrice', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  placeholder="0.00"
                  disabled={isSubmitting}
                />
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
                  onChange={(e) => handleNumberChange('initialQuantity', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  placeholder="100"
                  disabled={isSubmitting}
                />
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
                  onChange={(e) => handleNumberChange('alertQuantity', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  placeholder="10"
                  disabled={isSubmitting}
                />
                <p className="text-xs text-gray-500 mt-1">
                  Se mostrará una alerta cuando el stock llegue a esta cantidad
                </p>
              </div>
            </div>
          </form>
        </div>

        {/* Footer */}
        <div className="flex justify-end items-center space-x-3 p-6 border-t border-gray-200 flex-shrink-0 bg-gray-50">
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors font-medium disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            form="productForm"
            disabled={isSubmitting}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center font-medium"
          >
            {isSubmitting ? (
              <>
                <InlineSpinner className="h-4 w-4 text-white mr-2" />
                {product ? 'Actualizando...' : 'Creando...'}
              </>
            ) : (
              product ? 'Actualizar Producto' : 'Crear Producto'
            )}
          </button>
        </div>
      </div>
    </div>
  );
};