import React from 'react';
import { InlineSpinner } from './../components/LoadingSpinner';
import type { CashRegister } from './../types/cashRegister';

interface OpenCashModalProps {
  cashRegister: CashRegister;
  isSubmitting: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export const OpenCashModal: React.FC<OpenCashModalProps> = ({
  cashRegister,
  isSubmitting,
  onConfirm,
  onCancel,
}) => {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full">
        <div className="p-6">
          {/* Icon */}
          <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
            <svg className="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 11V7a4 4 0 118 0m-4 8v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2z" />
            </svg>
          </div>

          {/* Content */}
          <div className="text-center mb-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              Abrir Caja Registradora
            </h3>
            <p className="text-gray-600 mb-4">
              ¿Estás seguro de que deseas abrir la siguiente caja?
            </p>

            {/* Cash Register Info */}
            <div className="bg-gray-50 rounded-lg p-4 text-left space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Caja:</span>
                <span className="text-sm text-gray-900 font-semibold">{cashRegister.name}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Número:</span>
                <span className="text-sm text-gray-900">{cashRegister.registerNumber}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Ubicación:</span>
                <span className="text-sm text-gray-900">{cashRegister.location}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">Actividad:</span>
                <span className="text-sm text-gray-900">{cashRegister.activityName}</span>
              </div>
            </div>
          </div>

          {/* Alert */}
          <div className="mb-6 p-3 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex">
              <svg className="h-5 w-5 text-blue-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-xs text-blue-800">
                La caja quedará lista para realizar transacciones. Asegúrate de tener el efectivo inicial preparado.
              </p>
            </div>
          </div>

          {/* Buttons */}
          <div className="flex gap-3">
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
              type="button"
              onClick={onConfirm}
              disabled={isSubmitting}
              className="flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 
                       disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                       flex items-center justify-center font-medium"
            >
              {isSubmitting ? (
                <>
                  <InlineSpinner className="h-4 w-4 text-white mr-2" />
                  Abriendo...
                </>
              ) : (
                <>
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 11V7a4 4 0 118 0m-4 8v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2z" />
                  </svg>
                  Abrir Caja
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};