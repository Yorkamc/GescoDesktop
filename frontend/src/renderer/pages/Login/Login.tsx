import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext'; // CAMBIO: Importar del contexto correcto
import { useBackendStatus } from '../../hooks/useBackendStatus';
import { Alert } from '../../components/Alert';
import { InlineSpinner } from '../../components/LoadingSpinner';

export const Login: React.FC = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [retries, setRetries] = useState(0);
  
  // CAMBIO: Ahora usa el contexto correcto
  const { login, isLoading, error, clearError } = useAuth();
  const { status, checkStatus, isConnected, isChecking } = useBackendStatus();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    console.log('🔐 Iniciando proceso de login...');
    
    if (!isConnected) {
      console.warn('⚠️ Backend no conectado, reintentando...');
      if (retries >= 3) {
        console.error('❌ Máximo de reintentos alcanzado');
        return;
      }
      setRetries(prev => prev + 1);
      await checkStatus();
      return;
    }
    
    console.log('✅ Backend conectado, procediendo con login');
    const success = await login(username, password);
    
    if (success) {
      console.log('✅ Login exitoso, navegando a dashboard...');
      navigate('/dashboard');
    } else {
      console.error('❌ Login fallido');
    }
  };

  const getStatusColor = () => {
    switch (status) {
      case 'checking': return 'bg-yellow-500';
      case 'connected': return 'bg-green-500';
      case 'disconnected': return 'bg-red-500';
    }
  };

  const getStatusText = () => {
    switch (status) {
      case 'checking': return 'Iniciando servidor...';
      case 'connected': return 'Servidor conectado';
      case 'disconnected': return 'Servidor no disponible';
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="bg-white rounded-2xl shadow-xl w-full max-w-md p-8">
        <div className="text-center mb-8">
          <div className="mx-auto w-16 h-16 bg-blue-600 rounded-full flex items-center justify-center mb-4">
            <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-gray-800 mb-2">GESCO Desktop</h1>
          <p className="text-gray-600">Sistema de Gestión de Actividades</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <Alert 
              type="error" 
              message={error} 
              onDismiss={clearError}
            />
          )}

          {isChecking && (
            <Alert
              type="warning"
              message="El servidor se está iniciando, por favor espera..."
            />
          )}

          {status === 'disconnected' && retries >= 3 && (
            <Alert
              type="error"
              message="El servidor backend no responde. Verifica que esté corriendo en localhost:5100"
              onRetry={checkStatus}
            />
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Usuario
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Nombre de usuario o email"
              required
              disabled={isLoading || isChecking}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Contraseña
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="••••••••"
              required
              disabled={isLoading || isChecking}
            />
          </div>

          <button
            type="submit"
            disabled={isLoading || isChecking}
            className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {isLoading || isChecking ? (
              <>
                <InlineSpinner className="h-5 w-5 text-white mr-3" />
                {isChecking ? 'Esperando servidor...' : 'Iniciando sesión...'}
              </>
            ) : (
              'Iniciar Sesión'
            )}
          </button>
        </form>

        <div className="mt-4 text-center">
          <div className="flex items-center justify-center text-xs">
            <div className={`w-2 h-2 ${getStatusColor()} rounded-full mr-2 ${isChecking ? 'animate-pulse' : ''}`}></div>
            <span className={`${
              isConnected ? 'text-green-600' : 
              isChecking ? 'text-yellow-600' : 
              'text-red-600'
            }`}>
              {getStatusText()}
            </span>
          </div>
          
          {!isConnected && !isChecking && (
            <button
              onClick={checkStatus}
              className="mt-2 text-xs text-blue-600 hover:text-blue-800 underline"
            >
              Reintentar conexión
            </button>
          )}
        </div>
      </div>
    </div>
  );
};