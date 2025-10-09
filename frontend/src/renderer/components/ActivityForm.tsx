import React, { useEffect } from 'react';
import { useForm } from '../hooks/useForm';
import { validateActivity } from '../utils/validators';
import { InlineSpinner } from './LoadingSpinner';
import { Alert } from './Alert';
import type { Activity, CreateActivityRequest } from '../types';

interface ActivityFormProps {
  activity: Activity | null;
  onSubmit: (data: CreateActivityRequest) => Promise<void>;
  onCancel: () => void;
}

const extractDate = (dateString: string): string => {
  if (!dateString) return '';
  if (dateString.includes('T')) {
    return dateString.split('T')[0];
  }
  return dateString;
};

export const ActivityFormWithHook: React.FC<ActivityFormProps> = ({ 
  activity, 
  onSubmit, 
  onCancel 
}) => {
  const initialValues: CreateActivityRequest = {
    name: activity?.name || '',
    description: activity?.description || '',
    startDate: activity ? extractDate(activity.startDate) : '',
    startTime: activity?.startTime || '',
    endDate: activity?.endDate ? extractDate(activity.endDate) : '',
    endTime: activity?.endTime || '',
    location: activity?.location || '',
    activityStatusId: activity?.activityStatusId || 1,
  };

  const form = useForm({
    initialValues,
    validate: validateActivity,
    onSubmit: async (values) => {
      await onSubmit(values);
    },
  });

  // Actualizar valores cuando cambie la actividad a editar
  useEffect(() => {
    if (activity) {
      form.setValues(initialValues);
    }
  }, [activity?.id]);

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
            disabled={form.isSubmitting}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Form Content */}
        <form onSubmit={form.handleSubmit} className="flex-1 overflow-y-auto p-6">
          <div className="space-y-4">
            {/* Nombre */}
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nombre de la Actividad <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                name="name"
                value={form.values.name}
                onChange={form.handleChange}
                onBlur={form.handleBlur}
                className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                  form.touched.name && form.errors.name
                    ? 'border-red-300 focus:border-red-500'
                    : 'border-gray-300'
                }`}
                placeholder="Ej: Festival de Verano 2024"
                disabled={form.isSubmitting}
              />
              {form.touched.name && form.errors.name && (
                <p className="mt-1 text-sm text-red-600">{form.errors.name}</p>
              )}
            </div>

            {/* Descripción */}
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Descripción
              </label>
              <textarea
                name="description"
                value={form.values.description}
                onChange={form.handleChange}
                onBlur={form.handleBlur}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows={3}
                placeholder="Descripción detallada de la actividad..."
                disabled={form.isSubmitting}
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Fecha de Inicio */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Fecha de Inicio <span className="text-red-500">*</span>
                </label>
                <input
                  type="date"
                  name="startDate"
                  value={form.values.startDate}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                    form.touched.startDate && form.errors.startDate
                      ? 'border-red-300'
                      : 'border-gray-300'
                  }`}
                  disabled={form.isSubmitting}
                />
                {form.touched.startDate && form.errors.startDate && (
                  <p className="mt-1 text-sm text-red-600">{form.errors.startDate}</p>
                )}
              </div>

              {/* Hora de Inicio */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hora de Inicio
                </label>
                <input
                  type="time"
                  name="startTime"
                  value={form.values.startTime}
                  onChange={form.handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={form.isSubmitting}
                />
              </div>

              {/* Fecha de Fin */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Fecha de Fin
                </label>
                <input
                  type="date"
                  name="endDate"
                  value={form.values.endDate}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                    form.touched.endDate && form.errors.endDate
                      ? 'border-red-300'
                      : 'border-gray-300'
                  }`}
                  disabled={form.isSubmitting}
                />
                {form.touched.endDate && form.errors.endDate && (
                  <p className="mt-1 text-sm text-red-600">{form.errors.endDate}</p>
                )}
              </div>

              {/* Hora de Fin */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hora de Fin
                </label>
                <input
                  type="time"
                  name="endTime"
                  value={form.values.endTime}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                    form.touched.endTime && form.errors.endTime
                      ? 'border-red-300'
                      : 'border-gray-300'
                  }`}
                  disabled={form.isSubmitting}
                />
                {form.touched.endTime && form.errors.endTime && (
                  <p className="mt-1 text-sm text-red-600">{form.errors.endTime}</p>
                )}
              </div>
            </div>

            {/* Ubicación */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Ubicación
              </label>
              <input
                type="text"
                name="location"
                value={form.values.location}
                onChange={form.handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Ej: Parque Central, San José"
                disabled={form.isSubmitting}
              />
            </div>

            {/* Estado */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Estado
              </label>
              <select
                name="activityStatusId"
                value={form.values.activityStatusId}
                onChange={form.handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={form.isSubmitting}
              >
                <option value={1}>No Iniciada</option>
                <option value={2}>En Progreso</option>
                <option value={3}>Completada</option>
                <option value={4}>Cancelada</option>
              </select>
            </div>
          </div>
        </form>

        {/* Footer */}
        <div className="flex justify-end items-center space-x-3 p-6 border-t border-gray-200 flex-shrink-0 bg-gray-50">
          <button
            type="button"
            onClick={onCancel}
            disabled={form.isSubmitting}
            className="px-4 py-2 text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors font-medium disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={form.handleSubmit}
            disabled={form.isSubmitting}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center font-medium"
          >
            {form.isSubmitting ? (
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