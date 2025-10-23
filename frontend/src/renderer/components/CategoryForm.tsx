import React, { useState, useEffect } from 'react';
import type { ServiceCategory, CreateServiceCategoryRequest } from '../types';
import { InlineSpinner } from '../pages/Category/LoadingSpinner';

interface ServiceCategoryFormProps {
  category?: ServiceCategory | null;
  isSubmitting: boolean;
  onSubmit: (data: CreateServiceCategoryRequest) => void;
  onCancel: () => void;
}

export const ServiceCategoryForm: React.FC<ServiceCategoryFormProps> = ({
  category,
  isSubmitting,
  onSubmit,
  onCancel,
}) => {
  const [formData, setFormData] = useState<CreateServiceCategoryRequest>({
    organizationId: '',
    name: '',
    description: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (category) {
      setFormData({
        organizationId: category.organizationId,
        name: category.name,
        description: category.description,
      });
    }
  }, [category]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.organizationId.trim()) {
      newErrors.organizationId = 'El ID de organización es requerido';
    }

    if (!formData.name.trim()) {
      newErrors.name = 'El nombre es requerido';
    } else if (formData.name.length < 3) {
      newErrors.name = 'El nombre debe tener al menos 3 caracteres';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'La descripción es requerida';
    } else if (formData.description.length < 10) {
      newErrors.description = 'La descripción debe tener al menos 10 caracteres';
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

  const handleChange = (field: keyof CreateServiceCategoryRequest, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <div className="flex justify-between items-start mb-6">
            <div>
              <h2 className="text-2xl font-bold text-gray-900">
                {category ? 'Editar Categoría' : 'Nueva Categoría'}
              </h2>
              <p className="text-gray-600 mt-1">
                {category
                  ? 'Modifica los datos de la categoría'
                  : 'Completa los datos para crear una nueva categoría'}
              </p>
            </div>
            <button
              onClick={onCancel}
              disabled={isSubmitting}
              className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
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

          <form onSubmit={handleSubmit}>
            <div className="space-y-6">
              <div>
                <label
                  htmlFor="organizationId"
                  className="block text-sm font-medium text-gray-700 mb-2"
                >
                  ID de Organización
                </label>
                <input
                  id="organizationId"
                  type="text"
                  value={formData.organizationId}
                  onChange={(e) => handleChange('organizationId', e.target.value)}
                  disabled={isSubmitting || !!category}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed
                           ${errors.organizationId ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="Ej: ORG-12345"
                />
                {errors.organizationId && (
                  <p className="mt-1 text-sm text-red-600">{errors.organizationId}</p>
                )}
                {category && (
                  <p className="mt-1 text-sm text-gray-500">
                    El ID de organización no se puede modificar
                  </p>
                )}
              </div>

              <div>
                <label
                  htmlFor="name"
                  className="block text-sm font-medium text-gray-700 mb-2"
                >
                  Nombre de la Categoría
                </label>
                <input
                  id="name"
                  type="text"
                  value={formData.name}
                  onChange={(e) => handleChange('name', e.target.value)}
                  disabled={isSubmitting}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed
                           ${errors.name ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="Ej: Consultoría Empresarial"
                />
                {errors.name && (
                  <p className="mt-1 text-sm text-red-600">{errors.name}</p>
                )}
              </div>

              <div>
                <label
                  htmlFor="description"
                  className="block text-sm font-medium text-gray-700 mb-2"
                >
                  Descripción
                </label>
                <textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => handleChange('description', e.target.value)}
                  disabled={isSubmitting}
                  rows={4}
                  className={`w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 
                           focus:border-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed
                           ${errors.description ? 'border-red-500' : 'border-gray-300'}`}
                  placeholder="Describe la categoría de servicio..."
                />
                {errors.description && (
                  <p className="mt-1 text-sm text-red-600">{errors.description}</p>
                )}
                <p className="mt-1 text-sm text-gray-500">
                  {formData.description.length} caracteres (mínimo 10)
                </p>
              </div>
            </div>

            <div className="flex gap-3 mt-8 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={onCancel}
                disabled={isSubmitting}
                className="flex-1 px-6 py-3 border border-gray-300 rounded-lg text-gray-700 
                         hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed
                         transition-colors duration-200"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 
                         disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-200
                         flex items-center justify-center"
              >
                {isSubmitting ? (
                  <>
                    <InlineSpinner className="h-4 w-4 text-white mr-2" />
                    {category ? 'Actualizando...' : 'Creando...'}
                  </>
                ) : category ? (
                  'Actualizar Categoría'
                ) : (
                  'Crear Categoría'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};