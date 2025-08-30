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

interface DashboardStats {
  actividades: number;
  actividadesActivas: number;
  ventasHoy: number;
  transacciones: number;
  ventasMes: number;
  transaccionesMes: number;
  totalUsuarios: number;
  usuariosActivos: number;
  totalProductos: number;
  productosActivos: number;
  productosAgotados: number;
  fechaConsulta: string;
  periodoReporte: string;
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
      if (error.code === 'ECONNREFUSED' || error.code === 'ERR_NETWORK') {
        throw new Error('No se puede conectar al servidor. Verifica que el backend esté ejecutándose en http://localhost:5100');
      }
      
      if (error.response?.status === 401) {
        throw new Error('Usuario o contraseña incorrectos');
      }
      
      if (error.response?.status === 400) {
        throw new Error(error.response.data?.message || 'Datos de login inválidos');
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
// STATS SERVICE (CORREGIDO)
// =====================================================
export const statsService = {
  async getStats(): Promise<DashboardStats> {
    try {
      const response = await api.get('/stats');
      
      // Mapear las propiedades del backend al frontend
      return {
        actividades: response.data.actividades || 0,
        actividadesActivas: response.data.actividadesActivas || 0,
        ventasHoy: response.data.ventasHoy || 0,
        transacciones: response.data.transacciones || 0,
        ventasMes: response.data.ventasMes || 0,
        transaccionesMes: response.data.transaccionesMes || 0,
        totalUsuarios: response.data.totalUsuarios || 0,
        usuariosActivos: response.data.usuariosActivos || 0,
        totalProductos: response.data.totalProductos || 0,
        productosActivos: response.data.productosActivos || 0,
        productosAgotados: response.data.productosAgotados || 0,
        fechaConsulta: response.data.fechaConsulta || new Date().toISOString(),
        periodoReporte: response.data.periodoReporte || 'Período actual'
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
  },

  async getSalesSummary(dias: number = 7): Promise<any[]> {
    try {
      const response = await api.get(`/stats/sales-summary?dias=${dias}`);
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo resumen de ventas:', error);
      throw new Error('Error al cargar el resumen de ventas');
    }
  },

  async getRecentActivities(limite: number = 10): Promise<any[]> {
    try {
      const response = await api.get(`/stats/recent-activities?limite=${limite}`);
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo actividades recientes:', error);
      throw new Error('Error al cargar las actividades recientes');
    }
  }
};

// =====================================================
// SYSTEM SERVICE
// =====================================================
export const systemService = {
  async getHealth(): Promise<any> {
    try {
      const response = await api.get('/system/health');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error verificando salud del sistema:', error);
      throw new Error('Error en health check del sistema');
    }
  },

  async getSystemStats(): Promise<any> {
    try {
      const response = await api.get('/system/stats');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo estadísticas del sistema:', error);
      throw new Error('Error al cargar estadísticas del sistema');
    }
  },

  async getSystemInfo(): Promise<any> {
    try {
      const response = await api.get('/system/info');
      return response.data;
    } catch (error: any) {
      console.error('❌ Error obteniendo información del sistema:', error);
      throw new Error('Error al cargar información del sistema');
    }
  }
};

// =====================================================
// HEALTH CHECK
// =====================================================
export const healthService = {
  async checkHealth(): Promise<boolean> {
    try {
      const response = await api.get('/system/health');
      return response.data.status === 'healthy';
    } catch (error) {
      console.error('❌ Backend no disponible:', error);
      return false;
    }
  },

  async checkBackendConnection(): Promise<{ 
    connected: boolean; 
    message: string; 
    latency?: number 
  }> {
    const startTime = Date.now();
    try {
      const response = await api.get('/system/health');
      const latency = Date.now() - startTime;
      
      return {
        connected: response.data.status === 'healthy',
        message: 'Conectado al backend',
        latency
      };
    } catch (error: any) {
      return {
        connected: false,
        message: error.code === 'ECONNREFUSED' 
          ? 'Backend no disponible en http://localhost:5100' 
          : 'Error de conexión con el backend'
      };
    }
  }
};

// =====================================================
// UTILITIES
// =====================================================
export const apiUtils = {
  formatError(error: any): string {
    if (error.response?.data?.message) {
      return error.response.data.message;
    }
    
    if (error.message) {
      return error.message;
    }
    
    switch (error.code) {
      case 'ECONNREFUSED':
        return 'No se puede conectar al servidor. Verifica que esté ejecutándose.';
      case 'ERR_NETWORK':
        return 'Error de red. Verifica tu conexión.';
      case 'TIMEOUT':
        return 'La petición tardó demasiado. Intenta de nuevo.';
      default:
        return 'Error desconocido del servidor.';
    }
  },

  isNetworkError(error: any): boolean {
    return ['ECONNREFUSED', 'ERR_NETWORK', 'TIMEOUT'].includes(error.code);
  },

  isAuthError(error: any): boolean {
    return error.response?.status === 401;
  }
};

export default api;