import axios, { AxiosError } from 'axios';

const API_URL = 'http://localhost:5100/api';

// Verificar conectividad al inicializar
console.log('🔧 Inicializando cliente API para:', API_URL);

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000, // Aumentar timeout para debugging
});

// Interceptor mejorado para requests
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`, {
      data: config.data,
      headers: config.headers
    });
    return config;
  },
  (error) => {
    console.error('❌ Request error:', error);
    return Promise.reject(error);
  }
);

// Interceptor mejorado para responses
api.interceptors.response.use(
  (response) => {
    console.log(`✅ API Response: ${response.status} ${response.statusText}`, {
      url: response.config.url,
      data: response.data
    });
    return response;
  },
  (error: AxiosError) => {
    console.error('❌ API Error Details:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      url: error.config?.url,
      message: error.message,
      code: error.code
    });

    // Diagnóstico específico para errores comunes
    if (error.code === 'ERR_NETWORK') {
      console.error('🌐 PROBLEMA DE RED:');
      console.error('   - Backend no está corriendo en localhost:5100');
      console.error('   - Firewall bloquea la conexión');
      console.error('   - CORS mal configurado');
    }

    if (error.code === 'ECONNREFUSED') {
      console.error('🚫 CONEXIÓN RECHAZADA:');
      console.error('   - Backend no responde en puerto 5100');
      console.error('   - Servicio no iniciado');
    }

    if (error.response?.status === 401) {
      console.error('🔐 ERROR DE AUTENTICACIÓN');
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      if (!window.location.pathname.includes('/login')) {
        console.log('📍 Redirigiendo al login...');
        window.location.href = '#/login'; // HashRouter compatible
      }
    }

    return Promise.reject(error);
  }
);

// Interfaces (mantener las existentes)
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
// AUTH SERVICE - CON DEBUGGING MEJORADO
// =====================================================
export const authService = {
  async login(usuario: string, password: string): Promise<LoginResponse> {
    try {
      console.log('🔐 Iniciando login para:', usuario);
      
      // Test de conectividad previo
      try {
        const healthCheck = await api.get('/system/health', { timeout: 5000 });
        console.log('✅ Backend disponible:', healthCheck.data);
      } catch (healthError) {
        console.error('❌ Backend no disponible:', healthError);
        throw new Error('No se puede conectar al servidor. Verifica que esté corriendo en http://localhost:5100');
      }

      const response = await api.post('/auth/login', { usuario, password });
      
      if (response.data.success && response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.usuario));
        console.log('✅ Login exitoso, token guardado');
        console.log('👤 Usuario:', response.data.usuario);
      }
      
      return response.data;
    } catch (error: any) {
      console.error('❌ Login failed:', error);
      
      // Diagnóstico específico de errores
      if (error.code === 'ERR_NETWORK') {
        throw new Error('No se puede conectar al servidor. Asegúrate de que el backend esté corriendo en http://localhost:5100');
      }
      
      if (error.code === 'ECONNREFUSED') {
        throw new Error('Servidor no disponible. Inicia el backend con: dotnet run');
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
// HEALTH SERVICE - NUEVO
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

// Resto de servicios (mantener los existentes)
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

export const statsService = {
  async getStats(): Promise<DashboardStats> {
    try {
      const response = await api.get('/stats');
      
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