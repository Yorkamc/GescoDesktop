import React, { useState } from 'react';
import { InlineSpinner } from '../../components/LoadingSpinner';
import type { Product, UpdateStockRequest } from '../../types';

interface StockAdjustmentModalProps {
  product: Product;
  isSubmitting: boolean;
  onSubmit: (adjustment: UpdateStockRequest) => void;
  onCancel: () => void;
}

export const StockAdjustmentModal: React.FC<StockAdjustmentModalProps> = ({
  product,
  isSubmitting,
  onSubmit,
  onCancel,
}) => {
  const [newQuantity, setNewQuantity] = useState(product.currentQuantity);
  const [reason, setReason] = useState('');
  const [formError, setFormError] = useState('');

  const difference = newQuantity - product.currentQuantity;
  const isIncrease = difference > 0;
  const isDecrease = difference < 0;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    if (newQuantity < 0) {
      setFormError('La cantidad no puede ser negativa');
      return;
    }

    if (difference === 0) {
      setFormError('La cantidad debe ser diferente a la actual');
      return;
    }

    if (!reason.trim()) {
      setFormError('Debe proporcionar una razón para el ajuste');
      return;
    }

    onSubmit({ newQuantity, reason: reason.trim() });
  };

  return (
    <div className="fixed inset-0 bg-gray-900 bg-opacity-50 overflow-y-auto z-50 flex items-center justify-center p-4">
      <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-md">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h3 className="text-xl font-semibold text-gray-900">
            Ajustar Stock
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

        {/* Content */}
        <div className="p-6">
          {/* Product Info */}
          <div className="bg-gray-50 rounded-lg p-4 mb-4">
            <h4 className="font-medium text-gray-900 mb-1">{product.name}</h4>
            <p className="text-sm text-gray-500">Código: {product.code}</p>
            <div className="mt-3 flex items-baseline">
              <span className="text-2xl font-bold text-gray-900">
                {product.currentQuantity}
              </span>
              <span className="ml-2 text-sm text-gray-500">unidades actuales</span>
            </div>
          </div>

          {formError && (
            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
              {formError}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            {/* New Quantity */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nueva Cantidad <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                min="0"
                value={newQuantity}
                onChange={(e) => setNewQuantity(parseInt(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
                disabled={isSubmitting}
              />
            </div>

            {/* Difference Display */}
            {difference !== 0 && (
              <div className={`p-3 rounded-lg ${
                isIncrease ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'
              }`}>
                <div className="flex items-center">
                  {isIncrease ? (
                    <svg className="w-5 h-5 text-green-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5 text-red-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 14l-7 7m0 0l-7-7m7 7V3" />
                    </svg>
                  )}
                  <span className={`font-medium ${isIncrease ? 'text-green-800' : 'text-red-800'}`}>
                    {isIncrease ? 'Incremento' : 'Disminución'} de {Math.abs(difference)} unidades
                  </span>
                </div>
              </div>
            )}

            {/* Reason */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Razón del Ajuste <span className="text-red-500">*</span>
              </label>
              <textarea
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                rows={3}
                required
                placeholder="Ej: Inventario físico, pérdida, donación, corrección..."
                disabled={isSubmitting}
              />
            </div>

            {/* Actions */}
            <div className="flex justify-end space-x-3 pt-4">
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
                disabled={isSubmitting || difference === 0}
                className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center font-medium"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    Ajustando...
                  </>
                ) : (
                  'Aplicar Ajuste'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};