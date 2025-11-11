import React, { useState, useEffect } from 'react';
import { InlineSpinner } from '../../components/LoadingSpinner';
import { useActivityProducts } from '../../hooks/useActivityProducts';
import type { SalesCombo, CreateComboRequest, CreateComboItem } from '../../types/combo';

interface ComboFormProps {
  combo: SalesCombo | null;
  preselectedActivityId?: string;
  isSubmitting: boolean;
  onSubmit: (data: CreateComboRequest) => void;
  onCancel: () => void;
}

export const ComboForm: React.FC<ComboFormProps> = ({
  combo,
  preselectedActivityId,
  isSubmitting,
  onSubmit,
  onCancel,
}) => {
  const [formData, setFormData] = useState<CreateComboRequest>({
    activityId: preselectedActivityId || '',
    name: '',
    description: '',
    comboPrice: 0,
    items: [],
  });

  // ✅ Estado separado para el input del precio
  const [comboPriceInput, setComboPriceInput] = useState('0');

  const [errors, setErrors] = useState<Record<string, string>>({});

  const { 
    products: availableProducts, 
    isLoading: loadingProducts 
  } = useActivityProducts(formData.activityId);

  useEffect(() => {
    if (combo) {
      setFormData({
        activityId: combo.activityId,
        name: combo.name,
        description: combo.description || '',
        comboPrice: combo.comboPrice,
        items: combo.items.map(item => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
      });
      // ✅ Inicializar el input del precio
      setComboPriceInput(combo.comboPrice.toString());
    } else if (preselectedActivityId) {
      setFormData(prev => ({
        ...prev,
        activityId: preselectedActivityId,
      }));
      setComboPriceInput('0');
    }
  }, [combo, preselectedActivityId]);

  // ✅ Handler mejorado para el precio
  const handleComboPriceChange = (value: string) => {
    setComboPriceInput(value);
    
    // Permitir cadena vacía o solo punto mientras se escribe
    if (value === '' || value === '.') {
      setFormData(prev => ({ ...prev, comboPrice: 0 }));
      return;
    }
    
    const numValue = parseFloat(value);
    if (!isNaN(numValue) && numValue >= 0) {
      setFormData(prev => ({ ...prev, comboPrice: numValue }));
    }
  };

  const handleAddItem = () => {
    setFormData({
      ...formData,
      items: [...formData.items, { productId: '', quantity: 1 }],
    });
  };

  const handleRemoveItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index),
    });
  };

  const handleItemChange = (index: number, field: keyof CreateComboItem, value: string | number) => {
    const newItems = [...formData.items];
    newItems[index] = { ...newItems[index], [field]: value };
    setFormData({ ...formData, items: newItems });
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.activityId) {
      newErrors.activityId = 'Debes seleccionar una actividad';
    }

    if (!formData.name.trim()) {
      newErrors.name = 'El nombre es requerido';
    }

    if (formData.comboPrice <= 0) {
      newErrors.comboPrice = 'El precio debe ser mayor a 0';
    }

    if (formData.items.length < 2) {
      newErrors.items = 'Un combo debe tener al menos 2 productos';
    }

    formData.items.forEach((item, index) => {
      if (!item.productId) {
        newErrors[`item_${index}_product`] = 'Selecciona un producto';
      }
      if (item.quantity < 1) {
        newErrors[`item_${index}_quantity`] = 'La cantidad debe ser mayor a 0';
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      onSubmit(formData);
    }
  };

  const shouldHideActivitySelector = !!combo || !!preselectedActivityId;

  // Calcular precio regular del combo
  const regularPrice = formData.items.reduce((total, item) => {
    const product = availableProducts.find(p => p.id === item.productId);
    return total + (product ? product.unitPrice * item.quantity : 0);
  }, 0);

  const savings = regularPrice - formData.comboPrice;
  const savingsPercentage = regularPrice > 0 ? ((savings / regularPrice) * 100).toFixed(0) : 0;

  const activityName = combo?.activityName || 'Actividad seleccionada';

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                {combo ? 'Editar Combo' : 'Nuevo Combo'}
              </h2>
              <p className="text-gray-600 mt-1">
                {combo
                  ? 'Modifica los datos del combo'
                  : 'Crea un nuevo combo de productos con precio especial'}
              </p>
            </div>
            <button
              onClick={onCancel}
              disabled={isSubmitting}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
              type="button"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Actividad - Solo mostrar si NO está pre-seleccionada */}
            {!shouldHideActivitySelector ? (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Actividad <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.activityId}
                  onChange={(e) => {
                    setFormData({ ...formData, activityId: e.target.value, items: [] });
                  }}
                  disabled={isSubmitting}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100
                           ${errors.activityId ? 'border-red-500' : 'border-gray-300'}`}
                >
                  <option value="">Selecciona una actividad...</option>
                  {/* Aquí deberías cargar las actividades con useActivities() */}
                </select>
                {errors.activityId && (
                  <p className="mt-1 text-sm text-red-600">{errors.activityId}</p>
                )}
              </div>
            ) : (
              <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                <div className="flex items-center">
                  <svg className="w-5 h-5 text-blue-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                  <div>
                    <p className="text-sm font-medium text-blue-900">Actividad:</p>
                    <p className="text-sm text-blue-700">{activityName}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Nombre y Precio */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nombre del Combo <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  disabled={isSubmitting}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100
                           ${errors.name ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="Ej: Combo Familiar"
                  autoComplete="off"
                />
                {errors.name && (
                  <p className="mt-1 text-sm text-red-600">{errors.name}</p>
                )}
              </div>

              {/* ✅ Campo de precio corregido */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Precio del Combo (₡) <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  inputMode="decimal"
                  value={comboPriceInput}
                  onChange={(e) => handleComboPriceChange(e.target.value)}
                  disabled={isSubmitting}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100
                           ${errors.comboPrice ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="0.00"
                />
                {/* ✅ Mostrar valor formateado */}
                <p className="text-xs text-gray-500 mt-1">
                  Valor: ₡{formData.comboPrice.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                </p>
                {errors.comboPrice && (
                  <p className="mt-1 text-sm text-red-600">{errors.comboPrice}</p>
                )}
              </div>
            </div>

            {/* Descripción */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Descripción (Opcional)
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                disabled={isSubmitting}
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                         focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
                placeholder="Describe el combo y sus beneficios..."
              />
            </div>

            {/* Productos del Combo */}
            <div>
              <div className="flex justify-between items-center mb-3">
                <label className="block text-sm font-medium text-gray-700">
                  Productos del Combo <span className="text-red-500">*</span>
                  <span className="text-xs text-gray-500 ml-2">(Mínimo 2 productos)</span>
                </label>
                <button
                  type="button"
                  onClick={handleAddItem}
                  disabled={isSubmitting || !formData.activityId || availableProducts.length === 0}
                  className="text-sm text-blue-600 hover:text-blue-700 font-medium disabled:opacity-50"
                >
                  + Agregar Producto
                </button>
              </div>

              {errors.items && (
                <p className="mb-2 text-sm text-red-600">{errors.items}</p>
              )}

              {!formData.activityId && (
                <div className="p-4 bg-gray-50 rounded-lg text-center text-gray-500 text-sm">
                  Selecciona una actividad para ver los productos disponibles
                </div>
              )}

              {loadingProducts && formData.activityId && (
                <div className="p-4 bg-gray-50 rounded-lg text-center">
                  <InlineSpinner className="h-5 w-5 text-blue-600 mx-auto" />
                  <p className="text-sm text-gray-600 mt-2">Cargando productos...</p>
                </div>
              )}

              {formData.activityId && !loadingProducts && availableProducts.length === 0 && (
                <div className="p-4 bg-yellow-50 rounded-lg text-center">
                  <p className="text-sm text-yellow-800">
                    No hay productos disponibles para esta actividad.
                  </p>
                </div>
              )}

              {formData.activityId && !loadingProducts && formData.items.length === 0 && availableProducts.length > 0 && (
                <div className="p-4 bg-gray-50 rounded-lg text-center text-gray-500 text-sm">
                  No hay productos agregados. Haz clic en "Agregar Producto"
                </div>
              )}

              <div className="space-y-3">
                {formData.items.map((item, index) => {
                  const product = availableProducts.find(p => p.id === item.productId);
                  return (
                    <div key={index} className="flex gap-3 items-start p-4 bg-purple-50 rounded-lg">
                      <div className="flex-1">
                        <label className="block text-xs font-medium text-gray-700 mb-1">
                          Producto
                        </label>
                        <select
                          value={item.productId}
                          onChange={(e) => handleItemChange(index, 'productId', e.target.value)}
                          disabled={isSubmitting}
                          className={`w-full px-3 py-2 border rounded-lg text-sm
                                   ${errors[`item_${index}_product`] ? 'border-red-500' : 'border-gray-300'}`}
                        >
                          <option value="">Selecciona...</option>
                          {availableProducts.map((prod) => (
                            <option key={prod.id} value={prod.id}>
                              {prod.name} - ₡{prod.unitPrice.toLocaleString()}
                            </option>
                          ))}
                        </select>
                        {errors[`item_${index}_product`] && (
                          <p className="mt-1 text-xs text-red-600">{errors[`item_${index}_product`]}</p>
                        )}
                      </div>

                      <div className="w-24">
                        <label className="block text-xs font-medium text-gray-700 mb-1">
                          Cantidad
                        </label>
                        <input
                          type="number"
                          min="1"
                          value={item.quantity}
                          onChange={(e) => handleItemChange(index, 'quantity', parseInt(e.target.value) || 1)}
                          disabled={isSubmitting}
                          className={`w-full px-3 py-2 border rounded-lg text-sm
                                   ${errors[`item_${index}_quantity`] ? 'border-red-500' : 'border-gray-300'}`}
                        />
                        {errors[`item_${index}_quantity`] && (
                          <p className="mt-1 text-xs text-red-600">{errors[`item_${index}_quantity`]}</p>
                        )}
                      </div>

                      {product && (
                        <div className="w-28 pt-6">
                          <p className="text-xs text-gray-600">Subtotal:</p>
                          <p className="text-sm font-semibold text-purple-700">
                            ₡{(product.unitPrice * item.quantity).toLocaleString()}
                          </p>
                        </div>
                      )}

                      <button
                        type="button"
                        onClick={() => handleRemoveItem(index)}
                        disabled={isSubmitting}
                        className="mt-6 p-2 text-red-600 hover:bg-red-50 rounded-lg disabled:opacity-50"
                        title="Eliminar producto"
                      >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                        </svg>
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>

            {/* Resumen de precios */}
            {formData.items.length >= 2 && regularPrice > 0 && (
              <div className="p-4 bg-gradient-to-r from-blue-50 to-purple-50 border border-blue-200 rounded-lg">
                <h4 className="text-sm font-semibold text-gray-900 mb-3">Resumen de Precios:</h4>
                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-700">Precio Regular (productos sueltos):</span>
                    <span className="font-semibold text-gray-900 line-through">
                      ₡{regularPrice.toLocaleString()}
                    </span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-700">Precio del Combo:</span>
                    <span className="font-bold text-purple-700 text-lg">
                      ₡{formData.comboPrice.toLocaleString()}
                    </span>
                  </div>
                  {savings > 0 && (
                    <div className="flex justify-between text-sm pt-2 border-t border-blue-300">
                      <span className="text-green-700 font-medium">Ahorro para el cliente:</span>
                      <span className="font-bold text-green-700">
                        ₡{savings.toLocaleString()} ({savingsPercentage}%)
                      </span>
                    </div>
                  )}
                  {savings < 0 && (
                    <div className="p-2 bg-red-50 border border-red-200 rounded mt-2">
                      <p className="text-xs text-red-700">
                        ⚠️ El precio del combo es mayor al precio regular. Considera reducirlo para que sea atractivo.
                      </p>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Alert */}
            <div className="p-3 bg-blue-50 border border-blue-200 rounded-lg">
              <div className="flex">
                <svg className="h-5 w-5 text-blue-400 mr-2 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p className="text-xs text-blue-800">
                  Un combo debe tener al menos 2 productos. El precio del combo debería ser menor al precio regular para resultar atractivo al cliente.
                </p>
              </div>
            </div>

            {/* Botones */}
            <div className="flex gap-3 pt-4 border-t border-gray-200">
              <button
                type="button"
                onClick={onCancel}
                disabled={isSubmitting}
                className="flex-1 px-6 py-3 border border-gray-300 rounded-lg text-gray-700 
                         hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors font-medium"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting || formData.items.length < 2}
                className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                         flex items-center justify-center font-medium"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    {combo ? 'Actualizando...' : 'Creando...'}
                  </>
                ) : combo ? (
                  'Actualizar Combo'
                ) : (
                  'Crear Combo'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};