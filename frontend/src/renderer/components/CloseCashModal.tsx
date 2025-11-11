import React, { useState, useEffect } from 'react';
import { InlineSpinner } from './LoadingSpinner';
import { useSales } from '../hooks/useSales';
import type { CashRegister, CloseCashRegisterRequest } from '../types/cashRegister';

interface CloseCashModalProps {
  cashRegister: CashRegister;
  isSubmitting: boolean;
  onSubmit: (data: CloseCashRegisterRequest) => void;
  onCancel: () => void;
}

export const CloseCashModal: React.FC<CloseCashModalProps> = ({
  cashRegister,
  isSubmitting,
  onSubmit,
  onCancel,
}) => {
  const { sales, getSalesSummary } = useSales(cashRegister.id);
  
  const [formData, setFormData] = useState<CloseCashRegisterRequest>({
    cashDeclared: 0,
    closedBy: '',
    supervisedBy: undefined,
    observations: '',
  });

  const [loadingSummary, setLoadingSummary] = useState(true);
  const [summary, setSummary] = useState<{
    totalSales: number;
    completedTransactions: number;
    totalItemsSold: number;
    totalCash: number;
    totalCard: number;
    totalTransfers: number;
  } | null>(null);

  // ✅ Cargar resumen de ventas al montar
  useEffect(() => {
    const loadSummary = async () => {
      try {
        setLoadingSummary(true);
        const data = await getSalesSummary(cashRegister.id);
        
        // ✅ Calcular totales por método de pago desde las ventas completadas
        const completedSales = sales.filter(s => s.salesStatusId === 2); // Asumiendo que 2 = Completada
        
        let totalCash = 0;
        let totalCard = 0;
        let totalTransfers = 0;

        completedSales.forEach(sale => {
          sale.payments.forEach(payment => {
            // Asumiendo IDs de métodos de pago: 1=Efectivo, 2=Tarjeta, 3=Transferencia
            if (payment.paymentMethodId === 1) {
              totalCash += payment.amount;
            } else if (payment.paymentMethodId === 2) {
              totalCard += payment.amount;
            } else if (payment.paymentMethodId === 3) {
              totalTransfers += payment.amount;
            }
          });
        });

        setSummary({
          totalSales: data?.totalSales || 0,
          completedTransactions: data?.completedTransactions || 0,
          totalItemsSold: data?.totalItemsSold || 0,
          totalCash,
          totalCard,
          totalTransfers,
        });

        // ✅ Pre-llenar el efectivo declarado con el total de efectivo
        setFormData(prev => ({
          ...prev,
          cashDeclared: totalCash,
        }));

        console.log('📊 Cash Register Summary:', { totalCash, totalCard, totalTransfers });
      } catch (error) {
        console.error('Error loading sales summary:', error);
      } finally {
        setLoadingSummary(false);
      }
    };

    // ✅ Obtener usuario actual
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;
    const userId = user?.id || user?.nombreUsuario || '';

    setFormData(prev => ({
      ...prev,
      closedBy: userId,
      supervisedBy: userId, // ✅ Mismo usuario para supervisión
    }));

    loadSummary();
  }, [cashRegister.id, sales, getSalesSummary]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('🔒 Closing cash register with data:', formData);
    onSubmit(formData);
  };

  const difference = summary ? formData.cashDeclared - summary.totalCash : 0;
  const hasDifference = Math.abs(difference) > 0.01;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div>
              <div className="flex items-center gap-2 mb-2">
                <div className="w-10 h-10 bg-red-100 rounded-full flex items-center justify-center">
                  <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </div>
                <h2 className="text-2xl font-bold text-gray-900">Cerrar Caja Registradora</h2>
              </div>
              <p className="text-gray-600">Completa los datos del cierre de caja</p>
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

          {/* Info de la caja */}
          <div className="mb-6 p-4 bg-gray-50 rounded-lg">
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-600">Caja:</span>
                <span className="ml-2 font-semibold text-gray-900">{cashRegister.name}</span>
              </div>
              <div>
                <span className="text-gray-600">Número:</span>
                <span className="ml-2 font-semibold text-gray-900">{cashRegister.registerNumber}</span>
              </div>
            </div>
          </div>

          {/* Resumen de ventas */}
          {loadingSummary ? (
            <div className="mb-6 p-6 bg-blue-50 border border-blue-200 rounded-lg text-center">
              <InlineSpinner className="h-5 w-5 text-blue-600 mx-auto" />
              <p className="text-sm text-blue-800 mt-2">Calculando ventas...</p>
            </div>
          ) : summary ? (
            <div className="mb-6 p-4 bg-gradient-to-r from-blue-50 to-green-50 border border-blue-200 rounded-lg">
              <h3 className="text-sm font-semibold text-gray-900 mb-3 flex items-center">
                <svg className="w-5 h-5 text-blue-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                </svg>
                Resumen de Ventas Completadas
              </h3>
              <div className="grid grid-cols-2 gap-3">
                <div className="bg-white p-3 rounded-lg">
                  <p className="text-xs text-gray-600">Ventas Completadas</p>
                  <p className="text-lg font-bold text-gray-900">{summary.completedTransactions}</p>
                </div>
                <div className="bg-white p-3 rounded-lg">
                  <p className="text-xs text-gray-600">Items Vendidos</p>
                  <p className="text-lg font-bold text-gray-900">{summary.totalItemsSold}</p>
                </div>
                <div className="bg-white p-3 rounded-lg col-span-2">
                  <p className="text-xs text-gray-600">Total General</p>
                  <p className="text-2xl font-bold text-green-700">
                    ₡{summary.totalSales.toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                  </p>
                </div>
                <div className="bg-white p-3 rounded-lg">
                  <p className="text-xs text-gray-600">💵 Efectivo</p>
                  <p className="text-base font-semibold text-gray-900">
                    ₡{summary.totalCash.toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                  </p>
                </div>
                <div className="bg-white p-3 rounded-lg">
                  <p className="text-xs text-gray-600">💳 Tarjeta</p>
                  <p className="text-base font-semibold text-gray-900">
                    ₡{summary.totalCard.toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                  </p>
                </div>
                <div className="bg-white p-3 rounded-lg col-span-2">
                  <p className="text-xs text-gray-600">📱 Transferencias</p>
                  <p className="text-base font-semibold text-gray-900">
                    ₡{summary.totalTransfers.toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                  </p>
                </div>
              </div>
            </div>
          ) : null}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* ✅ Efectivo Declarado (pre-llenado automáticamente) */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Efectivo Declarado (₡) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.cashDeclared}
                onChange={(e) => setFormData({ ...formData, cashDeclared: parseFloat(e.target.value) || 0 })}
                disabled={isSubmitting || loadingSummary}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 
                         focus:border-blue-500 disabled:bg-gray-100 text-lg font-semibold"
                required
                placeholder="0.00"
                autoFocus
              />
              <p className="text-xs text-gray-500 mt-1">
                Monto total en efectivo contado en la caja (pre-llenado con las ventas en efectivo)
              </p>
            </div>

            {/* ✅ Mostrar diferencia si existe */}
            {summary && hasDifference && (
              <div className={`p-4 rounded-lg border ${
                difference > 0 
                  ? 'bg-blue-50 border-blue-200' 
                  : 'bg-red-50 border-red-200'
              }`}>
                <div className="flex items-start">
                  <svg 
                    className={`w-5 h-5 mr-2 flex-shrink-0 mt-0.5 ${
                      difference > 0 ? 'text-blue-600' : 'text-red-600'
                    }`} 
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <div>
                    <p className={`text-sm font-semibold ${
                      difference > 0 ? 'text-blue-900' : 'text-red-900'
                    }`}>
                      {difference > 0 ? '⬆️ Sobrante' : '⬇️ Faltante'} de efectivo
                    </p>
                    <p className={`text-lg font-bold ${
                      difference > 0 ? 'text-blue-800' : 'text-red-800'
                    }`}>
                      {difference > 0 ? '+' : ''}₡{difference.toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                    </p>
                    <p className="text-xs text-gray-600 mt-1">
                      Esperado: ₡{summary.totalCash.toLocaleString('es-CR', { minimumFractionDigits: 2 })} | 
                      Declarado: ₡{formData.cashDeclared.toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* ✅ Sin diferencia - mostrar confirmación verde */}
            {summary && !hasDifference && formData.cashDeclared > 0 && (
              <div className="p-4 rounded-lg border bg-green-50 border-green-200">
                <div className="flex items-center">
                  <svg className="w-5 h-5 text-green-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <div>
                    <p className="text-sm font-semibold text-green-900">✓ Efectivo cuadrado</p>
                    <p className="text-xs text-green-700">El efectivo declarado coincide con las ventas</p>
                  </div>
                </div>
              </div>
            )}

            {/* ✅ Observaciones (Opcional) */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Observaciones (Opcional)
              </label>
              <textarea
                value={formData.observations}
                onChange={(e) => setFormData({ ...formData, observations: e.target.value })}
                disabled={isSubmitting}
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                         focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
                placeholder="Notas adicionales sobre el cierre (ej: razón de diferencias, eventos especiales, etc.)"
              />
            </div>

            {/* Alert */}
            <div className="p-3 bg-yellow-50 border border-yellow-200 rounded-lg">
              <div className="flex">
                <svg className="h-5 w-5 text-yellow-400 mr-2 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                <p className="text-xs text-yellow-800">
                  Una vez cerrada la caja, no podrás realizar más transacciones hasta que la vuelvas a abrir.
                  {hasDifference && ' Se registrará la diferencia en el cierre.'}
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
                disabled={isSubmitting || loadingSummary}
                className="flex-1 px-6 py-3 bg-red-600 text-white rounded-lg hover:bg-red-700 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                         flex items-center justify-center font-medium"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    Cerrando...
                  </>
                ) : (
                  <>
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                    Cerrar Caja
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