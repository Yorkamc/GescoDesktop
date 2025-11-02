import React, { useState } from 'react';
import { InlineSpinner } from '../../components/LoadingSpinner';
import type { SalesTransaction, CompleteSaleRequest, CreatePaymentRequest, PaymentMethod } from '../../types/sales';

interface CompleteSaleModalProps {
  sale: SalesTransaction;
  paymentMethods: PaymentMethod[];
  isSubmitting: boolean;
  onSubmit: (data: CompleteSaleRequest) => void;
  onCancel: () => void;
}

export const CompleteSaleModal: React.FC<CompleteSaleModalProps> = ({
  sale,
  paymentMethods,
  isSubmitting,
  onSubmit,
  onCancel,
}) => {
  const [payments, setPayments] = useState<CreatePaymentRequest[]>([
    {
      paymentMethodId: 0,
      amount: sale.totalAmount,
      reference: '',
      processedBy: '',
    },
  ]);

  const [errors, setErrors] = useState<Record<string, string>>({});

  // Obtener usuario actual
  React.useEffect(() => {
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        setPayments(prev => prev.map(p => ({
          ...p,
          processedBy: user.id || user.nombreUsuario || '',
        })));
      }
    } catch (err) {
      console.error('Error obteniendo usuario:', err);
    }
  }, []);

  const handleAddPayment = () => {
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;

    setPayments([
      ...payments,
      {
        paymentMethodId: 0,
        amount: 0,
        reference: '',
        processedBy: user?.id || user?.nombreUsuario || '',
      },
    ]);
  };

  const handleRemovePayment = (index: number) => {
    if (payments.length > 1) {
      setPayments(payments.filter((_, i) => i !== index));
    }
  };

  const handlePaymentChange = (
    index: number,
    field: keyof CreatePaymentRequest,
    value: string | number
  ) => {
    const newPayments = [...payments];
    newPayments[index] = { ...newPayments[index], [field]: value };
    setPayments(newPayments);
  };

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    const totalPayments = payments.reduce((sum, p) => sum + Number(p.amount), 0);
    if (totalPayments < sale.totalAmount) {
      newErrors.totalPayments = `El total de pagos (₡${totalPayments.toLocaleString()}) es menor al total de la venta (₡${sale.totalAmount.toLocaleString()})`;
    }

    payments.forEach((payment, index) => {
      if (!payment.paymentMethodId || payment.paymentMethodId === 0) {
        newErrors[`payment_${index}_method`] = 'Selecciona un método de pago';
      }

      if (payment.amount <= 0) {
        newErrors[`payment_${index}_amount`] = 'El monto debe ser mayor a 0';
      }

      const method = paymentMethods.find(m => m.id === payment.paymentMethodId);
      if (method?.requiresReference && !payment.reference?.trim()) {
        newErrors[`payment_${index}_reference`] = 'Este método requiere una referencia';
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      onSubmit({ payments });
    }
  };

  const totalPayments = payments.reduce((sum, p) => sum + Number(p.amount), 0);
  const remaining = sale.totalAmount - totalPayments;
  const change = totalPayments > sale.totalAmount ? totalPayments - sale.totalAmount : 0;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="mb-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Completar Venta</h2>
            <p className="text-gray-600 text-sm">Procesa los pagos para finalizar la venta</p>

            {/* Resumen de venta */}
            <div className="mt-4 p-4 bg-gray-50 rounded-lg">
              <div className="flex justify-between items-center mb-2">
                <span className="font-medium text-gray-700">Venta:</span>
                <span className="font-bold text-gray-900">{sale.transactionNumber}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="font-medium text-gray-700">Total a pagar:</span>
                <span className="text-2xl font-bold text-blue-600">₡{sale.totalAmount.toLocaleString()}</span>
              </div>
            </div>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Pagos */}
            <div>
              <div className="flex justify-between items-center mb-3">
                <label className="block text-sm font-medium text-gray-700">
                  Métodos de Pago <span className="text-red-500">*</span>
                </label>
                <button
                  type="button"
                  onClick={handleAddPayment}
                  disabled={isSubmitting}
                  className="text-sm text-blue-600 hover:text-blue-700 font-medium disabled:opacity-50"
                >
                  + Agregar Pago
                </button>
              </div>

              {errors.totalPayments && (
                <div className="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-800">{errors.totalPayments}</p>
                </div>
              )}

              <div className="space-y-3">
                {payments.map((payment, index) => (
                  <div key={index} className="p-4 bg-gray-50 rounded-lg space-y-3">
                    <div className="flex justify-between items-center">
                      <h4 className="text-sm font-semibold text-gray-700">
                        Pago {index + 1}
                      </h4>
                      {payments.length > 1 && (
                        <button
                          type="button"
                          onClick={() => handleRemovePayment(index)}
                          disabled={isSubmitting}
                          className="text-red-600 hover:text-red-700 disabled:opacity-50"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                          </svg>
                        </button>
                      )}
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                      {/* Método de pago */}
                      <div>
                        <label className="block text-xs font-medium text-gray-700 mb-1">
                          Método <span className="text-red-500">*</span>
                        </label>
                        <select
                          value={payment.paymentMethodId}
                          onChange={(e) => handlePaymentChange(index, 'paymentMethodId', parseInt(e.target.value))}
                          disabled={isSubmitting}
                          className={`w-full px-3 py-2 border rounded-lg text-sm
                                   ${errors[`payment_${index}_method`] ? 'border-red-500' : 'border-gray-300'}`}
                        >
                          <option value={0}>Selecciona...</option>
                          {paymentMethods.filter(m => m.active).map((method) => (
                            <option key={method.id} value={method.id}>
                              {method.name}
                            </option>
                          ))}
                        </select>
                        {errors[`payment_${index}_method`] && (
                          <p className="mt-1 text-xs text-red-600">{errors[`payment_${index}_method`]}</p>
                        )}
                      </div>

                      {/* Monto */}
                      <div>
                        <label className="block text-xs font-medium text-gray-700 mb-1">
                          Monto (₡) <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          min="0"
                          value={payment.amount}
                          onChange={(e) => handlePaymentChange(index, 'amount', parseFloat(e.target.value) || 0)}
                          disabled={isSubmitting}
                          className={`w-full px-3 py-2 border rounded-lg text-sm
                                   ${errors[`payment_${index}_amount`] ? 'border-red-500' : 'border-gray-300'}`}
                          placeholder="0.00"
                        />
                        {errors[`payment_${index}_amount`] && (
                          <p className="mt-1 text-xs text-red-600">{errors[`payment_${index}_amount`]}</p>
                        )}
                      </div>
                    </div>

                    {/* Referencia (si es requerida) */}
                    {paymentMethods.find(m => m.id === payment.paymentMethodId)?.requiresReference && (
                      <div>
                        <label className="block text-xs font-medium text-gray-700 mb-1">
                          Referencia <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          value={payment.reference}
                          onChange={(e) => handlePaymentChange(index, 'reference', e.target.value)}
                          disabled={isSubmitting}
                          className={`w-full px-3 py-2 border rounded-lg text-sm
                                   ${errors[`payment_${index}_reference`] ? 'border-red-500' : 'border-gray-300'}`}
                          placeholder="Número de transacción, comprobante, etc."
                        />
                        {errors[`payment_${index}_reference`] && (
                          <p className="mt-1 text-xs text-red-600">{errors[`payment_${index}_reference`]}</p>
                        )}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Resumen de pagos */}
            <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg space-y-2">
              <div className="flex justify-between text-sm">
                <span className="font-medium text-blue-900">Total Venta:</span>
                <span className="font-bold text-blue-900">₡{sale.totalAmount.toLocaleString()}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="font-medium text-blue-900">Total Pagos:</span>
                <span className="font-bold text-blue-900">₡{totalPayments.toLocaleString()}</span>
              </div>
              {remaining > 0 && (
                <div className="flex justify-between text-sm pt-2 border-t border-blue-300">
                  <span className="font-medium text-red-700">Faltante:</span>
                  <span className="font-bold text-red-700">₡{remaining.toLocaleString()}</span>
                </div>
              )}
              {change > 0 && (
                <div className="flex justify-between text-sm pt-2 border-t border-blue-300">
                  <span className="font-medium text-green-700">Cambio:</span>
                  <span className="font-bold text-green-700">₡{change.toLocaleString()}</span>
                </div>
              )}
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
                disabled={isSubmitting || remaining > 0}
                className="flex-1 px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                         flex items-center justify-center font-medium"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    Procesando...
                  </>
                ) : (
                  <>
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    Completar Venta
                  </>
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};