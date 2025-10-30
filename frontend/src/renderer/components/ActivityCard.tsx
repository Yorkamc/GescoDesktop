import React from 'react';
import type { Activity } from '../types';
import { formatDate, getActivityStatusColor } from '../utils/formatters';

export interface ActivityCardProps {
  activity: Activity;
  onEdit: (activity: Activity) => void;
  onDelete: (id: string) => void;
  onManageCategories?: (activity: Activity) => void;
  onManageProducts?: (activity: Activity) => void;
  categoryCount?: number;
  productCount?: number;
}

export const ActivityCard: React.FC<ActivityCardProps> = ({
  activity,
  onEdit,
  onDelete,
  onManageCategories,
  onManageProducts,
  categoryCount = 0,
  productCount = 0,
}) => {
  const startDate = formatDate(activity.startDate);
  const endDate = activity.endDate ? formatDate(activity.endDate) : 'Sin definir';

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow duration-200">
      <div className="flex justify-between items-start mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-1">
            {activity.name}
          </h3>
          <span
            className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getActivityStatusColor(
              activity.statusName
            )}`}
          >
            {activity.statusName || 'Sin estado'}
          </span>
        </div>
      </div>

      {activity.description && (
        <p className="text-gray-600 text-sm mb-4 line-clamp-3">
          {activity.description}
        </p>
      )}

      <div className="space-y-2 mb-4 text-sm text-gray-600">
        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span className="font-medium">Inicio:</span>
          <span className="ml-1">{startDate}</span>
          {activity.startTime && <span className="ml-1">• {activity.startTime.substring(0, 5)}</span>}
        </div>

        <div className="flex items-center">
          <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span className="font-medium">Fin:</span>
          <span className="ml-1">{endDate}</span>
          {activity.endTime && <span className="ml-1">• {activity.endTime.substring(0, 5)}</span>}
        </div>

        {activity.location && (
          <div className="flex items-center">
            <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <span className="font-medium">Ubicación:</span>
            <span className="ml-1">{activity.location}</span>
          </div>
        )}

        {activity.managerName && (
          <div className="flex items-center">
            <svg className="w-4 h-4 mr-2 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <span className="font-medium">Responsable:</span>
            <span className="ml-1">{activity.managerName}</span>
          </div>
        )}

        {/* Indicador de categorías */}
        {categoryCount > 0 && (
          <div className="flex items-center text-purple-600">
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
              />
            </svg>
            <span className="font-medium">
              {categoryCount} {categoryCount === 1 ? 'categoría asignada' : 'categorías asignadas'}
            </span>
          </div>
        )}

        {/* Indicador de productos */}
        {productCount > 0 && (
          <div className="flex items-center text-green-600">
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
              />
            </svg>
            <span className="font-medium">
              {productCount} {productCount === 1 ? 'producto asignado' : 'productos asignados'}
            </span>
          </div>
        )}
      </div>

      {/* Botones con diseño responsive mejorado - Grid de 2x2 REORGANIZADO */}
      <div className="grid grid-cols-2 gap-2">
        {/* PRIMERA FILA: Editar + Eliminar */}
        
        {/* Botón Editar */}
        <button
          onClick={() => onEdit(activity)}
          className="bg-blue-50 text-blue-700 px-3 py-2 rounded-lg hover:bg-blue-100 
                   transition-colors duration-200 flex items-center justify-center gap-2 text-sm font-medium"
        >
          <svg
            className="w-4 h-4 flex-shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
            />
          </svg>
          <span className="hidden sm:inline">Editar</span>
        </button>

        {/* Botón Eliminar */}
        <button
          onClick={() => onDelete(activity.id)}
          className="bg-red-50 text-red-700 px-3 py-2 rounded-lg hover:bg-red-100 
                   transition-colors duration-200 flex items-center justify-center gap-2 text-sm font-medium"
        >
          <svg
            className="w-4 h-4 flex-shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
            />
          </svg>
          <span className="hidden sm:inline">Eliminar</span>
        </button>

        {/* SEGUNDA FILA: Categorías + Productos */}

        {/* Botón Categorías */}
        {onManageCategories && (
          <button
            onClick={() => onManageCategories(activity)}
            className="bg-purple-50 text-purple-700 px-3 py-2 rounded-lg hover:bg-purple-100 
                     transition-colors duration-200 flex items-center justify-center gap-2 text-sm font-medium"
          >
            <svg
              className="w-4 h-4 flex-shrink-0"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
              />
            </svg>
            <span className="hidden sm:inline">Categorías</span>
            {categoryCount > 0 && (
              <span className="ml-1 inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 text-xs font-bold text-white bg-purple-600 rounded-full">
                {categoryCount}
              </span>
            )}
          </button>
        )}

        {/* Botón Productos */}
        {onManageProducts && (
          <button
            onClick={() => onManageProducts(activity)}
            className="bg-green-50 text-green-700 px-3 py-2 rounded-lg hover:bg-green-100 
                     transition-colors duration-200 flex items-center justify-center gap-2 text-sm font-medium"
          >
            <svg
              className="w-4 h-4 flex-shrink-0"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4"
              />
            </svg>
            <span className="hidden sm:inline">Productos</span>
            {productCount > 0 && (
              <span className="ml-1 inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 text-xs font-bold text-white bg-green-600 rounded-full">
                {productCount}
              </span>
            )}
          </button>
        )}
      </div>
    </div>
  );
};