import axios, { AxiosError } from 'axios';

const API_URL = 'http://localhost:5100/api';

console.log('🔧 Inicializando cliente API para:', API_URL);

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

// Interceptor para requests
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`, config.data);
    return config;
  },
  (error) => {
    console.error('❌ Request error:', error);
    return Promise.reject(error);
  }
);

// Interceptor para responses
api.interceptors.response.use(
  (response) => {
    console.log(`✅ API Response: ${response.status} ${response.statusText}`, response.data);
    return response;
  },
  (error: AxiosError) => {
    console.error('❌ API Error Details:', error);

    if (error.code === 'ERR_NETWORK') {
      console.error('🌐 PROBLEMA DE RED: Backend no está corriendo en localhost:5100');
    }

    if (error.response?.status === 401) {
      console.error('🔐 ERROR DE AUTENTICACIÓN');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '#/login';
      }
    }

    return Promise.reject(error);
  }
);

// =====================================================
// INTERFACES COMPLETAS
// =====================================================
interface LoginResponse {
  success: boolean;
  message?: string;
  usuario?: any;
  token?: string;
  isOffline?: boolean;
  tokenExpiration?: string;
}

interface ActivationResponse {
  success: boolean;
  message?: string;
  fechaExpiracion?: string;
  diasRestantes?: number;
}

interface LicenseStatus {
  isActive: boolean;
  message: string;
  fechaActivacion?: string;
  fechaExpiracion?: string;
  diasRestantes: number;
  maxUsuarios: number;
  organizacionId?: number;
}

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

// INTERFACES PARA ACTIVIDADES
interface Activity {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
  location?: string;
  activityStatusId: number;
  statusName?: string;
  managerUserId?: string;
  managerName?: string;
  organizationId?: string;
  organizationName?: string;
  createdAt: string;
}

interface CreateActivityRequest {
  name: string;
  description?: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
  location?: string;
  activityStatusId?: number;
  managerUserId?: string;
  organizationId?: string;
}

interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
  timestamp: string;
}

// =====================================================
// AUTH SERVICE
// =====================================================
export const authService = {
  async login(usuario: string, password: string): Promise<LoginResponse> {
    try {
      console.log('🔐 Iniciando login para:', usuario);
      
      const response = await api.post('/auth/login', { usuario, password });
      
      if (response.data.success && response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.usuario));
        console.log('✅ Login exitoso, token guardado');
      }
      
      return response.data;
    } catch (error: any) {
      console.error('❌ Login failed:', error);
      
      if (error.code === 'ERR_NETWORK') {
        throw new Error('No se puede conectar al servidor. Asegúrate de que el backend esté corriendo en http://localhost:5100');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Usuario o contraseña incorrectos');
      }
      
      if (error.response?.status === 400) {
        throw new Error(error.response.data?.message || 'Datos de login inválidos');
      }

      if (error.response?.status >= 500) {
        throw new Error('Error interno del servidor');
      }
      
      throw new Error(error.response?.data?.message || error.message || 'Error desconocido en el login');
    }
  },

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
      console.log('✅ Logout exitoso en servidor');
    } catch (error) {
      console.warn('⚠️ Error en logout del servidor:', error);
    } finally {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      console.log('✅ Tokens locales eliminados');
    }
  },

  async validateToken(): Promise<boolean> {
    const token = localStorage.getItem('token');
    if (!token) {
      console.log('📝 No hay token para validar');
      return false;
    }

    try {
      const response = await api.post('/auth/validate');
      console.log('✅ Token válido');
      return response.data.valid === true;
    } catch (error) {
      console.warn('⚠️ Token inválido o expirado');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      return false;
    }
  }
};

// =====================================================
// HEALTH SERVICE
// =====================================================
export const healthService = {
  async checkBackendConnection(): Promise<{ 
    connected: boolean; 
    message: string; 
    latency?: number 
  }> {
    const startTime = Date.now();
    try {
      console.log('🏥 Verificando conexión con backend...');
      const response = await api.get('/system/health', { timeout: 5000 });
      const latency = Date.now() - startTime;
      
      console.log(`✅ Backend conectado (${latency}ms):`, response.data);
      
      return {
        connected: response.data.status === 'healthy',
        message: 'Conectado al backend',
        latency
      };
    } catch (error: any) {
      const latency = Date.now() - startTime;
      console.error(`❌ Backend no conectado (${latency}ms):`, error.message);
      
      return {
        connected: false,
        message: error.code === 'ECONNREFUSED' 
          ? 'Backend no disponible en http://localhost:5100' 
          : `Error de conexión: ${error.message}`
      };
    }
  },

  async checkHealth(): Promise<boolean> {
    const result = await this.checkBackendConnection();
    return result.connected;
  }
};

// =====================================================
// LICENSE SERVICE
// =====================================================
export const licenseService = {
  async getStatus(): Promise<LicenseStatus> {
    try {
      const response = await api.get('/license/status');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo estado de licencia:', error);
      throw new Error('Error al consultar el estado de la licencia');
    }
  },

  async activate(codigoActivacion: string, organizacionId: number): Promise<ActivationResponse> {
    try {
      const response = await api.post('/license/activate', {
        codigoActivacion,
        organizacionId
      });
      
      return response.data;
    } catch (error: any) {
      console.error('❌ Error activando licencia:', error);
      
      if (error.response?.status === 400) {
        throw new Error(error.response.data.message || 'Datos de activación inválidos');
      }
      
      throw new Error('Error al activar la licencia');
    }
  }
};

// =====================================================
// STATS SERVICE CORREGIDO
// =====================================================
export const statsService = {
  async getStats(): Promise<DashboardStats> {
    try {
      const response = await api.get('/stats');
      
      // Mapear las propiedades del backend (camelCase) al frontend
      return {
        totalActivities: response.data.totalActivities || 0,
        activeActivities: response.data.activeActivities || 0,
        todaySales: response.data.todaySales || 0,
        todayTransactions: response.data.todayTransactions || 0,
        monthSales: response.data.monthSales || 0,
        monthTransactions: response.data.monthTransactions || 0,
        totalUsers: response.data.totalUsers || 0,
        activeUsers: response.data.activeUsers || 0,
        totalProducts: response.data.totalProducts || 0,
        activeProducts: response.data.activeProducts || 0,
        lowStockProducts: response.data.lowStockProducts || 0,
        queryDate: response.data.queryDate || new Date().toISOString(),
        reportPeriod: response.data.reportPeriod || 'Período actual'
      };
    } catch (error: any) {
      console.error('❌ Error obteniendo estadísticas:', error);
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      if (error.code === 'ECONNREFUSED' || error.code === 'ERR_NETWORK') {
        throw new Error('No se puede conectar al servidor de estadísticas');
      }
      
      throw new Error('Error al cargar las estadísticas');
    }
  }
};

// =====================================================
// ACTIVITIES SERVICE - CORREGIDO PARA FECHAS
// =====================================================
export const activitiesService = {
  async getActivities(organizationId?: string): Promise<Activity[]> {
    try {
      const params = organizationId ? { organizationId } : {};
      const response = await api.get('/activities', { params });
      
      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo actividades');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo actividades:', error);
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cargar las actividades');
    }
  },

  async getActivity(id: string): Promise<Activity> {
    try {
      const response = await api.get(`/activities/${id}`);
      
      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Actividad no encontrada');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo actividad:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Actividad no encontrada');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cargar la actividad');
    }
  },

  async createActivity(activity: CreateActivityRequest): Promise<Activity> {
    try {
      // Asegurar formato correcto de datos
      const activityData = {
        name: activity.name.trim(),
        description: activity.description?.trim() || '',
        startDate: activity.startDate, // Ya debe venir en formato YYYY-MM-DD
        startTime: activity.startTime || undefined,
        endDate: activity.endDate || undefined,
        endTime: activity.endTime || undefined,
        location: activity.location?.trim() || '',
        activityStatusId: activity.activityStatusId || 1,
        managerUserId: activity.managerUserId || undefined,
        organizationId: activity.organizationId || undefined
      };

      console.log('📝 Enviando datos de actividad:', activityData);

      const response = await api.post('/activities', activityData);
      
      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error creando actividad');
      }
    } catch (error: any) {
      console.error('❌ Error creando actividad:', error);
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al crear la actividad');
    }
  },

  async updateActivity(id: string, activity: CreateActivityRequest): Promise<Activity> {
    try {
      // Asegurar formato correcto de datos
      const activityData = {
        name: activity.name.trim(),
        description: activity.description?.trim() || '',
        startDate: activity.startDate,
        startTime: activity.startTime || undefined,
        endDate: activity.endDate || undefined,
        endTime: activity.endTime || undefined,
        location: activity.location?.trim() || '',
        activityStatusId: activity.activityStatusId || 1,
        managerUserId: activity.managerUserId || undefined,
        organizationId: activity.organizationId || undefined
      };

      console.log('📝 Actualizando actividad:', activityData);

      const response = await api.put(`/activities/${id}`, activityData);
      
      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error actualizando actividad');
      }
    } catch (error: any) {
      console.error('❌ Error actualizando actividad:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Actividad no encontrada');
      }
      
      if (error.response?.status === 400) {
        const errors = error.response.data.errors || [error.response.data.message];
        throw new Error(`Datos inválidos: ${errors.join(', ')}`);
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al actualizar la actividad');
    }
  },

  async deleteActivity(id: string): Promise<void> {
    try {
      const response = await api.delete(`/activities/${id}`);
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Error eliminando actividad');
      }
    } catch (error: any) {
      console.error('❌ Error eliminando actividad:', error);
      
      if (error.response?.status === 404) {
        throw new Error('Actividad no encontrada');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al eliminar la actividad');
    }
  },

  async getActiveActivities(): Promise<Activity[]> {
    try {
      const response = await api.get('/activities/active');
      
      if (response.data.success) {
        return response.data.data || [];
      } else {
        throw new Error(response.data.message || 'Error obteniendo actividades activas');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo actividades activas:', error);
      throw new Error('Error al cargar las actividades activas');
    }
  },

  async getActivityStats(): Promise<DashboardStats> {
    try {
      const response = await api.get('/activities/stats');
      
      if (response.data.success) {
        return response.data.data;
      } else {
        throw new Error(response.data.message || 'Error obteniendo estadísticas');
      }
    } catch (error: any) {
      console.error('❌ Error obteniendo estadísticas de actividades:', error);
      throw new Error('Error al cargar las estadísticas');
    }
  }
};

// Test inicial de conectividad
setTimeout(async () => {
  console.log('🔍 Probando conectividad inicial con backend...');
  const health = await healthService.checkBackendConnection();
  if (health.connected) {
    console.log('✅ Conectividad inicial exitosa');
  } else {
    console.warn('⚠️ Sin conectividad inicial con backend:', health.message);
  }
}, 1000);

export default api;