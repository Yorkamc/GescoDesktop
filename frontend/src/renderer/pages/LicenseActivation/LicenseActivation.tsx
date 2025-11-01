import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLicense } from '../../hooks/useLicense';
import { Alert } from '../../components/Alert';
import { InlineSpinner } from '../../components/LoadingSpinner';
import { LicenseStatusCard } from './LicenseStatusCard';

export const LicenseActivation: React.FC = () => {
  const navigate = useNavigate();
  const [activationCode, setActivationCode] = useState('');
  const [organizationId, setOrganizationId] = useState<number>(0);
  
  const {
    licenseStatus,
    isLoading,
    message,
    messageType,
    activateLicense,
    clearMessages
  } = useLicense();

  // ✅ NUEVO: Obtener organizationId automáticamente al montar
  useEffect(() => {
    const orgId = getOrganizationId();
    console.log('🏢 Organization ID obtenido para licencia:', orgId);
    setOrganizationId(orgId);
  }, []);

  // ✅ NUEVO: Función para obtener organizationId
  const getOrganizationId = (): number => {
    console.log('🔍 Buscando organizationId...');
    
    try {
      // Opción 1: Desde localStorage 'user'
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        const orgId = user.organizationId || user.organization_id || user.OrganizationId || '';
        
        if (orgId) {
          // Convertir a número si es necesario
          const numericId = typeof orgId === 'number' ? orgId : parseInt(orgId.split('-')[0], 16) || 1;
          console.log('✅ Organization ID encontrado:', numericId);
          return numericId;
        }
      }

      // Opción 2: Desde JWT token
      const token = localStorage.getItem('token') || localStorage.getItem('accessToken');
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          const orgId = payload.organizationId || payload.organization_id || '';
          
          if (orgId) {
            const numericId = typeof orgId === 'number' ? orgId : parseInt(orgId.split('-')[0], 16) || 1;
            console.log('✅ Organization ID encontrado en token:', numericId);
            return numericId;
          }
        } catch (tokenErr) {
          console.error('❌ Error parseando token:', tokenErr);
        }
      }

      console.warn('⚠️ No se encontró organizationId, usando 1 por defecto');
    } catch (err) {
      console.error('❌ Error obteniendo organizationId:', err);
    }
    
    return 1; // Valor por defecto
  };

  const handleActivate = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!organizationId) {
      alert('Error: No se pudo obtener el ID de organización. Por favor, inicia sesión nuevamente.');
      return;
    }

    console.log('📤 Activando licencia con:', { activationCode, organizationId });
    
    const success = await activateLicense(activationCode, organizationId);
    if (success) {
      setActivationCode('');
    }
  };

  const shouldShowForm = !licenseStatus?.isActive || (licenseStatus?.diasRestantes ?? 0) < 30;

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* ✅ NUEVO: Header consistente con otros componentes */}
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
                <h1 className="text-xl font-bold text-gray-900">Activación de Licencia</h1>
                <p className="text-xs text-gray-500">GESCO Desktop</p>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="flex-1 py-12">
        <div className="max-w-md mx-auto bg-white rounded-xl shadow-lg p-8">
          {/* Icon Header */}
          <div className="text-center mb-8">
            <div className="mx-auto w-16 h-16 bg-indigo-600 rounded-full flex items-center justify-center mb-4">
              <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-6 6c-1.085 0-2.035-.299-2.886-.737M15 7c0-1.104-.896-2-2-2-1.085 0-2.035.299-2.886.737M15 7v4m-6-2a6 6 0 106 6c1.085 0 2.035-.299 2.886.737M9 9a2 2 0 012-2c1.085 0 2.035.299 2.886.737" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Estado de Licencia</h2>
            <p className="text-gray-600 text-sm">Gestiona tu licencia de GESCO</p>
          </div>

          {/* License Status */}
          {licenseStatus && (
            <LicenseStatusCard status={licenseStatus} />
          )}

          {/* Activation Form */}
          {shouldShowForm && (
            <>
              <form onSubmit={handleActivate} className="space-y-6 mt-6">
                {message && (
                  <Alert
                    type={messageType === 'success' ? 'success' : 'error'}
                    message={message}
                    onDismiss={clearMessages}
                  />
                )}

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Código de Activación <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={activationCode}
                    onChange={(e) => setActivationCode(e.target.value.toUpperCase())}
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent font-mono text-center tracking-widest"
                    placeholder="XXXXX-XXXXX-XXXXX"
                    required
                    disabled={isLoading}
                    maxLength={17}
                    autoFocus
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    Ingresa el código de activación proporcionado
                  </p>
                </div>

                {/* ❌ ELIMINADO: Campo de ID de Organización */}
                {/* Ya no se muestra, se obtiene automáticamente */}

                <button
                  type="submit"
                  disabled={isLoading || !activationCode || !organizationId}
                  className="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center transition-colors"
                >
                  {isLoading ? (
                    <>
                      <InlineSpinner className="h-5 w-5 text-white mr-3" />
                      Activando...
                    </>
                  ) : (
                    'Activar Licencia'
                  )}
                </button>
              </form>

              {/* Help text */}
              <div className="mt-6 p-4 bg-blue-50 rounded-lg border border-blue-100">
                <div className="flex items-start gap-2">
                  <svg className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <div>
                    <p className="text-xs text-blue-800 font-semibold mb-1">Modo de desarrollo:</p>
                    <p className="text-xs text-blue-700">
                      Puedes usar cualquier código de activación. El sistema funcionará en modo offline 
                      y te dará 3 meses de licencia de prueba.
                    </p>
                  </div>
                </div>
              </div>
            </>
          )}

          {/* Success State - Navigate to Dashboard */}
          {licenseStatus?.isActive && !shouldShowForm && (
            <div className="mt-6 space-y-4">
              <div className="p-4 bg-green-50 border border-green-200 rounded-lg">
                <div className="flex items-center gap-2">
                  <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <p className="text-sm text-green-800 font-medium">
                    Licencia activa y funcionando correctamente
                  </p>
                </div>
              </div>

              <button
                onClick={() => navigate('/dashboard')}
                className="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-indigo-700 transition-colors flex items-center justify-center gap-2"
              >
                Ir al Dashboard
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7l5 5m0 0l-5 5m5-5H6" />
                </svg>
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};