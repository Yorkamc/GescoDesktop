import React, { useState } from 'react';
import { InlineSpinner } from './../components/LoadingSpinner';
import type { CashRegister, CloseCashRegisterRequest } from './../types/cashRegister';

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
  const [formData, setFormData] = useState<CloseCashRegisterRequest>({
    cashDeclared: 0,
    closedBy: '',
    supervisedBy: '',
    observations: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  // Obtener usuario actual del localStorage
  React.useEffect(() => {
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        setFormData(prev => ({
          ...prev,
          closedBy: user.id || user.nombreUsuario || '',
        }));
      }
    } catch (err) {
      console.error('Error obteniendo usuario:', err);
    }
  }, []);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (formData.cashDeclared < 0) {
      newErrors.cashDeclared = 'El monto no puede ser negativo';
    }

    if (!formData.closedBy.trim()) {
      newErrors.closedBy = 'Debe especificar quién cierra la caja';
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
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Icon */}
          <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 mb-4">
            <svg className="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>

          {/* Header */}
          <div className="text-center mb-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Cerrar Caja Registradora
            </h3>
            <p className="text-gray-600 text-sm mb-4">
              Completa los datos del cierre de caja
            </p>

            {/* Cash Register Info */}
            <div className="bg-gray-50 rounded-lg p-3 text-left space-y-1">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-gray-700">Caja:</span>
                <span className="text-xs text-gray-900 font-semibold">{cashRegister.name}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-gray-700">Número:</span>
                <span className="text-xs text-gray-900">{cashRegister.registerNumber}</span>
              </div>
            </div>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Efectivo Declarado */}
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
                disabled={isSubmitting}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 
                         focus:border-red-500 disabled:bg-gray-100
                         ${errors.cashDeclared ? 'border-red-500' : 'border-gray-300'}`}
                placeholder="0.00"
                autoFocus
              />
              {errors.cashDeclared && (
                <p className="mt-1 text-sm text-red-600">{errors.cashDeclared}</p>
              )}
              <p className="mt-1 text-xs text-gray-500">
                Monto total en efectivo contado en la caja
              </p>
            </div>

            {/* Cerrado Por */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Cerrado Por <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={formData.closedBy}
                onChange={(e) => setFormData({ ...formData, closedBy: e.target.value })}
                disabled={isSubmitting}
                className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-red-500 
                         focus:border-red-500 disabled:bg-gray-100
                         ${errors.closedBy ? 'border-red-500' : 'border-gray-300'}`}
                placeholder="ID o nombre del usuario"
              />
              {errors.closedBy && (
                <p className="mt-1 text-sm text-red-600">{errors.closedBy}</p>
              )}
            </div>

            {/* Supervisado Por (Opcional) */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Supervisado Por (Opcional)
              </label>
              <input
                type="text"
                value={formData.supervisedBy}
                onChange={(e) => setFormData({ ...formData, supervisedBy: e.target.value })}
                disabled={isSubmitting}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                         focus:ring-red-500 focus:border-red-500 disabled:bg-gray-100"
                placeholder="ID o nombre del supervisor"
              />
            </div>

            {/* Observaciones */}
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
                         focus:ring-red-500 focus:border-red-500 disabled:bg-gray-100"
                placeholder="Notas adicionales sobre el cierre..."
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
                </p>
              </div>
            </div>

            {/* Buttons */}
            <div className="flex gap-3 pt-2">
              <button
                type="button"
                onClick={onCancel}
                disabled={isSubmitting}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-gray-700 
                         hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors font-medium"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 
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
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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