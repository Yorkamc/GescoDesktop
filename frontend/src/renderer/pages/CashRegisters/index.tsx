import React, { useState, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useCashRegisters } from '../../hooks/useCashRegisters';
import { CashRegisterCard } from '../../components/CashRegisterCard';
import { CashRegisterForm } from '../../components/CashRegisterForm';
import { OpenCashModal } from '../../components/OpenCashModal';
import { CloseCashModal } from '../../components/CloseCashModal';
import { Alert } from '../../components/Alert';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import type { CashRegister, CreateCashRegisterRequest, CloseCashRegisterRequest } from '../../types/cashRegister';

export const CashRegisters: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  // ✅ Obtener activityId de la URL
  const activityIdFromUrl = searchParams.get('activityId') || undefined;
  
  const {
    cashRegisters,
    openCashRegisters,
    isLoading,
    error,
    createCashRegister,
    updateCashRegister,
    deleteCashRegister,
    openCashRegister,
    closeCashRegister,
    refreshCashRegisters,
    clearError,
  } = useCashRegisters(activityIdFromUrl); // ✅ Pasar activityId al hook

  const [showForm, setShowForm] = useState(false);
  const [editingCashRegister, setEditingCashRegister] = useState<CashRegister | null>(null);
  const [openingCashRegister, setOpeningCashRegister] = useState<CashRegister | null>(null);
  const [closingCashRegister, setClosingCashRegister] = useState<CashRegister | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showOpenOnly, setShowOpenOnly] = useState(false);

  const handleEdit = (cashRegister: CashRegister) => {
    setEditingCashRegister(cashRegister);
    setShowForm(true);
  };

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`¿Estás seguro de que quieres eliminar la caja "${name}"?`)) {
      return;
    }
    await deleteCashRegister(id);
  };

  const handleOpen = (cashRegister: CashRegister) => {
    setOpeningCashRegister(cashRegister);
  };

  const handleClose = (cashRegister: CashRegister) => {
    setClosingCashRegister(cashRegister);
  };

  const handleSubmitForm = async (data: CreateCashRegisterRequest) => {
    setIsSubmitting(true);
    try {
      if (editingCashRegister) {
        const updated = await updateCashRegister(editingCashRegister.id, data);
        if (updated) {
          setShowForm(false);
          setEditingCashRegister(null);
        }
      } else {
        const created = await createCashRegister(data);
        if (created) {
          setShowForm(false);
        }
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSubmitOpen = async () => {
    if (!openingCashRegister) return;
    
    setIsSubmitting(true);
    try {
      const success = await openCashRegister(openingCashRegister.id);
      if (success) {
        setOpeningCashRegister(null);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSubmitClose = async (data: CloseCashRegisterRequest) => {
    if (!closingCashRegister) return;
    
    setIsSubmitting(true);
    try {
      const success = await closeCashRegister(closingCashRegister.id, data);
      if (success) {
        setClosingCashRegister(null);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingCashRegister(null);
    setOpeningCashRegister(null);
    setClosingCashRegister(null);
    clearError();
  };

  // ✅ Función para volver a actividades
  const handleBack = () => {
    if (activityIdFromUrl) {
      // Si vino de una actividad específica, volver a actividades
      navigate('/activities');
    } else {
      // Si no, volver al dashboard
      navigate('/dashboard');
    }
  };

  const filteredCashRegisters = useMemo(() => {
    return cashRegisters.filter(cr => {
      const matchesSearch = cr.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                           cr.location.toLowerCase().includes(searchQuery.toLowerCase()) ||
                           cr.registerNumber.toString().includes(searchQuery);
      const matchesOpen = !showOpenOnly || cr.isOpen;
      return matchesSearch && matchesOpen;
    });
  }, [cashRegisters, searchQuery, showOpenOnly]);

  const openCount = cashRegisters.filter(cr => cr.isOpen).length;
  const closedCount = cashRegisters.filter(cr => !cr.isOpen).length;

  if (isLoading) {
    return <LoadingSpinner fullScreen message="Cargando cajas..." />;
  }

  return (
    <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
      {/* Header */}
      <header className="bg-white shadow-sm border-b flex-shrink-0">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center">
              {/* ✅ Botón de regreso mejorado */}
              <button
                onClick={handleBack}
                className="p-2 text-gray-500 hover:text-gray-700 rounded-lg mr-3"
                title={activityIdFromUrl ? "Volver a Actividades" : "Volver al Dashboard"}
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div>
                <h1 className="text-xl font-bold text-gray-900">Cajas Registradoras</h1>
                <p className="text-xs text-gray-500">
                  {cashRegisters.length} cajas • {openCount} abiertas • {closedCount} cerradas
                </p>
              </div>
            </div>

            <button
              onClick={() => {
                setEditingCashRegister(null);
                setShowForm(true);
              }}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 flex items-center"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              Nueva Caja
            </button>
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
                onRetry={refreshCashRegisters}
                onDismiss={clearError}
              />
            )}

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-blue-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Total Cajas</p>
                    <p className="text-2xl font-bold text-gray-900">{cashRegisters.length}</p>
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
                    <p className="text-sm font-medium text-gray-600">Abiertas</p>
                    <p className="text-2xl font-bold text-green-600">{openCount}</p>
                  </div>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-gray-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Cerradas</p>
                    <p className="text-2xl font-bold text-gray-600">{closedCount}</p>
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
                    placeholder="Buscar por nombre, ubicación o número..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div className="flex items-end">
                  <label className="flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={showOpenOnly}
                      onChange={(e) => setShowOpenOnly(e.target.checked)}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                    />
                    <span className="ml-2 text-sm text-gray-700">Solo cajas abiertas</span>
                  </label>
                </div>
              </div>
            </div>

            {/* Cash Registers Grid */}
            {filteredCashRegisters.length === 0 ? (
              <div className="text-center py-12 bg-white rounded-lg shadow-sm">
                <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
                <h3 className="text-lg font-medium text-gray-900 mb-2">No hay cajas registradoras</h3>
                <p className="text-gray-600 mb-4">
                  {searchQuery || showOpenOnly
                    ? 'No se encontraron cajas con los filtros seleccionados'
                    : 'Comienza creando tu primera caja registradora'}
                </p>
                {!searchQuery && !showOpenOnly && (
                  <button
                    onClick={() => setShowForm(true)}
                    className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                  >
                    Crear Primera Caja
                  </button>
                )}
              </div>
            ) : (
              <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {filteredCashRegisters.map((cashRegister) => (
                  <CashRegisterCard
                    key={cashRegister.id}
                    cashRegister={cashRegister}
                    onEdit={handleEdit}
                    onDelete={handleDelete}
                    onOpen={handleOpen}
                    onClose={handleClose}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      </main>

      {/* Modals */}
      {showForm && (
        <CashRegisterForm
          cashRegister={editingCashRegister}
          preselectedActivityId={activityIdFromUrl} 
          isSubmitting={isSubmitting}
          onSubmit={handleSubmitForm}
          onCancel={handleCancel}
        />
      )}

      {openingCashRegister && (
        <OpenCashModal
          cashRegister={openingCashRegister}
          isSubmitting={isSubmitting}
          onConfirm={handleSubmitOpen}
          onCancel={handleCancel}
        />
      )}

      {closingCashRegister && (
        <CloseCashModal
          cashRegister={closingCashRegister}
          isSubmitting={isSubmitting}
          onSubmit={handleSubmitClose}
          onCancel={handleCancel}
        />
      )}
    </div>
  );
};