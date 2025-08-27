import axios, { AxiosError } from 'axios';

const API_URL = 'http://localhost:5100/api';

// Configuración del cliente HTTP
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 segundos timeout
});

// Interceptor para agregar token automáticamente
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    // Log de requests en desarrollo
    console.log(`🔗 API Request: ${config.method?.toUpperCase()} ${config.url}`, config.data);
    return config;
  },
  (error) => {
    console.error('❌ Request error:', error);
    return Promise.reject(error);
  }
);

// Interceptor para manejar respuestas y errores
api.interceptors.response.use(
  (response) => {
    console.log(`✅ API Response: ${response.status}`, response.data);
    return response;
  },
  (error: AxiosError) => {
    console.error('❌ API Error:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      url: error.config?.url
    });

    // Si es 401, limpiar tokens y redirigir al login
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      // Solo redirigir si no estamos ya en login
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
    }

    return Promise.reject(error);
  }
);

// Interfaces
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

interface Stats {
  actividades: number;
  ventasHoy: number;
  transacciones: number;
}

// =====================================================
// AUTH SERVICE
// =====================================================
export const authService = {
  async login(usuario: string, password: string): Promise<LoginResponse> {
    try {
      const response = await api.post('/auth/login', { usuario, password });
      
      if (response.data.success && response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.usuario));
        console.log('✅ Login exitoso, token guardado');
      }
      
      return response.data;
    } catch (error: any) {
      console.error('❌ Login error:', error);
      
      // Manejar errores específicos
      if (error.code === 'ECONNREFUSED') {
        throw new Error('No se puede conectar al servidor. Verifica que el backend esté ejecutándose.');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Usuario o contraseña incorrectos');
      }
      
      throw new Error(error.response?.data?.message || 'Error de conexión con el servidor');
    }
  },

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout');
      console.log('✅ Logout exitoso en servidor');
    } catch (error) {
      console.warn('⚠️ Error en logout del servidor, continuando con logout local');
    } finally {
      // Siempre limpiar tokens localmente
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
      // Limpiar token inválido
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      return false;
    }
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
// STATS SERVICE
// =====================================================
export const statsService = {
  async getStats(): Promise<Stats> {
    try {
      const response = await api.get('/stats');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo estadísticas:', error);
      
      if (error.response?.status === 401) {
        throw new Error('Sesión expirada');
      }
      
      throw new Error('Error al cargar las estadísticas');
    }
  },

  async getTodaySales(): Promise<{ date: string; total: number }> {
    try {
      const response = await api.get('/sales/today');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo ventas del día:', error);
      throw new Error('Error al cargar las ventas del día');
    }
  },

  async getActivities(): Promise<any[]> {
    try {
      const response = await api.get('/activities');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo actividades:', error);
      throw new Error('Error al cargar las actividades');
    }
  }
};

// =====================================================
// HEALTH CHECK
// =====================================================
export const healthService = {
  async checkHealth(): Promise<boolean> {
    try {
      const response = await api.get('/health');
      return response.data.status === 'healthy';
    } catch (error) {
      console.error('❌ Backend no disponible:', error);
      return false;
    }
  }
};

export default api;