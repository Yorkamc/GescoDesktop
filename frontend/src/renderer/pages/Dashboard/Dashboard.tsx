import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { useDashboardStats } from '../../hooks/useDashboardStats';
import { useBackendStatus } from '../../hooks/useBackendStatus';
import { StatCard } from '../../components/StatCard';
import { Alert } from '../../components/Alert';
import { LoadingSpinner } from '../../components/LoadingSpinner';

export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const { stats, isLoading, error, lastUpdate, refreshStats, clearError } = useDashboardStats();
  const { status: backendStatus, checkStatus } = useBackendStatus(false);

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('es-CR', {
      style: 'currency',
      currency: 'CRC',
      minimumFractionDigits: 0
    }).format(amount);
  };

  const formatDate = (date: Date) => {
    return date.toLocaleString('es-CR', {
      hour: '2-digit',
      minute: '2-digit',
      day: '2-digit',
      month: '2-digit'
    });
  };

  const handleRefresh = async () => {
    await refreshStats();
    await checkStatus();
  };

  if (isLoading) {
    return <LoadingSpinner fullScreen message="Cargando dashboard..." />;
  }

  return (
    <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
      {/* Header */}
      <header className="bg-white shadow-sm border-b flex-shrink-0">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center">
              <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center mr-3">
                <span className="text-white font-bold text-sm">G</span>
              </div>
              <div>
                <h1 className="text-xl font-bold text-gray-900">GESCO Desktop</h1>
                <p className="text-xs text-gray-500">
                  Actualizado: {formatDate(lastUpdate)}
                </p>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <button
                onClick={handleRefresh}
                disabled={isLoading}
                className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
                title="Actualizar datos"
              >
                <svg className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
              </button>

              {user && (
                <div className="flex items-center space-x-3">
                  <div className="text-right">
                    <p className="text-sm font-medium text-gray-900">
                      {user.nombreCompleto || user.nombreUsuario}
                    </p>
                    <p className="text-xs text-gray-500">{user.nombreRol}</p>
                  </div>
                  <div className="w-8 h-8 bg-gray-300 rounded-full flex items-center justify-center">
                    <span className="text-sm font-medium text-gray-700">
                      {(user.nombreCompleto || user.nombreUsuario)?.charAt(0) || 'U'}
                    </span>
                  </div>
                </div>
              )}

              <button
                onClick={logout}
                className="text-gray-500 hover:text-gray-700 p-2 rounded-lg hover:bg-gray-100 transition-colors"
                title="Cerrar sesión"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto">
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="px-4 py-6 sm:px-0">
            <div className="mb-8">
              <h2 className="text-2xl font-bold text-gray-900 mb-2">
                Bienvenido{user?.nombreCompleto ? `, ${user.nombreCompleto.split(' ')[0]}` : ''}
              </h2>
              <p className="text-gray-600">
                {stats.reportPeriod || 'Aquí tienes un resumen de la actividad actual'}
              </p>
            </div>

            {error && (
              <Alert
                type="error"
                message={error}
                onRetry={refreshStats}
                onDismiss={clearError}
              />
            )}

            {/* Stats Grid */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
              <StatCard
                title="Actividades"
                value={stats.activeActivities}
                subtitle={`${stats.totalActivities} total`}
                loading={isLoading}
                color="bg-blue-100"
                icon={
                  <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                }
              />

              <StatCard
                title="Ventas Hoy"
                value={formatCurrency(stats.todaySales)}
                subtitle={`${stats.todayTransactions} transacciones`}
                loading={isLoading}
                color="bg-green-100"
                icon={
                  <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                  </svg>
                }
              />

              <StatCard
                title="Ventas Mes"
                value={formatCurrency(stats.monthSales)}
                subtitle={`${stats.monthTransactions} transacciones`}
                loading={isLoading}
                color="bg-purple-100"
                icon={
                  <svg className="w-5 h-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                }
              />

              <StatCard
                title="Productos"
                value={stats.activeProducts}
                subtitle={`${stats.lowStockProducts} agotados`}
                loading={isLoading}
                color="bg-yellow-100"
                icon={
                  <svg className="w-5 h-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                  </svg>
                }
              />
            </div>

            {/* Quick Actions */}
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
              <div className="bg-white overflow-hidden shadow-sm rounded-lg border">
                <div className="px-6 py-4 border-b border-gray-200">
                  <h3 className="text-lg font-medium text-gray-900">Acciones Rápidas</h3>
                </div>
                <div className="p-6">
                  <div className="space-y-3">
                    <button
                      onClick={() => navigate('/license-activation')}
                      className="w-full flex items-center px-4 py-3 text-left bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                    >
                      <svg className="w-5 h-5 text-gray-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-6 6c-1.085 0-2.035-.299-2.886-.737M15 7c0-1.104-.896-2-2-2-1.085 0-2.035.299-2.886.737M15 7v4m-6-2a6 6 0 106 6c1.085 0 2.035-.299 2.886-.737M9 9a2 2 0 012-2c1.085 0 2.035.299 2.886.737" />
                      </svg>
                      <div>
                        <span className="font-medium">Gestionar Licencia</span>
                        <p className="text-sm text-gray-500">Activar o renovar licencia</p>
                      </div>
                    </button>

                    <button 
                      onClick={() => navigate('/activities')}
                      className="w-full flex items-center px-4 py-3 text-left bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
                    >
                      <svg className="w-5 h-5 text-blue-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                      </svg>
                      <div>
                        <span className="font-medium">Gestionar Actividades</span>
                        <p className="text-sm text-gray-500">Ver y crear actividades</p>
                      </div>
                    </button>
                  </div>
                </div>
              </div>

              {/* System Status Panel */}
              <div className="bg-white overflow-hidden shadow-sm rounded-lg border">
                <div className="px-6 py-4 border-b border-gray-200">
                  <h3 className="text-lg font-medium text-gray-900">Estado del Sistema</h3>
                </div>
                <div className="p-6">
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-gray-600">API Backend</span>
                      <div className="flex items-center">
                        <div className={`w-2 h-2 ${backendStatus === 'connected' ? 'bg-green-500' : 'bg-red-500'} rounded-full mr-2`}></div>
                        <span className={`text-sm ${backendStatus === 'connected' ? 'text-green-600' : 'text-red-600'}`}>
                          {backendStatus === 'connected' ? 'Activo' : 'Desconectado'}
                        </span>
                      </div>
                    </div>

                    <div className="pt-4 border-t border-gray-200">
                      <div className="space-y-2 text-xs text-gray-500">
                        <div className="flex justify-between">
                          <span>Última actualización:</span>
                          <span>{formatDate(lastUpdate)}</span>
                        </div>
                        <div className="flex justify-between">
                          <span>Versión:</span>
                          <span>1.0.0</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {backendStatus !== 'connected' && (
              <div className="mt-6">
                <Alert
                  type="warning"
                  message="Backend API no disponible. Verifica que esté ejecutándose en http://localhost:5100"
                />
              </div>
            )}
          </div>
        </div>
      </main>
    </div>
  );
};