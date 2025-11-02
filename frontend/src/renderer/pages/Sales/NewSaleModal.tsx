import React, { useState, useEffect } from 'react';
import { InlineSpinner } from '../../components/LoadingSpinner';
import { useSales } from '../../hooks/useSales';
import { useCashRegisters } from '../../hooks/useCashRegisters';
import { useActivityProducts } from '../../hooks/useActivityProducts'; // ✅ Tu hook existente
import type { CreateSaleRequest, CreateSaleItem } from '../../types/sales';

interface NewSaleModalProps {
  cashRegisterId?: string;
  onClose: () => void;
}

export const NewSaleModal: React.FC<NewSaleModalProps> = ({
  cashRegisterId,
  onClose,
}) => {
  const { createSale } = useSales(cashRegisterId);
  const { cashRegisters } = useCashRegisters();
  const [selectedCashRegisterId, setSelectedCashRegisterId] = useState(cashRegisterId || '');
  const [items, setItems] = useState<CreateSaleItem[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  // ✅ Obtener activityId de la caja seleccionada
  const selectedCashRegister = cashRegisters.find(cr => cr.id === selectedCashRegisterId);
  const activityId = selectedCashRegister?.activityId;

  // ✅ Usar tu hook existente
  const { 
    products: availableProducts, 
    isLoading: loadingProducts 
  } = useActivityProducts(activityId);

  const handleAddItem = () => {
    setItems([...items, { productId: '', quantity: 1 }]);
  };

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleItemChange = (index: number, field: keyof CreateSaleItem, value: string | number) => {
    const newItems = [...items];
    newItems[index] = { ...newItems[index], [field]: value };
    setItems(newItems);
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!selectedCashRegisterId) {
      newErrors.cashRegisterId = 'Debes seleccionar una caja';
    }

    if (items.length === 0) {
      newErrors.items = 'Debes agregar al menos un producto';
    }

    items.forEach((item, index) => {
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setIsSubmitting(true);
    try {
      const userStr = localStorage.getItem('user');
      const user = userStr ? JSON.parse(userStr) : null;

      const request: CreateSaleRequest = {
        cashRegisterId: selectedCashRegisterId,
        items,
        createdBy: user?.id || user?.nombreUsuario || '',
      };

      const created = await createSale(request);
      if (created) {
        onClose();
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const openCashRegisters = cashRegisters.filter(cr => cr.isOpen);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">Nueva Venta</h2>
              <p className="text-gray-600 mt-1">Crea una nueva venta pendiente de pago</p>
            </div>
            <button
              onClick={onClose}
              disabled={isSubmitting}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Caja Registradora */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Caja Registradora <span className="text-red-500">*</span>
              </label>
              <select
                value={selectedCashRegisterId}
                onChange={(e) => {
                  setSelectedCashRegisterId(e.target.value);
                  setItems([]); // Limpiar items al cambiar de caja
                }}
                disabled={isSubmitting || !!cashRegisterId}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                         focus:border-blue-500 disabled:bg-gray-100
                         ${errors.cashRegisterId ? 'border-red-500' : 'border-gray-300'}`}
              >
                <option value="">Selecciona una caja...</option>
                {openCashRegisters.map((cr) => (
                  <option key={cr.id} value={cr.id}>
                    {cr.name} - {cr.activityName}
                  </option>
                ))}
              </select>
              {errors.cashRegisterId && (
                <p className="mt-1 text-sm text-red-600">{errors.cashRegisterId}</p>
              )}
              {cashRegisterId && (
                <p className="mt-1 text-sm text-gray-500">
                  Caja pre-seleccionada desde la que vienes
                </p>
              )}
            </div>

            {/* Productos */}
            <div>
              <div className="flex justify-between items-center mb-3">
                <label className="block text-sm font-medium text-gray-700">
                  Productos <span className="text-red-500">*</span>
                </label>
                <button
                  type="button"
                  onClick={handleAddItem}
                  disabled={isSubmitting || !selectedCashRegisterId || availableProducts.length === 0}
                  className="text-sm text-blue-600 hover:text-blue-700 font-medium disabled:opacity-50"
                >
                  + Agregar Producto
                </button>
              </div>

              {errors.items && (
                <p className="mb-2 text-sm text-red-600">{errors.items}</p>
              )}

              {!selectedCashRegisterId && (
                <div className="p-4 bg-gray-50 rounded-lg text-center text-gray-500 text-sm">
                  Selecciona una caja para ver los productos disponibles
                </div>
              )}

              {loadingProducts && selectedCashRegisterId && (
                <div className="p-4 bg-gray-50 rounded-lg text-center">
                  <InlineSpinner className="h-5 w-5 text-blue-600 mx-auto" />
                  <p className="text-sm text-gray-600 mt-2">Cargando productos...</p>
                </div>
              )}

              {selectedCashRegisterId && !loadingProducts && availableProducts.length === 0 && (
                <div className="p-4 bg-yellow-50 rounded-lg text-center">
                  <p className="text-sm text-yellow-800">
                    No hay productos disponibles para esta actividad.
                  </p>
                </div>
              )}

              {selectedCashRegisterId && !loadingProducts && items.length === 0 && availableProducts.length > 0 && (
                <div className="p-4 bg-gray-50 rounded-lg text-center text-gray-500 text-sm">
                  No hay productos agregados. Haz clic en "Agregar Producto"
                </div>
              )}

              <div className="space-y-3">
                {items.map((item, index) => {
                  const product = availableProducts.find(p => p.id === item.productId);
                  return (
                    <div key={index} className="flex gap-3 items-start p-4 bg-gray-50 rounded-lg">
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
                              {prod.currentQuantity !== undefined && ` (Stock: ${prod.currentQuantity})`}
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
                          max={product?.currentQuantity}
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
                          <p className="text-sm font-semibold text-green-700">
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

            {/* Total */}
            {items.length > 0 && (
              <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                <div className="flex justify-between items-center">
                  <span className="text-sm font-medium text-blue-900">Total de la venta:</span>
                  <span className="text-2xl font-bold text-blue-900">
                    ₡{items.reduce((sum, item) => {
                      const product = availableProducts.find(p => p.id === item.productId);
                      return sum + (product ? product.unitPrice * item.quantity : 0);
                    }, 0).toLocaleString()}
                  </span>
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
                  La venta se creará en estado "Pendiente". Deberás completarla con los pagos para finalizar la transacción.
                </p>
              </div>
            </div>

            {/* Botones */}
            <div className="flex gap-3 pt-4 border-t border-gray-200">
              <button
                type="button"
                onClick={onClose}
                disabled={isSubmitting}
                className="flex-1 px-6 py-3 border border-gray-300 rounded-lg text-gray-700 
                         hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors font-medium"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting || items.length === 0}
                className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                         flex items-center justify-center font-medium"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    Creando...
                  </>
                ) : (
                  'Crear Venta'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};