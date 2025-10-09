import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLicense } from '../../hooks/useLicense';
import { Alert } from '../../components/Alert';
import { InlineSpinner } from '../../components/LoadingSpinner';
import { LicenseStatusCard } from './LicenseStatusCard';

export const LicenseActivation: React.FC = () => {
  const navigate = useNavigate();
  const [activationCode, setActivationCode] = useState('');
  const [organizationId, setOrganizationId] = useState(1);
  
  const {
    licenseStatus,
    isLoading,
    message,
    messageType,
    activateLicense,
    clearMessages
  } = useLicense();

  const handleActivate = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const success = await activateLicense(activationCode, organizationId);
    if (success) {
      setActivationCode('');
    }
  };

  const shouldShowForm = !licenseStatus?.isActive || (licenseStatus?.diasRestantes ?? 0) < 30;

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="py-12">
        <div className="max-w-md mx-auto bg-white rounded-xl shadow-lg p-8">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="mx-auto w-16 h-16 bg-indigo-600 rounded-full flex items-center justify-center mb-4">
              <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-6 6c-1.085 0-2.035-.299-2.886-.737M15 7c0-1.104-.896-2-2-2-1.085 0-2.035.299-2.886.737M15 7v4m-6-2a6 6 0 106 6c1.085 0 2.035-.299 2.886-.737M9 9a2 2 0 012-2c1.085 0 2.035.299 2.886.737" />
              </svg>
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Activación de Licencia</h1>
            <p className="text-gray-600">GESCO Desktop</p>
          </div>

          {/* License Status */}
          {licenseStatus && (
            <LicenseStatusCard status={licenseStatus} />
          )}

          {/* Activation Form */}
          {shouldShowForm && (
            <>
              <form onSubmit={handleActivate} className="space-y-6">
                {message && (
                  <Alert
                    type={messageType === 'success' ? 'success' : 'error'}
                    message={message}
                    onDismiss={clearMessages}
                  />
                )}

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Código de Activación
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
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    ID de Organización
                  </label>
                  <input
                    type="number"
                    value={organizationId}
                    onChange={(e) => setOrganizationId(parseInt(e.target.value) || 1)}
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    min={1}
                    required
                    disabled={isLoading}
                  />
                </div>

                <button
                  type="submit"
                  disabled={isLoading || !activationCode}
                  className="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
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
              <div className="mt-6 p-4 bg-blue-50 rounded-lg">
                <p className="text-xs text-blue-800 font-semibold mb-2">Para desarrollo:</p>
                <p className="text-xs text-blue-700">
                  Puedes usar cualquier código de activación. El sistema funcionará en modo offline 
                  y te dará 3 meses de licencia de prueba.
                </p>
              </div>
            </>
          )}

          {/* Navigation */}
          <div className="mt-8 text-center space-y-2">
            {licenseStatus?.isActive && (
              <button
                onClick={() => navigate('/dashboard')}
                className="text-indigo-600 hover:text-indigo-800 font-medium"
              >
                Ir al Dashboard →
              </button>
            )}
            <br />
            <button
              onClick={() => navigate('/login')}
              className="text-gray-500 hover:text-gray-700 text-sm"
            >
              ← Volver al Login
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};