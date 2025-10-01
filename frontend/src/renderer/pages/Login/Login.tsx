import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService, healthService } from '../../services/api';

export const Login: React.FC = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [backendStatus, setBackendStatus] = useState<'checking' | 'connected' | 'disconnected'>('checking');
  const [backendRetries, setBackendRetries] = useState(0);

  useEffect(() => {
    checkBackendStatus();
    
    // Escuchar eventos del proceso principal
    const handleBackendConnected = (event: CustomEvent) => {
      console.log('✅ Backend conectado (evento)', event.detail);
      setBackendStatus('connected');
      setBackendRetries(0);
    };
    
    const handleBackendFailed = () => {
      console.log('❌ Backend no conectado (evento)');
      setBackendStatus('disconnected');
    };
    
    window.addEventListener('backend-connected', handleBackendConnected as EventListener);
    window.addEventListener('backend-connection-failed', handleBackendFailed);
    
    return () => {
      window.removeEventListener('backend-connected', handleBackendConnected as EventListener);
      window.removeEventListener('backend-connection-failed', handleBackendFailed);
    };
  }, []);

  const checkBackendStatus = async () => {
    try {
      const health = await healthService.checkBackendConnection();
      if (health.connected) {
        setBackendStatus('connected');
        setBackendRetries(0);
      } else {
        setBackendStatus('disconnected');
      }
    } catch (error) {
      setBackendStatus('disconnected');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Si el backend no está conectado, intentar de todas formas
    // pero mostrar advertencia
    if (backendStatus === 'disconnected') {
      setError('El backend está iniciándose. Espera unos segundos e intenta nuevamente.');
      
      // Reintentar verificar el backend
      setBackendRetries(prev => prev + 1);
      await checkBackendStatus();
      
      if (backendRetries >= 3) {
        setError('El servidor backend no responde. Verifica que esté corriendo en localhost:5100');
      }
      return;
    }
    
    setLoading(true);
    setError('');
    
    try {
      const result = await authService.login(username, password);
      
      if (result.success) {
        if (result.token) {
          localStorage.setItem('token', result.token);
        }
        if (result.usuario) {
          localStorage.setItem('user', JSON.stringify(result.usuario));
        }
        
        navigate('/dashboard');
      } else {
        setError(result.message || 'Error de autenticación');
      }
    } catch (err: any) {
      if (err.message.includes('conectar al servidor')) {
        setBackendStatus('disconnected');
        setError('El servidor backend no responde. Espera unos segundos...');
        
        // Reintentar después de 2 segundos
        setTimeout(checkBackendStatus, 2000);
      } else {
        setError(err.message || 'Error de conexión con el servidor');
      }
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = () => {
    switch (backendStatus) {
      case 'checking': return 'bg-yellow-500';
      case 'connected': return 'bg-green-500';
      case 'disconnected': return 'bg-red-500';
    }
  };

  const getStatusText = () => {
    switch (backendStatus) {
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
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-center">
              <svg className="w-5 h-5 mr-2 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <span className="text-sm">{error}</span>
            </div>
          )}

          {backendStatus === 'checking' && (
            <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 px-4 py-3 rounded-lg flex items-center">
              <svg className="animate-spin h-5 w-5 mr-2 flex-shrink-0" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span className="text-sm">El servidor se está iniciando, por favor espera...</span>
            </div>
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
              disabled={loading || backendStatus === 'checking'}
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
              disabled={loading || backendStatus === 'checking'}
            />
          </div>

          <button
            type="submit"
            disabled={loading || backendStatus === 'checking'}
            className="w-full bg-blue-600 text-white py-3 px-4 rounded-lg font-medium hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
          >
            {loading ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Iniciando sesión...
              </>
            ) : backendStatus === 'checking' ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Esperando servidor...
              </>
            ) : (
              'Iniciar Sesión'
            )}
          </button>
        </form>

        <div className="mt-6 p-4 bg-gray-50 rounded-lg">
          <p className="text-xs text-gray-600 font-semibold mb-1">Credenciales por defecto:</p>
          <div className="flex justify-between text-xs text-gray-600">
            <span>Usuario: <code className="bg-gray-200 px-1 rounded">admin</code></span>
            <span>Contraseña: <code className="bg-gray-200 px-1 rounded">admin123</code></span>
          </div>
        </div>

        <div className="mt-4 text-center">
          <div className="flex items-center justify-center text-xs">
            <div className={`w-2 h-2 ${getStatusColor()} rounded-full mr-2 ${backendStatus === 'checking' ? 'animate-pulse' : ''}`}></div>
            <span className={`${
              backendStatus === 'connected' ? 'text-green-600' : 
              backendStatus === 'checking' ? 'text-yellow-600' : 
              'text-red-600'
            }`}>
              {getStatusText()}
            </span>
          </div>
          
          {backendStatus === 'disconnected' && (
            <button
              onClick={checkBackendStatus}
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