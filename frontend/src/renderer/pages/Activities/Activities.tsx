import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { activitiesService } from '../../services/api';

interface Activity {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
  location?: string;
  activityStatusId: number;
  statusName?: string;
  managerUserId?: string;
  managerName?: string;
  organizationId?: string;
  organizationName?: string;
  createdAt: string;
}

interface CreateActivityRequest {
  name: string;
  description?: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
  location?: string;
  activityStatusId?: number;
  managerUserId?: string;
  organizationId?: string;
}

export const Activities: React.FC = () => {
  const navigate = useNavigate();
  const [activities, setActivities] = useState<Activity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [editingActivity, setEditingActivity] = useState<Activity | null>(null);
  const [showForm, setShowForm] = useState(false);

  // Form state
  const [formData, setFormData] = useState<CreateActivityRequest>({
    name: '',
    description: '',
    startDate: '',
    startTime: '',
    endDate: '',
    endTime: '',
    location: '',
    activityStatusId: 1,
    managerUserId: '',
    organizationId: ''
  });

  useEffect(() => {
    loadActivities();
  }, []);

  const loadActivities = async () => {
    try {
      setLoading(true);
      const data = await activitiesService.getActivities();
      setActivities(data);
      setError('');
    } catch (err: any) {
      setError(err.message);
      console.error('Error loading activities:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateActivity = async (e: React.FormEvent) => {
   e.preventDefault();
    try {
      setIsCreating(true);
      
      // Validaciones del frontend
      if (!formData.name.trim()) {
        setError('El nombre de la actividad es requerido');
        return;
      }
      
      if (!formData.startDate) {
        setError('La fecha de inicio es requerida');
        return;
      }

      // Asegurar que las fechas estén en formato correcto
      const activityData: CreateActivityRequest = {
        name: formData.name.trim(),
        description: formData.description?.trim() || '',
        startDate: formData.startDate, // Formato YYYY-MM-DD
        startTime: formData.startTime || undefined,
        endDate: formData.endDate || undefined,
        endTime: formData.endTime || undefined,
        location: formData.location?.trim() || '',
        activityStatusId: formData.activityStatusId || 1
      };

      console.log('📝 Creando actividad con datos:', activityData);

      const newActivity = await activitiesService.createActivity(activityData);
      setActivities([newActivity, ...activities]);
      setShowForm(false);
      resetForm();
      setError('');
    } catch (err: any) {
      console.error('Error creating activity:', err);
      setError(err.message);
    } finally {
      setIsCreating(false);
    }
  };

  const handleUpdateActivity = async (e: React.FormEvent) => {
   e.preventDefault();
    if (!editingActivity) return;

    try {
      setIsCreating(true);
      
      // Validaciones del frontend
      if (!formData.name.trim()) {
        setError('El nombre de la actividad es requerido');
        return;
      }
      
      if (!formData.startDate) {
        setError('La fecha de inicio es requerida');
        return;
      }

      const activityData: CreateActivityRequest = {
        name: formData.name.trim(),
        description: formData.description?.trim() || '',
        startDate: formData.startDate,
        startTime: formData.startTime || undefined,
        endDate: formData.endDate || undefined,
        endTime: formData.endTime || undefined,
        location: formData.location?.trim() || '',
        activityStatusId: formData.activityStatusId || 1
      };

      console.log('📝 Actualizando actividad con datos:', activityData);

      const updatedActivity = await activitiesService.updateActivity(editingActivity.id, activityData);
      setActivities(activities.map(a => a.id === editingActivity.id ? updatedActivity : a));
      setShowForm(false);
      setEditingActivity(null);
      resetForm();
      setError('');
    } catch (err: any) {
      console.error('Error updating activity:', err);
      setError(err.message);
    } finally {
      setIsCreating(false);
    }
  };

  const handleDeleteActivity = async (id: string) => {
    if (!confirm('¿Estás seguro de que quieres eliminar esta actividad?')) {
      return;
    }

    try {
      await activitiesService.deleteActivity(id);
      setActivities(activities.filter(a => a.id !== id));
      setError('');
    } catch (err: any) {
      setError(err.message);
    }
  };

  const handleEditActivity = (activity: Activity) => {
  setEditingActivity(activity);
    
    // Extraer solo la parte de fecha (YYYY-MM-DD) del string de fecha
    const extractDate = (dateString: string) => {
      if (!dateString) return '';
      // Si ya está en formato YYYY-MM-DD, devolverlo tal como está
      if (dateString.includes('T')) {
        return dateString.split('T')[0];
      }
      // Si es solo fecha, devolverla tal como está
      return dateString;
    };

    setFormData({
      name: activity.name,
      description: activity.description || '',
      startDate: extractDate(activity.startDate),
      startTime: activity.startTime || '',
      endDate: activity.endDate ? extractDate(activity.endDate) : '',
      endTime: activity.endTime || '',
      location: activity.location || '',
      activityStatusId: activity.activityStatusId,
      managerUserId: activity.managerUserId || '',
      organizationId: activity.organizationId || ''
    });
    setShowForm(true);
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      startDate: '',
      startTime: '',
      endDate: '',
      endTime: '',
      location: '',
      activityStatusId: 1,
      managerUserId: '',
      organizationId: ''
    });
    setEditingActivity(null);
  };

  const formatDate = (dateString: string) => {
 if (!dateString) return 'Sin fecha';
    
    try {
      // Intentar parsear la fecha
      const date = new Date(dateString);
      
      // Verificar si la fecha es válida
      if (isNaN(date.getTime())) {
        return 'Fecha inválida';
      }
      
      return date.toLocaleDateString('es-ES', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Fecha inválida';
    }
  };
   const formatTime = (timeString?: string) => {
    if (!timeString) return '';
    
    try {
      // Si ya está en formato HH:MM, devolverlo tal como está
      if (timeString.match(/^\d{2}:\d{2}$/)) {
        return timeString;
      }
      
      // Si viene con segundos, extraer solo HH:MM
      if (timeString.match(/^\d{2}:\d{2}:\d{2}$/)) {
        return timeString.substring(0, 5);
      }
      
      return timeString;
    } catch (error) {
      console.error('Error formatting time:', error);
      return '';
    }
  };

  const getStatusColor = (statusName?: string) => {
    switch (statusName?.toLowerCase()) {
      case 'not started':
        return 'bg-gray-100 text-gray-800';
      case 'in progress':
        return 'bg-blue-100 text-blue-800';
      case 'completed':
        return 'bg-green-100 text-green-800';
      case 'cancelled':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
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
                <h1 className="text-xl font-bold text-gray-900">Gestión de Actividades</h1>
                <p className="text-xs text-gray-500">{activities.length} actividades registradas</p>
              </div>
            </div>

            <button
              onClick={() => {
                resetForm();
                setShowForm(true);
              }}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 flex items-center"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              Nueva Actividad
            </button>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          {/* Error Message */}
          {error && (
            <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-center">
              <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              {error}
              <button 
                onClick={loadActivities}
                className="ml-auto text-red-600 hover:text-red-800 underline"
              >
                Reintentar
              </button>
            </div>
          )}

          {/* Activity Form Modal */}
          {showForm && (
            <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
              <div className="relative top-20 mx-auto p-5 border w-11/12 md:w-3/4 lg:w-1/2 shadow-lg rounded-md bg-white">
                <div className="mt-3">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-medium text-gray-900">
                      {editingActivity ? 'Editar Actividad' : 'Nueva Actividad'}
                    </h3>
                    <button
                      onClick={() => {
                        setShowForm(false);
                        setEditingActivity(null);
                        resetForm();
                      }}
                      className="text-gray-400 hover:text-gray-600"
                    >
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <form onSubmit={editingActivity ? handleUpdateActivity : handleCreateActivity} className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Nombre de la Actividad *
                        </label>
                        <input
                          type="text"
                          value={formData.name}
                          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                          required
                          placeholder="Ej: Festival de Verano 2024"
                        />
                      </div>

                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Descripción
                        </label>
                        <textarea
                          value={formData.description}
                          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                          rows={3}
                          placeholder="Descripción detallada de la actividad..."
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Fecha de Inicio *
                        </label>
                        <input
                          type="date"
                          value={formData.startDate}
                          onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                          required
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Hora de Inicio
                        </label>
                        <input
                          type="time"
                          value={formData.startTime}
                          onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Fecha de Fin
                        </label>
                        <input
                          type="date"
                          value={formData.endDate}
                          onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Hora de Fin
                        </label>
                        <input
                          type="time"
                          value={formData.endTime}
                          onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        />
                      </div>

                      <div className="md:col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Ubicación
                        </label>
                        <input
                          type="text"
                          value={formData.location}
                          onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                          placeholder="Ej: Parque Central, San José"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Estado
                        </label>
                        <select
                          value={formData.activityStatusId}
                          onChange={(e) => setFormData({ ...formData, activityStatusId: parseInt(e.target.value) })}
                          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        >
                          <option value={1}>No Iniciada</option>
                          <option value={2}>En Progreso</option>
                          <option value={3}>Completada</option>
                          <option value={4}>Cancelada</option>
                        </select>
                      </div>
                    </div>

                    <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200">
                      <button
                        type="button"
                        onClick={() => {
                          setShowForm(false);
                          setEditingActivity(null);
                          resetForm();
                        }}
                        className="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md transition-colors"
                      >
                        Cancelar
                      </button>
                      <button
                        type="submit"
                        disabled={isCreating}
                        className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
                      >
                        {isCreating ? (
                          <>
                            <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                            </svg>
                            {editingActivity ? 'Actualizando...' : 'Creando...'}
                          </>
                        ) : (
                          editingActivity ? 'Actualizar' : 'Crear Actividad'
                        )}
                      </button>
                    </div>
                  </form>
                </div>
              </div>
            </div>
          )}

          {/* Activities List */}
          {loading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto mb-4"></div>
              <p className="text-gray-600">Cargando actividades...</p>
            </div>
          ) : activities.length === 0 ? (
            <div className="text-center py-12">
              <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <h3 className="text-lg font-medium text-gray-900 mb-2">No hay actividades</h3>
              <p className="text-gray-600 mb-4">Comienza creando tu primera actividad</p>
              <button
                onClick={() => {
                  resetForm();
                  setShowForm(true);
                }}
                className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
              >
                Crear Primera Actividad
              </button>
            </div>
          ) : (
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {activities.map((activity) => (
                <div key={activity.id} className="bg-white rounded-lg shadow-sm border hover:shadow-md transition-shadow">
                  <div className="p-6">
                    <div className="flex items-start justify-between mb-4">
                      <div className="flex-1">
                        <h3 className="text-lg font-medium text-gray-900 mb-1">{activity.name}</h3>
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(activity.statusName)}`}>
                          {activity.statusName || 'Sin estado'}
                        </span>
                      </div>
                      <div className="flex space-x-2">
                        <button
                          onClick={() => handleEditActivity(activity)}
                          className="text-gray-400 hover:text-blue-600"
                          title="Editar"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDeleteActivity(activity.id)}
                          className="text-gray-400 hover:text-red-600"
                          title="Eliminar"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </button>
                      </div>
                    </div>

                    {activity.description && (
                      <p className="text-gray-600 text-sm mb-4 line-clamp-3">{activity.description}</p>
                    )}

                    <div className="space-y-2 text-sm text-gray-500">
                      <div className="flex items-center">
                        <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                        <span>Inicio: {formatDate(activity.startDate)}</span>
                        {activity.startTime && <span className="ml-1">a las {activity.startTime}</span>}
                      </div>

                      {activity.endDate && (
                        <div className="flex items-center">
                          <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                          <span>Fin: {formatDate(activity.endDate)}</span>
                          {activity.endTime && <span className="ml-1">a las {activity.endTime}</span>}
                        </div>
                      )}

                      {activity.location && (
                        <div className="flex items-center">
                          <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                          </svg>
                          <span>{activity.location}</span>
                        </div>
                      )}

                      {activity.organizationName && (
                        <div className="flex items-center">
                          <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                          </svg>
                          <span>{activity.organizationName}</span>
                        </div>
                      )}
                    </div>

                    <div className="mt-4 pt-4 border-t border-gray-200">
                      <p className="text-xs text-gray-400">
                        Creada: {formatDate(activity.createdAt)}
                      </p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
};