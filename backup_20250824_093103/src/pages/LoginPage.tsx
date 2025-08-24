import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [credentials, setCredentials] = useState({
    usuario: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await authService.login(credentials.usuario, credentials.password);
      if (response.success) {
        navigate('/dashboard');
      } else {
        setError(response.message || 'Error al iniciar sesión');
      }
    } catch (err: any) {
      setError(err.message || 'Error de conexión');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="card w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">GESCO Desktop</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Sistema de Gestión de Actividades</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium mb-2">
              Usuario o Email
            </label>
            <input
              type="text"
              className="input-field"
              value={credentials.usuario}
              onChange={(e) => setCredentials({...credentials, usuario: e.target.value})}
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-2">
              Contraseña
            </label>
            <input
              type="password"
              className="input-field"
              value={credentials.password}
              onChange={(e) => setCredentials({...credentials, password: e.target.value})}
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full btn-primary"
          >
            {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
          </button>
        </form>

        <div className="mt-6 text-center">
          <button
            onClick={() => navigate('/activation')}
            className="text-primary-600 hover:text-primary-700 text-sm"
          >
            ¿Necesitas activar tu licencia?
          </button>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
