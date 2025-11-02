import React from 'react';
import type { SalesTransaction } from '../../types/sales';

interface SaleCardProps {
  sale: SalesTransaction;
  onCancel: (id: string, transactionNumber: string) => void;
  onComplete: (sale: SalesTransaction) => void;
}

export const SaleCard: React.FC<SaleCardProps> = ({
  sale,
  onCancel,
  onComplete,
}) => {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status?: string) => {
    switch (status) {
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'Completed':
        return 'bg-green-100 text-green-800';
      case 'Cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusLabel = (status?: string) => {
    switch (status) {
      case 'Pending':
        return 'Pendiente';
      case 'Completed':
        return 'Completada';
      case 'Cancelled':
        return 'Cancelada';
      default:
        return status || 'Desconocido';
    }
  };

  const isPending = sale.statusName === 'Pending';
  const isCompleted = sale.statusName === 'Completed';

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            {sale.transactionNumber}
          </h3>
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(sale.statusName)}`}>
            {getStatusLabel(sale.statusName)}
          </span>
        </div>
      </div>

      {/* Info */}
      <div className="space-y-2 mb-4 text-sm text-gray-600">
        {/* Fecha */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span className="font-medium">Fecha:</span>
          <span className="ml-1 text-xs">{formatDate(sale.transactionDate)}</span>
        </div>

        {/* Total */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span className="font-medium">Total:</span>
          <span className="ml-1 font-bold text-green-600">₡{sale.totalAmount.toLocaleString()}</span>
        </div>

        {/* Productos */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
          </svg>
          <span className="font-medium">Productos:</span>
          <span className="ml-1">{sale.details.length}</span>
        </div>

        {/* Factura (si existe) */}
        {sale.invoiceNumber && (
          <div className="flex items-center">
            <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <span className="font-medium">Factura:</span>
            <span className="ml-1">{sale.invoiceNumber}</span>
          </div>
        )}
      </div>

      {/* Detalles de productos */}
      <div className="mb-4 p-3 bg-gray-50 rounded-lg">
        <h4 className="text-xs font-semibold text-gray-700 mb-2">Detalle:</h4>
        <div className="space-y-1">
          {sale.details.slice(0, 3).map((detail) => (
            <div key={detail.id} className="flex justify-between text-xs text-gray-600">
              <span className="truncate flex-1">
                {detail.quantity}x {detail.productName || detail.comboName}
              </span>
              <span className="font-medium ml-2">₡{detail.totalAmount.toLocaleString()}</span>
            </div>
          ))}
          {sale.details.length > 3 && (
            <p className="text-xs text-gray-500 italic">
              +{sale.details.length - 3} productos más...
            </p>
          )}
        </div>
      </div>

      {/* Botones */}
      <div className="grid grid-cols-2 gap-2">
        {isPending && (
          <>
            <button
              onClick={() => onComplete(sale)}
              className="bg-green-50 text-green-700 px-3 py-2 rounded-lg hover:bg-green-100 
                       transition-colors flex items-center justify-center gap-2 text-sm font-medium"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Completar
            </button>

            <button
              onClick={() => onCancel(sale.id, sale.transactionNumber)}
              className="bg-red-50 text-red-700 px-3 py-2 rounded-lg hover:bg-red-100 
                       transition-colors flex items-center justify-center gap-2 text-sm font-medium"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
              Cancelar
            </button>
          </>
        )}

        {isCompleted && sale.payments.length > 0 && (
          <div className="col-span-2 p-3 bg-green-50 rounded-lg">
            <h4 className="text-xs font-semibold text-green-700 mb-2">Pagos:</h4>
            <div className="space-y-1">
              {sale.payments.map((payment) => (
                <div key={payment.id} className="flex justify-between text-xs text-green-700">
                  <span>{payment.paymentMethodName}</span>
                  <span className="font-medium">₡{payment.amount.toLocaleString()}</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};