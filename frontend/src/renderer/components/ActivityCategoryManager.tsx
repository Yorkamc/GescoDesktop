import React, { useState, useMemo } from 'react';
import { useActivityCategories } from '../hooks/useActivityCategories';
import { useServiceCategories } from '../hooks/useCategories';
import { LoadingSpinner } from './LoadingSpinner';
import { Alert } from './Alert';

interface ActivityCategoryManagerProps {
  activityId: string;
  activityName: string;
  onClose: () => void;
}

export const ActivityCategoryManager: React.FC<ActivityCategoryManagerProps> = ({
  activityId,
  activityName,
  onClose,
}) => {
  const {
    categories: assignedCategories,
    isLoading: loadingAssigned,
    error: assignedError,
    assignCategory,
    removeCategory,
    clearError: clearAssignedError,
  } = useActivityCategories(activityId);

  const {
    categories: allCategories,
    isLoading: loadingAll,
    error: allError,
  } = useServiceCategories();

  const [isAssigning, setIsAssigning] = useState(false);
  const [selectedCategoryId, setSelectedCategoryId] = useState('');

  // Categorías disponibles para asignar (que no están ya asignadas)
  const availableCategories = useMemo(() => {
    const assignedIds = new Set(assignedCategories.map(ac => ac.serviceCategoryId));
    return allCategories.filter(cat => cat.active && !assignedIds.has(cat.id));
  }, [allCategories, assignedCategories]);

  const handleAssign = async () => {
    if (!selectedCategoryId) return;

    setIsAssigning(true);
    try {
      await assignCategory(activityId, selectedCategoryId);
      setSelectedCategoryId('');
    } catch (err) {
      console.error('Error assigning category:', err);
    } finally {
      setIsAssigning(false);
    }
  };

  const handleRemove = async (categoryId: string) => {
    if (!confirm('¿Estás seguro de que quieres eliminar esta categoría de la actividad?')) {
      return;
    }

    try {
      await removeCategory(categoryId);
    } catch (err) {
      console.error('Error removing category:', err);
    }
  };

  const isLoading = loadingAssigned || loadingAll;
  const error = assignedError || allError;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          {/* Header */}
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                Gestionar Categorías
              </h2>
              <p className="text-gray-600 mt-1">
                Actividad: <span className="font-medium">{activityName}</span>
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <svg
                className="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          {error && (
            <Alert
              type="error"
              message={error}
              onDismiss={clearAssignedError}
            />
          )}

          {isLoading ? (
            <div className="py-12">
              <LoadingSpinner size="medium" message="Cargando categorías..." />
            </div>
          ) : (
            <>
              {/* Asignar nueva categoría */}
              <div className="mb-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
                <h3 className="text-lg font-semibold text-gray-900 mb-3">
                  Asignar Nueva Categoría
                </h3>
                
                {availableCategories.length === 0 ? (
                  <p className="text-gray-600 text-sm">
                    No hay categorías disponibles para asignar. Todas las categorías activas ya están asignadas a esta actividad.
                  </p>
                ) : (
                  <div className="flex gap-3">
                    <select
                      value={selectedCategoryId}
                      onChange={(e) => setSelectedCategoryId(e.target.value)}
                      disabled={isAssigning}
                      className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                               focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100"
                    >
                      <option value="">Selecciona una categoría...</option>
                      {availableCategories.map((category) => (
                        <option key={category.id} value={category.id}>
                          {category.name}
                        </option>
                      ))}
                    </select>
                    <button
                      onClick={handleAssign}
                      disabled={!selectedCategoryId || isAssigning}
                      className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                               disabled:opacity-50 disabled:cursor-not-allowed transition-colors
                               flex items-center gap-2"
                    >
                      {isAssigning ? (
                        <>
                          <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                          </svg>
                          Asignando...
                        </>
                      ) : (
                        <>
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                          </svg>
                          Asignar
                        </>
                      )}
                    </button>
                  </div>
                )}
              </div>

              {/* Categorías asignadas */}
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-3">
                  Categorías Asignadas ({assignedCategories.length})
                </h3>

                {assignedCategories.length === 0 ? (
                  <div className="text-center py-12 bg-gray-50 rounded-lg">
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
                        d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
                      />
                    </svg>
                    <h3 className="text-lg font-medium text-gray-900 mb-2">
                      No hay categorías asignadas
                    </h3>
                    <p className="text-gray-600">
                      Asigna una categoría desde el menú de arriba
                    </p>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {assignedCategories.map((category) => (
                      <div
                        key={category.id}
                        className="flex items-center justify-between p-4 bg-white border 
                                 border-gray-200 rounded-lg hover:shadow-md transition-shadow"
                      >
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                            <svg
                              className="w-6 h-6 text-purple-600"
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
                          </div>
                          <div>
                            <h4 className="font-medium text-gray-900">
                              {category.serviceCategoryName}
                            </h4>
                            <p className="text-sm text-gray-500">
                              Asignada el {new Date(category.createdAt).toLocaleDateString('es-ES')}
                            </p>
                          </div>
                        </div>
                        <button
                          onClick={() => handleRemove(category.id)}
                          className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                          title="Eliminar categoría"
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
                              d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                            />
                          </svg>
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </>
          )}

          {/* Footer */}
          <div className="mt-6 pt-6 border-t border-gray-200 flex justify-end">
            <button
              onClick={onClose}
              className="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 
                       transition-colors font-medium"
            >
              Cerrar
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};