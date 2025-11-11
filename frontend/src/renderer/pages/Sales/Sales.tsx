import React, { useState, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useSales } from '../../hooks/useSales';
import { useCashRegisters } from '../../hooks/useCashRegisters';
import { SaleCard } from './SaleCard';
import { NewSaleModal } from './NewSaleModal';
import { CompleteSaleModal } from './CompleteSaleModal';
import { Alert } from '../../components/Alert';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import type { SalesTransaction, CompleteSaleRequest } from '../../types/sales';

export const Sales: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  // ✅ Obtener ambos parámetros de la URL
  const cashRegisterId = searchParams.get('cashRegisterId') || undefined;
  const activityIdFromUrl = searchParams.get('activityId') || undefined;
  
  const {
    sales,
    paymentMethods,
    isLoading,
    error,
    cancelSale,
    completeSale,
    refreshSales,
    clearError,
  } = useSales(cashRegisterId);

  // ✅ Cargar información de la caja si viene de una
  const { cashRegisters } = useCashRegisters(activityIdFromUrl);
  const selectedCashRegister = cashRegisters.find(cr => cr.id === cashRegisterId);

  const [showNewSaleModal, setShowNewSaleModal] = useState(false);
  const [completingSale, setCompletingSale] = useState<SalesTransaction | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');

  const handleCancel = async (id: string, transactionNumber: string) => {
    if (!confirm(`¿Estás seguro de que quieres cancelar la venta ${transactionNumber}?`)) {
      return;
    }

    const reason = prompt('Motivo de cancelación (opcional):');
    await cancelSale(id, reason || undefined);
  };

  const handleComplete = (sale: SalesTransaction) => {
    setCompletingSale(sale);
  };

  const handleSubmitComplete = async (data: CompleteSaleRequest) => {
    if (!completingSale) return;
    
    setIsSubmitting(true);
    try {
      const completed = await completeSale(completingSale.id, data);
      if (completed) {
        setCompletingSale(null);
        await refreshSales(); // ✅ Refrescar después de completar
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  // ✅ Nueva función para manejar el éxito de crear venta
  const handleSaleCreated = async () => {
    setShowNewSaleModal(false);
    await refreshSales(); // ✅ Refrescar lista
  };

  const handleCloseModals = () => {
    setShowNewSaleModal(false);
    setCompletingSale(null);
    clearError();
  };

  // ✅ Volver a cajas manteniendo el activityId
  const handleBack = () => {
    if (activityIdFromUrl) {
      navigate(`/cash-registers?activityId=${activityIdFromUrl}`);
    } else {
      navigate('/cash-registers');
    }
  };


  const filteredSales = useMemo(() => {
    return sales.filter(sale => {
      const matchesSearch = sale.transactionNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
                           sale.invoiceNumber?.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesStatus = statusFilter === 'all' || sale.statusName === statusFilter;
      return matchesSearch && matchesStatus;
    });
  }, [sales, searchQuery, statusFilter]);

  const pendingSales = sales.filter(s => s.statusName === 'Pending').length;
  const completedSales = sales.filter(s => s.statusName === 'Completed').length;
  const totalRevenue = sales
    .filter(s => s.statusName === 'Completed')
    .reduce((sum, s) => sum + s.totalAmount, 0);

  if (isLoading) {
    return <LoadingSpinner fullScreen message="Cargando ventas..." />;
  }

  return (
    <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
      {/* Header */}
      <header className="bg-white shadow-sm border-b flex-shrink-0">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center">
              <button
                onClick={handleBack}
                className="p-2 text-gray-500 hover:text-gray-700 rounded-lg mr-3"
                title="Volver a Cajas"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div>
                <h1 className="text-xl font-bold text-gray-900">
                  Ventas
                  {/* ✅ Mostrar nombre de la caja si existe */}
                  {selectedCashRegister && (
                    <span className="text-sm font-normal text-gray-600 ml-2">
                      - {selectedCashRegister.name}
                    </span>
                  )}
                </h1>
                <p className="text-xs text-gray-500">
                  {sales.length} ventas • {pendingSales} pendientes • {completedSales} completadas
                </p>
              </div>
            </div>

            <div className="flex gap-2">
              <button
                onClick={() => setShowNewSaleModal(true)}
                className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 flex items-center"
              >
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                Nueva Venta
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto">
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            {error && (
              <Alert
                type="error"
                message={error}
                onRetry={refreshSales}
                onDismiss={clearError}
              />
            )}

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-blue-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Total Ventas</p>
                    <p className="text-2xl font-bold text-gray-900">{sales.length}</p>
                  </div>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-yellow-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Pendientes</p>
                    <p className="text-2xl font-bold text-yellow-600">{pendingSales}</p>
                  </div>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-green-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Completadas</p>
                    <p className="text-2xl font-bold text-green-600">{completedSales}</p>
                  </div>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-indigo-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Ingresos</p>
                    <p className="text-2xl font-bold text-indigo-600">₡{totalRevenue.toLocaleString()}</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Filters */}
            <div className="bg-white rounded-lg shadow-sm border p-4 mb-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Buscar</label>
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="Buscar por número de transacción o factura..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
                  <select
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="all">Todos los estados</option>
                    <option value="Pending">Pendientes</option>
                    <option value="Completed">Completadas</option>
                    <option value="Cancelled">Canceladas</option>
                  </select>
                </div>
              </div>
            </div>

            {/* Sales Grid */}
            {filteredSales.length === 0 ? (
              <div className="text-center py-12 bg-white rounded-lg shadow-sm">
                <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
                <h3 className="text-lg font-medium text-gray-900 mb-2">No hay ventas</h3>
                <p className="text-gray-600 mb-4">
                  {searchQuery || statusFilter !== 'all'
                    ? 'No se encontraron ventas con los filtros seleccionados'
                    : 'Comienza creando tu primera venta'}
                </p>
                {!searchQuery && statusFilter === 'all' && (
                  <button
                    onClick={() => setShowNewSaleModal(true)}
                    className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                  >
                    Crear Primera Venta
                  </button>
                )}
              </div>
            ) : (
              <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {filteredSales.map((sale) => (
                  <SaleCard
                    key={sale.id}
                    sale={sale}
                    onCancel={handleCancel}
                    onComplete={handleComplete}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      </main>

      {/* Modals */}
      {showNewSaleModal && (
        <NewSaleModal
          cashRegisterId={cashRegisterId}
          onClose={handleCloseModals}
          onSuccess={handleSaleCreated} // ✅ Pasar callback de éxito
        />
      )}

      {completingSale && (
        <CompleteSaleModal
          sale={completingSale}
          paymentMethods={paymentMethods}
          isSubmitting={isSubmitting}
          onSubmit={handleSubmitComplete}
          onCancel={handleCloseModals}
        />
      )}
    </div>
  );
};