import React from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';

const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const user = authService.getUser();

  const handleLogout = () => {
    authService.logout();
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <nav className="bg-white dark:bg-gray-800 shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-semibold">GESCO Desktop</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-gray-700 dark:text-gray-300">
                {user?.nombreCompleto || user?.nombreUsuario}
              </span>
              <button
                onClick={handleLogout}
                className="btn-secondary"
              >
                Cerrar Sesión
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            <div className="card">
              <h2 className="text-lg font-semibold mb-2">Actividades</h2>
              <p className="text-3xl font-bold text-primary-600">0</p>
              <p className="text-gray-600 dark:text-gray-400">Activas este mes</p>
            </div>
            
            <div className="card">
              <h2 className="text-lg font-semibold mb-2">Ventas</h2>
              <p className="text-3xl font-bold text-green-600">0</p>
              <p className="text-gray-600 dark:text-gray-400">Total del día</p>
            </div>
            
            <div className="card">
              <h2 className="text-lg font-semibold mb-2">Licencia</h2>
              <p className="text-3xl font-bold text-blue-600">Activa</p>
              <p className="text-gray-600 dark:text-gray-400">Válida hasta: --</p>
            </div>
          </div>

          <div className="mt-8 card">
            <h2 className="text-xl font-semibold mb-4">Acciones Rápidas</h2>
            <div className="grid grid-cols-2 gap-4">
              <button className="btn-primary">
                Nueva Actividad
              </button>
              <button className="btn-primary">
                Abrir Caja
              </button>
              <button className="btn-secondary">
                Ver Inventario
              </button>
              <button className="btn-secondary">
                Reportes
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default DashboardPage;
