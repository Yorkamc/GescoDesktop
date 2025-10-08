import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { statsService, authService, healthService } from '../../services/api';

interface DashboardStats {
  totalActivities: number;
  activeActivities: number;
  todaySales: number;
  todayTransactions: number;
  monthSales: number;
  monthTransactions: number;
  totalUsers: number;
  activeUsers: number;
  totalProducts: number;
  activeProducts: number;
  lowStockProducts: number;
  queryDate: string;
  reportPeriod: string;
}

interface User {
  id: string;
  nombreUsuario: string;
  nombreCompleto: string;
  nombreRol: string;
}

interface SystemStatus {
  database: boolean;
  api: boolean;
  sync: boolean;
}

export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<DashboardStats>({
    totalActivities: 0,
    activeActivities: 0,
    todaySales: 0,
    todayTransactions: 0,
    monthSales: 0,
    monthTransactions: 0,
    totalUsers: 0,
    activeUsers: 0,
    totalProducts: 0,
    activeProducts: 0,
    lowStockProducts: 0,
    queryDate: '',
    reportPeriod: ''
  });
  
  const [user, setUser] = useState<User | null>(null);
  const [systemStatus, setSystemStatus] = useState<SystemStatus>({
    database: true,
    api: true,
    sync: false
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [lastUpdate, setLastUpdate] = useState<Date>(new Date());

  useEffect(() => {
    loadDashboardData();
    loadUserData();
    checkSystemStatus();
    
    const interval = setInterval(() => {
      loadDashboardData();
      checkSystemStatus();
    }, 5 * 60 * 1000);

    return () => clearInterval(interval);
  }, []);

  const loadDashboardData = async () => {
    try {
      console.log('🔄 Cargando estadísticas del dashboard...');
      const data = await statsService.getStats();
      setStats(data);
      setLastUpdate(new Date());
      setError('');
      console.log('✅ Estadísticas cargadas:', data);
    } catch (err: any) {
      setError('Error cargando estadísticas: ' + err.message);
      console.error('Stats error:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadUserData = () => {
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        setUser(JSON.parse(userData));
      } catch (e) {
        console.error('Error parsing user data:', e);
      }
    }
  };

  const checkSystemStatus = async () => {
    try {
      const healthCheck = await healthService.checkBackendConnection();
      setSystemStatus({
        database: true,
        api: healthCheck.connected,
        sync: false
      });
    } catch (error) {
      setSystemStatus({
        database: false,
        api: false,
        sync: false
      });
    }
  };

  const handleLogout = async () => {
    try {
      await authService.logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      navigate('/login');
    }
  };

  const handleRefresh = async () => {
    setLoading(true);
    await loadDashboardData();
    await checkSystemStatus();
  };

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

  const getStatusColor = (status: boolean) => {
    return status ? 'bg-green-500' : 'bg-red-500';
  };

  const getStatusText = (status: boolean) => {
    return status ? 'Activo' : 'Desconectado';
  };

  const StatCard: React.FC<{
    title: string;
    value: string | number;
    icon: React.ReactNode;
    color: string;
    loading?: boolean;
    subtitle?: string;
  }> = ({ title, value, icon, color, loading, subtitle }) => (
    <div className="bg-white overflow-hidden shadow-sm rounded-lg border hover:shadow-md transition-shadow">
      <div className="p-6">
        <div className="flex items-center">
          <div className="flex-shrink-0">
            <div className={`w-8 h-8 ${color} rounded-lg flex items-center justify-center`}>
              {icon}
            </div>
          </div>
          <div className="ml-4 flex-1">
            <p className="text-sm font-medium text-gray-900">{title}</p>
            <div className="flex items-baseline">
              <p className={`text-2xl font-bold ${color.replace('bg-', 'text-').replace('-100', '-600')}`}>
                {loading ? (
                  <div className="animate-pulse bg-gray-200 h-8 w-16 rounded"></div>
                ) : (
                  typeof value === 'number' && value > 999 ? value.toLocaleString() : value
                )}
              </p>
            </div>
            {subtitle && (
              <p className="text-xs text-gray-500 mt-1">{subtitle}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );

  return (
    <div className="h-screen bg-gray-50 flex flex-col overflow-hidden">
      {/* Header - Fixed */}
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
                disabled={loading}
                className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors disabled:opacity-50"
                title="Actualizar datos"
              >
                <svg className={`w-5 h-5 ${loading ? 'animate-spin' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
                onClick={handleLogout}
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

      {/* Main Content - Scrollable */}
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
              <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-center">
                <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
                {error}
                <button 
                  onClick={handleRefresh}
                  className="ml-auto text-red-600 hover:text-red-800 underline"
                >
                  Reintentar
                </button>
              </div>
            )}

            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 mb-8">
              <StatCard
                title="Actividades"
                value={stats.activeActivities}
                subtitle={`${stats.totalActivities} total`}
                loading={loading}
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
                loading={loading}
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
                loading={loading}
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
                loading={loading}
                color="bg-yellow-100"
                icon={
                  <svg className="w-5 h-5 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                  </svg>
                }
              />
            </div>

            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-2 mb-8">
              <StatCard
                title="Usuarios"
                value={stats.activeUsers}
                subtitle={`${stats.totalUsers} registrados`}
                loading={loading}
                color="bg-indigo-100"
                icon={
                  <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
                  </svg>
                }
              />

              <StatCard
                title="Inventario"
                value={stats.totalProducts}
                subtitle={`${stats.activeProducts} disponibles`}
                loading={loading}
                color="bg-pink-100"
                icon={
                  <svg className="w-5 h-5 text-pink-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
                  </svg>
                }
              />
            </div>

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

                    <button 
                      className="w-full flex items-center px-4 py-3 text-left bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                      disabled
                    >
                      <svg className="w-5 h-5 text-gray-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                      </svg>
                      <div>
                        <span className="font-medium">Ver Reportes</span>
                        <p className="text-sm text-gray-500">Análisis y estadísticas</p>
                      </div>
                    </button>

                    <button 
                      onClick={() => window.open('http://localhost:5100/swagger', '_blank')}
                      className="w-full flex items-center px-4 py-3 text-left bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
                    >
                      <svg className="w-5 h-5 text-blue-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
                      </svg>
                      <div>
                        <span className="font-medium">API Documentation</span>
                        <p className="text-sm text-gray-500">Swagger/OpenAPI</p>
                      </div>
                    </button>
                  </div>
                </div>
              </div>

              <div className="bg-white overflow-hidden shadow-sm rounded-lg border">
                <div className="px-6 py-4 border-b border-gray-200">
                  <h3 className="text-lg font-medium text-gray-900">Estado del Sistema</h3>
                </div>
                <div className="p-6">
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-gray-600">Base de Datos Local</span>
                      <div className="flex items-center">
                        <div className={`w-2 h-2 ${getStatusColor(systemStatus.database)} rounded-full mr-2`}></div>
                        <span className={`text-sm ${systemStatus.database ? 'text-green-600' : 'text-red-600'}`}>
                          {getStatusText(systemStatus.database)}
                        </span>
                      </div>
                    </div>

                    <div className="flex items-center justify-between">
                      <span className="text-sm text-gray-600">API Backend</span>
                      <div className="flex items-center">
                        <div className={`w-2 h-2 ${getStatusColor(systemStatus.api)} rounded-full mr-2`}></div>
                        <span className={`text-sm ${systemStatus.api ? 'text-green-600' : 'text-red-600'}`}>
                          {getStatusText(systemStatus.api)}
                        </span>
                      </div>
                    </div>

                    <div className="flex items-center justify-between">
                      <span className="text-sm text-gray-600">Sincronización</span>
                      <div className="flex items-center">
                        <div className={`w-2 h-2 ${getStatusColor(systemStatus.sync)} rounded-full mr-2`}></div>
                        <span className={`text-sm ${systemStatus.sync ? 'text-green-600' : 'text-yellow-600'}`}>
                          {systemStatus.sync ? 'Online' : 'Modo Offline'}
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
                        <div className="flex justify-between">
                          <span>Modo:</span>
                          <span>Desarrollo</span>
                        </div>
                      </div>
                    </div>

                    <div className="pt-4 border-t border-gray-200">
                      <button
                        onClick={handleRefresh}
                        disabled={loading}
                        className="text-blue-600 hover:text-blue-800 text-sm font-medium flex items-center disabled:opacity-50"
                      >
                        <svg className={`w-4 h-4 mr-1 ${loading ? 'animate-spin' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                        </svg>
                        {loading ? 'Actualizando...' : 'Actualizar datos'}
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {systemStatus.api === false && (
              <div className="mt-6 bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                <div className="flex">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                      <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <h3 className="text-sm font-medium text-yellow-800">
                      Backend API no disponible
                    </h3>
                    <div className="mt-2 text-sm text-yellow-700">
                      <p>
                        No se puede conectar con el servidor backend. Verifica que esté ejecutándose en
                        <code className="bg-yellow-100 px-1 py-0.5 rounded ml-1">http://localhost:5100</code>
                      </p>
                      <p className="mt-2">
                        Para iniciarlo: <code className="bg-yellow-100 px-1 py-0.5 rounded">cd backend && dotnet run</code>
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </main>
    </div>
  );
};