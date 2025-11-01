import React, { useState, useMemo, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useServiceCategories } from '../../hooks/useCategories';
import { ServiceCategoryCard } from '../../components/CategoryCard';
import { ServiceCategoryForm } from '../../components/CategoryForm';
import { Alert } from '../../components/Alert';
import { LoadingSpinner } from '../../components/LoadingSpinner';
import type { ServiceCategory, CreateServiceCategoryRequest } from '../../types';

export const ServiceCategories: React.FC = () => {
  const navigate = useNavigate();
  const {
    categories,
    isLoading,
    error,
    createCategory,
    updateCategory,
    deleteCategory,
    refreshCategories,
    clearError,
  } = useServiceCategories();

  const [showForm, setShowForm] = useState(false);
  const [editingCategory, setEditingCategory] = useState<ServiceCategory | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [showActiveOnly, setShowActiveOnly] = useState(false);
  const [organizationId, setOrganizationId] = useState<string>('');

  // ✅ MEJORADO: Obtener organizationId al montar el componente
  useEffect(() => {
    const orgId = getOrganizationId();
    console.log('🏢 Organization ID obtenido:', orgId);
    setOrganizationId(orgId);
  }, []);

  // ✅ MEJORADO: Función más robusta para obtener organizationId
  const getOrganizationId = (): string => {
    console.log('🔍 Buscando organizationId...');
    
    try {
      // Opción 1: Desde localStorage 'user'
      const userStr = localStorage.getItem('user');
      console.log('📦 localStorage user:', userStr);
      
      if (userStr) {
        const user = JSON.parse(userStr);
        console.log('👤 Usuario parseado:', user);
        
        // Intentar diferentes variaciones del campo
        const orgId = 
          user.organizationId || 
          user.organization_id || 
          user.OrganizationId ||
          user.organisationId ||
          '';
        
        if (orgId) {
          console.log('✅ Organization ID encontrado en user:', orgId);
          return orgId;
        }
      }

      // Opción 2: Desde localStorage 'organizationId' directo
      const directOrgId = localStorage.getItem('organizationId') || localStorage.getItem('organization_id');
      if (directOrgId) {
        console.log('✅ Organization ID encontrado directo:', directOrgId);
        return directOrgId;
      }

      // Opción 3: Desde JWT token
      const token = localStorage.getItem('token') || localStorage.getItem('accessToken');
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          console.log('🎫 Token payload:', payload);
          
          const orgId = 
            payload.organizationId || 
            payload.organization_id || 
            payload.OrganizationId ||
            '';
          
          if (orgId) {
            console.log('✅ Organization ID encontrado en token:', orgId);
            return orgId;
          }
        } catch (tokenErr) {
          console.error('❌ Error parseando token:', tokenErr);
        }
      }

      console.warn('⚠️ No se encontró organizationId en ninguna ubicación');
      console.log('📋 localStorage keys:', Object.keys(localStorage));
      
    } catch (err) {
      console.error('❌ Error obteniendo organizationId:', err);
    }
    
    return '';
  };

  const handleEdit = (category: ServiceCategory) => {
    setEditingCategory(category);
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Estás seguro de que quieres eliminar esta categoría?')) {
      return;
    }

    try {
      await deleteCategory(id);
    } catch (err) {
      console.error('Error deleting category:', err);
    }
  };

  const handleSubmit = async (data: Omit<CreateServiceCategoryRequest, 'organizationId'>) => {
    console.log('📤 Iniciando submit de categoría');
    console.log('📋 Datos del formulario:', data);
    console.log('🏢 Organization ID actual:', organizationId);
    
    if (!organizationId) {
      console.error('❌ organizationId está vacío');
      alert('Error: No se pudo obtener el ID de organización. Por favor, inicia sesión nuevamente.');
      
      // Mostrar información de debug
      console.log('🔍 Debug info:');
      console.log('- localStorage keys:', Object.keys(localStorage));
      console.log('- user:', localStorage.getItem('user'));
      console.log('- token:', localStorage.getItem('token'));
      
      return;
    }

    setIsSubmitting(true);
    try {
      const fullData: CreateServiceCategoryRequest = {
        ...data,
        organizationId
      };

      console.log('✅ Datos completos a enviar:', fullData);

      if (editingCategory) {
        await updateCategory(editingCategory.id, fullData);
        console.log('✅ Categoría actualizada');
      } else {
        await createCategory(fullData);
        console.log('✅ Categoría creada');
      }
      
      handleCancel();
    } catch (err: any) {
      console.error('❌ Error submitting category:', err);
      alert(`Error al guardar la categoría: ${err.message || 'Error desconocido'}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditingCategory(null);
  };

  const filteredCategories = useMemo(() => {
    return categories.filter(category => {
      const matchesSearch = !searchQuery || 
        category.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        category.description.toLowerCase().includes(searchQuery.toLowerCase());
      
      const matchesActive = !showActiveOnly || category.active;
      
      return matchesSearch && matchesActive;
    });
  }, [categories, searchQuery, showActiveOnly]);

  // ✅ NUEVO: Mostrar advertencia si no hay organizationId
  useEffect(() => {
    if (!organizationId && !isLoading) {
      console.warn('⚠️ No se encontró organizationId. Algunas funciones pueden no estar disponibles.');
    }
  }, [organizationId, isLoading]);

  if (isLoading) {
    return (
      <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
        <header className="bg-white shadow-sm border-b flex-shrink-0">
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
                  <h1 className="text-xl font-bold text-gray-900">Categorías de Servicio</h1>
                  <p className="text-xs text-gray-500">Cargando...</p>
                </div>
              </div>
            </div>
          </div>
        </header>
        <div className="flex-1 flex justify-center items-center">
          <LoadingSpinner size="large" />
        </div>
      </div>
    );
  }

  return (
    <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
      {/* Header con botón de regreso */}
      <header className="bg-white shadow-sm border-b flex-shrink-0">
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
                <h1 className="text-xl font-bold text-gray-900">Categorías de Servicio</h1>
                <p className="text-xs text-gray-500">{categories.length} categorías registradas</p>
              </div>
            </div>
            <button
              onClick={() => {
                if (!organizationId) {
                  alert('Error: No se pudo obtener el ID de organización. Por favor, inicia sesión nuevamente.');
                  return;
                }
                setShowForm(true);
              }}
              className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 
                       transition-colors duration-200 flex items-center gap-2"
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
                  d="M12 4v16m8-8H4"
                />
              </svg>
              Nueva Categoría
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto">
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            {/* ✅ NUEVO: Mostrar advertencia si no hay organizationId */}
            {!organizationId && (
              <Alert
                type="warning"
                message="No se pudo obtener el ID de organización. Algunas funciones pueden no estar disponibles. Por favor, inicia sesión nuevamente."
              />
            )}

            {error && (
              <Alert
                type="error"
                message={error}
                onDismiss={clearError}
              />
            )}

            <div className="mb-6 bg-white rounded-lg shadow p-6">
              <div className="flex flex-col md:flex-row gap-4">
                <div className="flex-1">
                  <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-2">
                    Buscar categorías
                  </label>
                  <input
                    id="search"
                    type="text"
                    placeholder="Buscar por nombre o descripción..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 
                             focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                <div className="flex items-end">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={showActiveOnly}
                      onChange={(e) => setShowActiveOnly(e.target.checked)}
                      className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                    />
                    <span className="text-sm font-medium text-gray-700">
                      Solo activas
                    </span>
                  </label>
                </div>
              </div>

              <div className="mt-4 flex items-center justify-between text-sm text-gray-600">
                <span>
                  Mostrando {filteredCategories.length} de {categories.length} categorías
                </span>
                <button
                  onClick={refreshCategories}
                  className="text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1"
                >
                  <svg
                    className="w-4 h-4"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                    />
                  </svg>
                  Actualizar
                </button>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow">
              <div className="p-6">
                {filteredCategories.length === 0 ? (
                  <div className="text-center py-12">
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
                      {searchQuery || showActiveOnly
                        ? 'No se encontraron categorías'
                        : 'No hay categorías'}
                    </h3>
                    <p className="text-gray-600 mb-4">
                      {searchQuery || showActiveOnly
                        ? 'Intenta ajustar los filtros de búsqueda'
                        : 'Comienza creando tu primera categoría'}
                    </p>
                    {!searchQuery && !showActiveOnly && organizationId && (
                      <button
                        onClick={() => setShowForm(true)}
                        className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
                      >
                        Crear Primera Categoría
                      </button>
                    )}
                  </div>
                ) : (
                  <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {filteredCategories.map((category) => (
                      <ServiceCategoryCard
                        key={category.id}
                        category={category}
                        onEdit={handleEdit}
                        onDelete={handleDelete}
                      />
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </main>

      {showForm && (
        <ServiceCategoryForm
          category={editingCategory}
          isSubmitting={isSubmitting}
          onSubmit={handleSubmit}
          onCancel={handleCancel}
        />
      )}
    </div>
  );
};