import React from 'react';
import { useNavigate } from 'react-router-dom';
import type { CashRegister } from '../types/cashRegister';

interface CashRegisterCardProps {
  cashRegister: CashRegister;
  onEdit: (cashRegister: CashRegister) => void;
  onDelete: (id: string, name: string) => void;
  onOpen: (cashRegister: CashRegister) => void;
  onClose: (cashRegister: CashRegister) => void;
}

export const CashRegisterCard: React.FC<CashRegisterCardProps> = ({
  cashRegister,
  onEdit,
  onDelete,
  onOpen,
  onClose,
}) => {
  const navigate = useNavigate();

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const handleManageSales = () => {
    navigate(`/sales?cashRegisterId=${cashRegister.id}`);
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-blue-100 text-blue-700 font-bold text-sm">
              {cashRegister.registerNumber}
            </span>
            <h3 className="text-lg font-semibold text-gray-900">
              {cashRegister.name}
            </h3>
          </div>
          <span
            className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
              cashRegister.isOpen
                ? 'bg-green-100 text-green-800'
                : 'bg-gray-100 text-gray-800'
            }`}
          >
            {cashRegister.isOpen ? '🟢 Abierta' : '🔴 Cerrada'}
          </span>
        </div>
        
        {/* ✅ NUEVO: Botón de Ventas en la esquina superior derecha */}
        <button
          onClick={handleManageSales}
          className="ml-3 px-3 py-1.5 bg-green-50 text-green-700 rounded-lg hover:bg-green-100 
                   transition-colors duration-200 flex items-center gap-1.5 text-sm font-medium"
          title="Gestionar Ventas"
        >
          <svg className="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          <span className="whitespace-nowrap">Ventas</span>
        </button>
      </div>

      {/* Info */}
      <div className="space-y-2 mb-4 text-sm text-gray-600">
        {/* Actividad */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
          <span className="font-medium">Actividad:</span>
          <span className="ml-1 truncate">{cashRegister.activityName}</span>
        </div>

        {/* Ubicación */}
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          <span className="font-medium">Ubicación:</span>
          <span className="ml-1">{cashRegister.location}</span>
        </div>

        {/* Operador */}
        {cashRegister.operatorUserName && (
          <div className="flex items-center">
            <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <span className="font-medium">Operador:</span>
            <span className="ml-1">{cashRegister.operatorUserName}</span>
          </div>
        )}

        {/* Fecha de apertura */}
        {cashRegister.isOpen && cashRegister.openedAt && (
          <div className="flex items-center text-green-600">
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <span className="font-medium">Abierta desde:</span>
            <span className="ml-1 text-xs">{formatDate(cashRegister.openedAt)}</span>
          </div>
        )}

        {/* Fecha de cierre */}
        {!cashRegister.isOpen && cashRegister.closedAt && (
          <div className="flex items-center text-gray-600">
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
            <span className="font-medium">Cerrada:</span>
            <span className="ml-1 text-xs">{formatDate(cashRegister.closedAt)}</span>
          </div>
        )}
      </div>

      {/* Botones */}
      <div className="grid grid-cols-2 gap-2">
        {/* Abrir/Cerrar Caja */}
        {cashRegister.isOpen ? (
          <button
            onClick={() => onClose(cashRegister)}
            className="bg-red-50 text-red-700 px-3 py-2 rounded-lg hover:bg-red-100 
                     transition-colors flex items-center justify-center gap-2 text-sm font-medium"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
            Cerrar
          </button>
        ) : (
          <button
            onClick={() => onOpen(cashRegister)}
            className="bg-green-50 text-green-700 px-3 py-2 rounded-lg hover:bg-green-100 
                     transition-colors flex items-center justify-center gap-2 text-sm font-medium"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 11V7a4 4 0 118 0m-4 8v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2z" />
            </svg>
            Abrir
          </button>
        )}

        {/* Editar */}
        <button
          onClick={() => onEdit(cashRegister)}
          className="bg-blue-50 text-blue-700 px-3 py-2 rounded-lg hover:bg-blue-100 
                   transition-colors flex items-center justify-center gap-2 text-sm font-medium"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
          </svg>
          Editar
        </button>

        {/* Eliminar */}
        <button
          onClick={() => onDelete(cashRegister.id, cashRegister.name)}
          disabled={cashRegister.isOpen}
          className="col-span-2 bg-red-50 text-red-700 px-3 py-2 rounded-lg hover:bg-red-100 
                   transition-colors flex items-center justify-center gap-2 text-sm font-medium
                   disabled:opacity-50 disabled:cursor-not-allowed"
          title={cashRegister.isOpen ? 'No se puede eliminar una caja abierta' : 'Eliminar caja'}
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
          Eliminar
        </button>
      </div>
    </div>
  );
};