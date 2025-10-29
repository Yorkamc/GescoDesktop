import React, { useState, useMemo, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useActivities } from '../../hooks/useActivities';
import { activityCategoriesService } from '../../services/categoryApi';
import { ActivityCard } from '../../components/ActivityCard';
import { ActivityForm } from './ActivityForm';
import { ActivityCategoryManager } from '../../components/ActivityCategoryManager';
import { Alert } from '../../components/Alert';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import type { Activity, CreateActivityRequest } from '../../types';

export const Activities: React.FC = () => {
  const navigate = useNavigate();
  const {
    activities,
    isLoading,
    error,
    createActivity,
    updateActivity,
    deleteActivity,
    refreshActivities,
    clearError,
  } = useActivities();

  const [showForm, setShowForm] = useState(false);
  const [editingActivity, setEditingActivity] = useState<Activity | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  
  // Gestión de categorías
  const [managingActivity, setManagingActivity] = useState<Activity | null>(null);
  const [categoryCounts, setCategoryCounts] = useState<Record<string, number>>({});

  // Cargar conteo de categorías para cada actividad
  useEffect(() => {
    const loadCategoryCounts = async () => {
      const counts: Record<string, number> = {};
      
      for (const activity of activities) {
        try {
          const categories = await activityCategoriesService.getActivityCategories(activity.id);
          counts[activity.id] = categories.length;
        } catch (err) {
          console.error(`Error loading categories for activity ${activity.id}:`, err);
          counts[activity.id] = 0;
        }
      }
      
      setCategoryCounts(counts);
    };

    if (activities.length > 0) {
      loadCategoryCounts();
    }
  }, [activities]);

  const handleEdit = (activity: Activity) => {
    setEditingActivity(activity);
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Estás seguro de que quieres eliminar esta actividad?')) {
      return;
    }

    try {
      await deleteActivity(id);
    } catch (err) {
      console.error('Error deleting activity:', err);
    }
  };

  const handleSubmit = async (data: CreateActivityRequest) => {
    setIsSubmitting(true);
    try {
      if (editingActivity) {
        await updateActivity(editingActivity.id, data);
      } else {
        await createActivity(data);
      }
      handleCancel();
    } catch (err) {
      console.error('Error submitting activity:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingActivity(null);
  };

  const handleManageCategories = (activity: Activity) => {
    setManagingActivity(activity);
  };

  const handleCloseCategoryManager = () => {
    setManagingActivity(null);
    // Recargar conteo de categorías
    refreshActivities();
  };

  const filteredActivities = useMemo(() => {
    return activities.filter(activity => {
      const matchesSearch = !searchQuery || 
        activity.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        activity.description?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        activity.location?.toLowerCase().includes(searchQuery.toLowerCase());
      
      const matchesStatus = statusFilter === 'all' || 
        activity.activityStatusId === parseInt(statusFilter);
      
      return matchesSearch && matchesStatus;
    });
  }, [activities, searchQuery, statusFilter]);

  const stats = useMemo(() => {
    return {
      total: activities.length,
      notStarted: activities.filter(a => a.activityStatusId === 1).length,
      inProgress: activities.filter(a => a.activityStatusId === 2).length,
      completed: activities.filter(a => a.activityStatusId === 3).length,
    };
  }, [activities]);

  if (isLoading && activities.length === 0) {
    return (
      <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
        <header className="bg-white shadow-sm border-b flex-shrink-0">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex justify-between items-center py-4">
              <div className="flex items-center">
                <button
                  onClick={() => navigate('/dashboard')}
                  className="p-2 text-gray-500 hover:text-gray-700 rounded-lg mr-3"
                  title="Volver al Dashboard"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                  </svg>
                </button>
                <div>
                  <h1 className="text-xl font-bold text-gray-900">Actividades</h1>
                  <p className="text-xs text-gray-500">Cargando...</p>
                </div>
              </div>
            </div>
          </div>
        </header>
        <div className="flex-1 flex justify-center items-center">
          <LoadingSpinner size="large" />
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
      {/* Header con botón de regreso */}
      <header className="bg-white shadow-sm border-b flex-shrink-0">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center">
              <button
                onClick={() => navigate('/dashboard')}
                className="p-2 text-gray-500 hover:text-gray-700 rounded-lg mr-3"
                title="Volver al Dashboard"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div>
                <h1 className="text-xl font-bold text-gray-900">Actividades</h1>
                <p className="text-xs text-gray-500">{stats.total} actividades registradas</p>
              </div>
            </div>
            <button
              onClick={() => setShowForm(true)}
              className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 
                       transition-colors duration-200 flex items-center gap-2"
            >
              <svg
                className="w-5 h-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4v16m8-8H4"
                />
              </svg>
              Nueva Actividad
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto">
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
              <div className="bg-white p-4 rounded-lg border border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Total</p>
                    <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
                  </div>
                  <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                  </div>
                </div>
              </div>

              <div className="bg-white p-4 rounded-lg border border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Sin Iniciar</p>
                    <p className="text-2xl font-bold text-gray-900">{stats.notStarted}</p>
                  </div>
                  <div className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center">
                    <svg className="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                </div>
              </div>

              <div className="bg-white p-4 rounded-lg border border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">En Progreso</p>
                    <p className="text-2xl font-bold text-blue-600">{stats.inProgress}</p>
                  </div>
                  <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                    <svg className="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  </div>
                </div>
              </div>

              <div className="bg-white p-4 rounded-lg border border-gray-200">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">Completadas</p>
                    <p className="text-2xl font-bold text-green-600">{stats.completed}</p>
                  </div>
                  <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                    <svg className="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </div>
                </div>
              </div>
            </div>

            {error && (
              <Alert
                type="error"
                message={error}
                onDismiss={clearError}
              />
            )}

            {/* Filters */}
            <div className="mb-6 bg-white rounded-lg shadow p-6">
              <div className="flex flex-col md:flex-row gap-4">
                <div className="flex-1">
                  <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-2">
                    Buscar actividades
                  </label>
                  <input
                    id="search"
                    type="text"
                    placeholder="Buscar por nombre, descripción o ubicación..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                             focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                <div className="w-full md:w-64">
                  <label htmlFor="status" className="block text-sm font-medium text-gray-700 mb-2">
                    Filtrar por estado
                  </label>
                  <select
                    id="status"
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                             focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="all">Todos los estados</option>
                    <option value="1">Sin Iniciar</option>
                    <option value="2">En Progreso</option>
                    <option value="3">Completadas</option>
                    <option value="4">Canceladas</option>
                  </select>
                </div>
              </div>

              <div className="mt-4 flex items-center justify-between text-sm text-gray-600">
                <span>
                  Mostrando {filteredActivities.length} de {activities.length} actividades
                </span>
                <button
                  onClick={refreshActivities}
                  className="text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1"
                >
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                    />
                  </svg>
                  Actualizar
                </button>
              </div>
            </div>

            {/* Activities Grid */}
            <div className="bg-white rounded-lg shadow">
              <div className="p-6">
                {filteredActivities.length === 0 ? (
                  <div className="text-center py-12">
                    <svg
                      className="mx-auto h-12 w-12 text-gray-400 mb-4"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                      />
                    </svg>
                    <h3 className="text-lg font-medium text-gray-900 mb-2">
                      {searchQuery || statusFilter !== 'all'
                        ? 'No se encontraron actividades'
                        : 'No hay actividades'}
                    </h3>
                    <p className="text-gray-600 mb-4">
                      {searchQuery || statusFilter !== 'all'
                        ? 'Intenta ajustar los filtros de búsqueda'
                        : 'Comienza creando tu primera actividad'}
                    </p>
                    {!searchQuery && statusFilter === 'all' && (
                      <button
                        onClick={() => setShowForm(true)}
                        className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                      >
                        Crear Primera Actividad
                      </button>
                    )}
                  </div>
                ) : (
                  <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {filteredActivities.map((activity) => (
                      <ActivityCard
                        key={activity.id}
                        activity={activity}
                        onEdit={handleEdit}
                        onDelete={handleDelete}
                        onManageCategories={handleManageCategories}
                        categoryCount={categoryCounts[activity.id] || 0}
                      />
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </main>

      {/* Activity Form Modal */}
      {showForm && (
        <ActivityForm
          activity={editingActivity}
          isSubmitting={isSubmitting}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
        />
      )}

      {/* Category Manager Modal */}
      {managingActivity && (
        <ActivityCategoryManager
          activityId={managingActivity.id}
          activityName={managingActivity.name}
          onClose={handleCloseCategoryManager}
        />
      )}
    </div>
  );
};