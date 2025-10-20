import React from 'react';
import type { Product } from '../types';

export interface ProductCardProps {
  product: Product;
  onEdit: (product: Product) => void;
  onDelete: (id: string) => void;
  onAdjustStock: (product: Product) => void;
}

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
  return { label: 'En Stock', color: 'bg-green-100 text-green-800' };
};

export const ProductCard: React.FC<ProductCardProps> = ({ 
  product, 
  onEdit, 
  onDelete,
  onAdjustStock 
}) => {
  const stockStatus = getStockStatus(product.currentQuantity, product.alertQuantity);
  const stockPercentage = product.initialQuantity > 0 
    ? (product.currentQuantity / product.initialQuantity) * 100 
    : 0;

  return (
    <div className="bg-white rounded-lg shadow-sm border hover:shadow-md transition-shadow">
      <div className="p-6">
        {/* Header */}
        <div className="flex items-start justify-between mb-4">
          <div className="flex-1">
            <div className="flex items-center gap-2 mb-2">
              <h3 className="text-lg font-medium text-gray-900">{product.name}</h3>
              {!product.active && (
                <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800">
                  Inactivo
                </span>
              )}
            </div>
            <p className="text-sm text-gray-500">Código: {product.code}</p>
          </div>
          
          <div className="flex space-x-2">
            <button
              onClick={() => onAdjustStock(product)}
              className="text-gray-400 hover:text-blue-600"
              title="Ajustar Stock"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16V4m0 0L3 8m4-4l4 4m6 0v12m0 0l4-4m-4 4l-4-4" />
              </svg>
            </button>
            <button
              onClick={() => onEdit(product)}
              className="text-gray-400 hover:text-blue-600"
              title="Editar"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
              </svg>
            </button>
            <button
              onClick={() => onDelete(product.id)}
              className="text-gray-400 hover:text-red-600"
              title="Eliminar"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          </div>
        </div>

        {/* Description */}
        {product.description && (
          <p className="text-gray-600 text-sm mb-4 line-clamp-2">{product.description}</p>
        )}

        {/* Price */}
        <div className="mb-4">
          <p className="text-2xl font-bold text-blue-600">
            {formatCurrency(product.unitPrice)}
          </p>
          <p className="text-xs text-gray-500">Precio unitario</p>
        </div>

        {/* Stock Status Badge */}
        <div className="mb-3">
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${stockStatus.color}`}>
            {stockStatus.label}
          </span>
        </div>

        {/* Stock Bar */}
        <div className="mb-3">
          <div className="flex items-center justify-between text-sm mb-1">
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

        {/* Alert Threshold */}
        <div className="text-xs text-gray-500">
          <div className="flex items-center">
            <svg className="w-3 h-3 mr-1" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            Alerta cuando llegue a: {product.alertQuantity} unidades
          </div>
        </div>
      </div>
    </div>
  );
};