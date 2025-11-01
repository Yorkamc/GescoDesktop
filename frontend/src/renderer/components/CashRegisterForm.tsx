import React, { useState, useEffect } from 'react';
import { InlineSpinner } from './../components/LoadingSpinner';
import type { CashRegister, CreateCashRegisterRequest } from './../types/cashRegister';
import { useActivities } from './../hooks/useActivities';

interface CashRegisterFormProps {
  cashRegister: CashRegister | null;
  isSubmitting: boolean;
  onSubmit: (data: CreateCashRegisterRequest) => void;
  onCancel: () => void;
}

export const CashRegisterForm: React.FC<CashRegisterFormProps> = ({
  cashRegister,
  isSubmitting,
  onSubmit,
  onCancel,
}) => {
  const { activities } = useActivities();
  
  const [formData, setFormData] = useState<CreateCashRegisterRequest>({
    activityId: '',
    registerNumber: 1,
    name: '',
    location: '',
    operatorUserId: undefined,
    supervisorUserId: undefined,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (cashRegister) {
      setFormData({
        activityId: cashRegister.activityId,
        registerNumber: cashRegister.registerNumber,
        name: cashRegister.name,
        location: cashRegister.location,
        operatorUserId: cashRegister.operatorUserId,
        supervisorUserId: cashRegister.supervisorUserId,
      });
    }
  }, [cashRegister]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.activityId) {
      newErrors.activityId = 'Debes seleccionar una actividad';
    }

    if (!formData.name.trim()) {
      newErrors.name = 'El nombre es requerido';
    }

    if (formData.registerNumber < 1) {
      newErrors.registerNumber = 'El número debe ser mayor a 0';
    }

    if (!formData.location.trim()) {
      newErrors.location = 'La ubicación es requerida';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      onSubmit(formData);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                {cashRegister ? 'Editar Caja' : 'Nueva Caja Registradora'}
              </h2>
              <p className="text-gray-600 mt-1">
                {cashRegister
                  ? 'Modifica los datos de la caja'
                  : 'Completa los datos para crear una nueva caja'}
              </p>
            </div>
            <button
              onClick={onCancel}
              disabled={isSubmitting}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Actividad */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Actividad <span className="text-red-500">*</span>
              </label>
              <select
                value={formData.activityId}
                onChange={(e) => setFormData({ ...formData, activityId: e.target.value })}
                disabled={isSubmitting || !!cashRegister}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                         focus:border-blue-500 disabled:bg-gray-100
                         ${errors.activityId ? 'border-red-500' : 'border-gray-300'}`}
              >
                <option value="">Selecciona una actividad...</option>
                {activities.map((activity) => (
                  <option key={activity.id} value={activity.id}>
                    {activity.name}
                  </option>
                ))}
              </select>
              {errors.activityId && (
                <p className="mt-1 text-sm text-red-600">{errors.activityId}</p>
              )}
              {cashRegister && (
                <p className="mt-1 text-sm text-gray-500">
                  La actividad no se puede modificar
                </p>
              )}
            </div>

            {/* Nombre y Número */}
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Nombre <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  disabled={isSubmitting}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100
                           ${errors.name ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="Ej: Caja Principal"
                />
                {errors.name && (
                  <p className="mt-1 text-sm text-red-600">{errors.name}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Número <span className="text-red-500">*</span>
                </label>
                <input
                  type="number"
                  min="1"
                  value={formData.registerNumber}
                  onChange={(e) => setFormData({ ...formData, registerNumber: parseInt(e.target.value) || 1 })}
                  disabled={isSubmitting}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100
                           ${errors.registerNumber ? 'border-red-500' : 'border-gray-300'}`}
                />
                {errors.registerNumber && (
                  <p className="mt-1 text-sm text-red-600">{errors.registerNumber}</p>
                )}
              </div>
            </div>

            {/* Ubicación */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Ubicación <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.location}
                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                disabled={isSubmitting}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                         focus:border-blue-500 disabled:bg-gray-100
                         ${errors.location ? 'border-red-500' : 'border-gray-300'}`}
                placeholder="Ej: Entrada principal"
              />
              {errors.location && (
                <p className="mt-1 text-sm text-red-600">{errors.location}</p>
              )}
            </div>

            {/* Botones */}
            <div className="flex gap-3 mt-6 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={onCancel}
                disabled={isSubmitting}
                className="flex-1 px-6 py-3 border border-gray-300 rounded-lg text-gray-700 
                         hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                         flex items-center justify-center"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    {cashRegister ? 'Actualizando...' : 'Creando...'}
                  </>
                ) : cashRegister ? (
                  'Actualizar Caja'
                ) : (
                  'Crear Caja'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};