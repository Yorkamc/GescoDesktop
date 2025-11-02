import React, { useState, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useCombos } from '../../hooks/useCombos';
import { ComboCard } from './ComboCard';
import { ComboForm } from './ComboForm';
import { Alert } from '../../components/Alert';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import type { SalesCombo, CreateComboRequest } from '../../types/combo';

export const Combos: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  const activityId = searchParams.get('activityId') || undefined;
  
  const {
    combos,
    isLoading,
    error,
    createCombo,
    updateCombo,
    deleteCombo,
    toggleComboActive,
    refreshCombos,
    clearError,
  } = useCombos(activityId);

  const [showForm, setShowForm] = useState(false);
  const [editingCombo, setEditingCombo] = useState<SalesCombo | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showActiveOnly, setShowActiveOnly] = useState(false);

  const handleEdit = (combo: SalesCombo) => {
    setEditingCombo(combo);
    setShowForm(true);
  };

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`¿Estás seguro de que quieres eliminar el combo "${name}"?`)) {
      return;
    }
    await deleteCombo(id);
  };

  const handleToggleActive = async (id: string) => {
    await toggleComboActive(id);
  };

  const handleSubmitForm = async (data: CreateComboRequest) => {
    setIsSubmitting(true);
    try {
      if (editingCombo) {
        const updated = await updateCombo(editingCombo.id, data);
        if (updated) {
          setShowForm(false);
          setEditingCombo(null);
        }
      } else {
        const created = await createCombo(data);
        if (created) {
          setShowForm(false);
        }
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingCombo(null);
    clearError();
  };

  const handleBack = () => {

      navigate('/cash-registers');
  
  };

  const filteredCombos = useMemo(() => {
    return combos.filter(combo => {
      const matchesSearch = combo.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                           combo.description?.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesActive = !showActiveOnly || combo.active;
      return matchesSearch && matchesActive;
    });
  }, [combos, searchQuery, showActiveOnly]);

  const activeCount = combos.filter(c => c.active).length;
  const inactiveCount = combos.filter(c => !c.active).length;
  const totalSavings = combos
    .filter(c => c.active)
    .reduce((sum, c) => {
      const regularPrice = c.items.reduce((total, item) => total + (item.productPrice * item.quantity), 0);
      return sum + (regularPrice - c.comboPrice);
    }, 0);

  if (isLoading) {
    return <LoadingSpinner fullScreen message="Cargando combos..." />;
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
                title={activityId ? "Volver a Actividades" : "Volver a Ventas"}
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div>
                <h1 className="text-xl font-bold text-gray-900">Combos de Ventas</h1>
                <p className="text-xs text-gray-500">
                  {combos.length} combos • {activeCount} activos • {inactiveCount} inactivos
                </p>
              </div>
            </div>

            <button
              onClick={() => {
                setEditingCombo(null);
                setShowForm(true);
              }}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 flex items-center"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              Nuevo Combo
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
                onRetry={refreshCombos}
                onDismiss={clearError}
              />
            )}

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-purple-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Total Combos</p>
                    <p className="text-2xl font-bold text-gray-900">{combos.length}</p>
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
                    <p className="text-sm font-medium text-gray-600">Activos</p>
                    <p className="text-2xl font-bold text-green-600">{activeCount}</p>
                  </div>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow-sm border p-4">
                <div className="flex items-center">
                  <div className="flex-shrink-0 bg-blue-100 rounded-lg p-3">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                  <div className="ml-4">
                    <p className="text-sm font-medium text-gray-600">Ahorro Total</p>
                    <p className="text-2xl font-bold text-blue-600">₡{totalSavings.toLocaleString()}</p>
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
                    placeholder="Buscar por nombre o descripción..."
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div className="flex items-end">
                  <label className="flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      checked={showActiveOnly}
                      onChange={(e) => setShowActiveOnly(e.target.checked)}
                      className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                    />
                    <span className="ml-2 text-sm text-gray-700">Solo combos activos</span>
                  </label>
                </div>
              </div>
            </div>

            {/* Combos Grid */}
            {filteredCombos.length === 0 ? (
              <div className="text-center py-12 bg-white rounded-lg shadow-sm">
                <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
                <h3 className="text-lg font-medium text-gray-900 mb-2">No hay combos</h3>
                <p className="text-gray-600 mb-4">
                  {searchQuery || showActiveOnly
                    ? 'No se encontraron combos con los filtros seleccionados'
                    : 'Comienza creando tu primer combo'}
                </p>
                {!searchQuery && !showActiveOnly && (
                  <button
                    onClick={() => setShowForm(true)}
                    className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                  >
                    Crear Primer Combo
                  </button>
                )}
              </div>
            ) : (
              <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {filteredCombos.map((combo) => (
                  <ComboCard
                    key={combo.id}
                    combo={combo}
                    onEdit={handleEdit}
                    onDelete={handleDelete}
                    onToggleActive={handleToggleActive}
                  />
                ))}
              </div>
            )}
          </div>
        </div>
      </main>

      {/* Modal */}
      {showForm && (
        <ComboForm
          combo={editingCombo}
          preselectedActivityId={activityId}
          isSubmitting={isSubmitting}
          onSubmit={handleSubmitForm}
          onCancel={handleCancel}
        />
      )}
    </div>
  );
};