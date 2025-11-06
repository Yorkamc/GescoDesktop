import React, { useState, useEffect } from 'react';
import { InlineSpinner } from '../../components/LoadingSpinner';

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

interface ActivityFormProps {
  activity: Activity | null;
  isSubmitting: boolean;
  onSubmit: (data: CreateActivityRequest) => void;
  onCancel: () => void;
}

const extractDate = (dateString: string): string => {
  if (!dateString) return '';
  if (dateString.includes('T')) {
    return dateString.split('T')[0];
  }
  return dateString;
};

export const ActivityForm: React.FC<ActivityFormProps> = ({ 
  activity, 
  isSubmitting, 
  onSubmit, 
  onCancel 
}) => {
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

  const [formError, setFormError] = useState('');
  const [isInitialized, setIsInitialized] = useState(false);

  // ✅ Solo inicializar una vez
  useEffect(() => {
    if (activity && !isInitialized) {
      setFormData({
        name: activity.name,
        description: activity.description || '',
        startDate: extractDate(activity.startDate),
        startTime: activity.startTime || '',
        endDate: activity.endDate ? extractDate(activity.endDate) : '',
        endTime: activity.endTime || '',
        location: activity.location || '',
        activityStatusId: activity.activityStatusId,
        managerUserId: '',
        organizationId: ''
      });
      setIsInitialized(true);
    } else if (!activity && !isInitialized) {
      setIsInitialized(true);
    }
  }, [activity, isInitialized]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    if (!formData.name.trim()) {
      setFormError('El nombre de la actividad es requerido');
      return;
    }

    if (!formData.startDate) {
      setFormError('La fecha de inicio es requerida');
      return;
    }

    // ✅ Validar que la fecha de fin no sea anterior a la de inicio
    if (formData.endDate && formData.startDate > formData.endDate) {
      setFormError('La fecha de fin no puede ser anterior a la fecha de inicio');
      return;
    }

    onSubmit(formData);
  };

  // ✅ Función genérica para actualizar campos
  const handleFieldChange = (field: keyof CreateActivityRequest, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Limpiar error cuando el usuario empiece a escribir
    if (formError) {
      setFormError('');
    }
  };

  return (
    <div className="fixed inset-0 bg-gray-900 bg-opacity-50 overflow-y-auto z-50 flex items-center justify-center p-4">
      <div className="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 flex-shrink-0">
          <h3 className="text-xl font-semibold text-gray-900">
            {activity ? 'Editar Actividad' : 'Nueva Actividad'}
          </h3>
          <button
            onClick={onCancel}
            className="text-gray-400 hover:text-gray-600 transition-colors"
            disabled={isSubmitting}
            type="button"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Form Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {formError && (
            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
              {formError}
            </div>
          )}

          <form onSubmit={handleSubmit} id="activityForm" className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Nombre */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre de la Actividad <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => handleFieldChange('name', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  placeholder="Ej: Festival de Verano 2024"
                  disabled={isSubmitting}
                  autoComplete="off"
                />
              </div>

              {/* Descripción */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Descripción
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => handleFieldChange('description', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  rows={3}
                  placeholder="Descripción detallada de la actividad..."
                  disabled={isSubmitting}
                />
              </div>

              {/* Fecha de Inicio */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Fecha de Inicio <span className="text-red-500">*</span>
                </label>
                <input
                  type="date"
                  value={formData.startDate}
                  onChange={(e) => handleFieldChange('startDate', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  required
                  disabled={isSubmitting}
                />
              </div>

              {/* Hora de Inicio */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hora de Inicio
                </label>
                <input
                  type="time"
                  value={formData.startTime}
                  onChange={(e) => handleFieldChange('startTime', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={isSubmitting}
                />
              </div>

              {/* Fecha de Fin */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Fecha de Fin
                </label>
                <input
                  type="date"
                  value={formData.endDate}
                  onChange={(e) => handleFieldChange('endDate', e.target.value)}
                  min={formData.startDate} // ✅ Prevenir fechas anteriores
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={isSubmitting}
                />
              </div>

              {/* Hora de Fin */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hora de Fin
                </label>
                <input
                  type="time"
                  value={formData.endTime}
                  onChange={(e) => handleFieldChange('endTime', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={isSubmitting}
                />
              </div>

              {/* Ubicación */}
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Ubicación
                </label>
                <input
                  type="text"
                  value={formData.location}
                  onChange={(e) => handleFieldChange('location', e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Ej: Parque Central, San José"
                  disabled={isSubmitting}
                  autoComplete="off"
                />
              </div>

              {/* Estado */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Estado
                </label>
                <select
                  value={formData.activityStatusId}
                  onChange={(e) => handleFieldChange('activityStatusId', parseInt(e.target.value))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={isSubmitting}
                >
                  <option value={1}>No Iniciada</option>
                  <option value={2}>En Progreso</option>
                  <option value={3}>Completada</option>
                  <option value={4}>Cancelada</option>
                </select>
              </div>
            </div>
          </form>
        </div>

        {/* Footer */}
        <div className="flex justify-end items-center space-x-3 p-6 border-t border-gray-200 flex-shrink-0 bg-gray-50">
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors font-medium disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="submit"
            form="activityForm"
            disabled={isSubmitting}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center font-medium"
          >
            {isSubmitting ? (
              <>
                <InlineSpinner className="h-4 w-4 text-white mr-2" />
                {activity ? 'Actualizando...' : 'Creando...'}
              </>
            ) : (
              activity ? 'Actualizar Actividad' : 'Crear Actividad'
            )}
          </button>
        </div>
      </div>
    </div>
  );
};