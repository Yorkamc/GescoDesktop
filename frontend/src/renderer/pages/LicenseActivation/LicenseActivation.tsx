import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { licenseService } from '../../services/api';

interface LicenseStatus {
  isActive: boolean;
  message: string;
  fechaActivacion?: string;
  fechaExpiracion?: string;
  diasRestantes: number;
  maxUsuarios: number;
  organizacionId?: number;
}

export const LicenseActivation: React.FC = () => {
  const navigate = useNavigate();
  const [activationCode, setActivationCode] = useState('');
  const [organizationId, setOrganizationId] = useState(1);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState<'success' | 'error' | ''>('');
  const [licenseStatus, setLicenseStatus] = useState<LicenseStatus | null>(null);

  useEffect(() => {
    checkLicenseStatus();
  }, []);

  const checkLicenseStatus = async () => {
    try {
      const status = await licenseService.getStatus();
      setLicenseStatus(status);
    } catch (error) {
      console.error('Error checking license status:', error);
    }
  };

  const handleActivate = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');
    
    try {
      const result = await licenseService.activate(activationCode, organizationId);
      
      if (result.success) {
        setMessage(result.message || 'Licencia activada correctamente');
        setMessageType('success');
        setActivationCode('');
        await checkLicenseStatus();
      } else {
        setMessage(result.message || 'Error al activar la licencia');
        setMessageType('error');
      }
    } catch (error: any) {
      setMessage('Error de conexión con el servidor');
      setMessageType('error');
      console.error('Activation error:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

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
            <div className={`mb-6 p-4 rounded-lg border ${
              licenseStatus.isActive 
                ? 'bg-green-50 border-green-200' 
                : 'bg-yellow-50 border-yellow-200'
            }`}>
              <div className="flex items-center mb-2">
                {licenseStatus.isActive ? (
                  <svg className="w-5 h-5 text-green-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                  </svg>
                ) : (
                  <svg className="w-5 h-5 text-yellow-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                  </svg>
                )}
                <span className={`font-semibold ${
                  licenseStatus.isActive ? 'text-green-800' : 'text-yellow-800'
                }`}>
                  {licenseStatus.message}
                </span>
              </div>
              
              {licenseStatus.isActive && (
                <div className="text-sm text-gray-600 space-y-1">
                  {licenseStatus.fechaExpiracion && (
                    <p>Expira: {formatDate(licenseStatus.fechaExpiracion)}</p>
                  )}
                  <p>Días restantes: <span className="font-semibold">{licenseStatus.diasRestantes}</span></p>
                  <p>Máximo usuarios: <span className="font-semibold">{licenseStatus.maxUsuarios}</span></p>
                </div>
              )}
            </div>
          )}

          {/* Activation Form */}
          {(!licenseStatus?.isActive || licenseStatus?.diasRestantes < 30) && (
            <>
              <form onSubmit={handleActivate} className="space-y-6">
                {message && (
                  <div className={`p-4 rounded-lg border ${
                    messageType === 'success' 
                      ? 'bg-green-50 border-green-200 text-green-700' 
                      : 'bg-red-50 border-red-200 text-red-700'
                  }`}>
                    {message}
                  </div>
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
                    disabled={loading}
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
                    disabled={loading}
                  />
                </div>

                <button
                  type="submit"
                  disabled={loading || !activationCode}
                  className="w-full bg-indigo-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
                >
                  {loading ? (
                    <>
                      <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
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