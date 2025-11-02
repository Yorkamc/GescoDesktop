import React from 'react';
import type { SalesCombo } from '../../types/combo';

interface ComboCardProps {
  combo: SalesCombo;
  onEdit: (combo: SalesCombo) => void;
  onDelete: (id: string, name: string) => void;
  onToggleActive: (id: string) => void;
}

export const ComboCard: React.FC<ComboCardProps> = ({
  combo,
  onEdit,
  onDelete,
  onToggleActive,
}) => {
  const regularPrice = combo.items.reduce(
    (total, item) => total + (item.productPrice * item.quantity),
    0
  );
  const savings = regularPrice - combo.comboPrice;
  const savingsPercentage = regularPrice > 0 ? ((savings / regularPrice) * 100).toFixed(0) : 0;

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            {combo.name}
          </h3>
          <span
            className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
              combo.active
                ? 'bg-green-100 text-green-800'
                : 'bg-gray-100 text-gray-800'
            }`}
          >
            {combo.active ? '✓ Activo' : '✗ Inactivo'}
          </span>
        </div>
      </div>

      {/* Description */}
      {combo.description && (
        <p className="text-sm text-gray-600 mb-4 line-clamp-2">
          {combo.description}
        </p>
      )}

      {/* Info */}
      <div className="space-y-2 mb-4 text-sm text-gray-600">
        {/* Actividad */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          <span className="font-medium">Actividad:</span>
          <span className="ml-1 truncate">{combo.activityName}</span>
        </div>

        {/* Productos */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
          </svg>
          <span className="font-medium">Productos:</span>
          <span className="ml-1">{combo.items.length}</span>
        </div>

        {/* Precio Regular */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span className="font-medium">Precio Regular:</span>
          <span className="ml-1 line-through text-gray-500">₡{regularPrice.toLocaleString()}</span>
        </div>

        {/* Precio Combo */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span className="font-medium">Precio Combo:</span>
          <span className="ml-1 font-bold text-green-600">₡{combo.comboPrice.toLocaleString()}</span>
        </div>

        {/* Ahorro */}
        {savings > 0 && (
          <div className="flex items-center text-blue-600">
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
            </svg>
            <span className="font-medium">Ahorro:</span>
            <span className="ml-1 font-bold">₡{savings.toLocaleString()} ({savingsPercentage}%)</span>
          </div>
        )}
      </div>

      {/* Items del combo */}
      <div className="mb-4 p-3 bg-purple-50 rounded-lg">
        <h4 className="text-xs font-semibold text-purple-700 mb-2">Incluye:</h4>
        <div className="space-y-1">
          {combo.items.slice(0, 3).map((item) => (
            <div key={item.id} className="flex justify-between text-xs text-purple-700">
              <span className="truncate flex-1">
                {item.quantity}x {item.productName}
              </span>
              <span className="font-medium ml-2">₡{(item.productPrice * item.quantity).toLocaleString()}</span>
            </div>
          ))}
          {combo.items.length > 3 && (
            <p className="text-xs text-purple-600 italic">
              +{combo.items.length - 3} productos más...
            </p>
          )}
        </div>
      </div>

      {/* Botones */}
      <div className="grid grid-cols-2 gap-2">
        {/* Activar/Desactivar */}
        <button
          onClick={() => onToggleActive(combo.id)}
          className={`px-3 py-2 rounded-lg transition-colors flex items-center justify-center gap-2 text-sm font-medium ${
            combo.active
              ? 'bg-gray-50 text-gray-700 hover:bg-gray-100'
              : 'bg-green-50 text-green-700 hover:bg-green-100'
          }`}
        >
          {combo.active ? (
            <>
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
              </svg>
              Desactivar
            </>
          ) : (
            <>
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Activar
            </>
          )}
        </button>

        {/* Editar */}
        <button
          onClick={() => onEdit(combo)}
          className="bg-blue-50 text-blue-700 px-3 py-2 rounded-lg hover:bg-blue-100 
                   transition-colors flex items-center justify-center gap-2 text-sm font-medium"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
          Editar
        </button>

        {/* Eliminar */}
        <button
          onClick={() => onDelete(combo.id, combo.name)}
          className="col-span-2 bg-red-50 text-red-700 px-3 py-2 rounded-lg hover:bg-red-100 
                   transition-colors flex items-center justify-center gap-2 text-sm font-medium"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
          Eliminar
        </button>
      </div>
    </div>
  );
};