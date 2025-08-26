import axios from 'axios';

const API_URL = 'http://localhost:5100/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para agregar token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Auth service
export const authService = {
  async login(usuario: string, password: string) {
    const response = await api.post('/auth/login', { usuario, password });
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.usuario));
    }
    return response.data;
  },

  async logout() {
    await api.post('/auth/logout');
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  async validateToken() {
    const token = localStorage.getItem('token');
    if (!token) return false;
    try {
      const response = await api.post('/auth/validate');
      return response.data.valid;
    } catch {
      return false;
    }
  }
};

// Stats service
export const statsService = {
  async getStats() {
    const response = await api.get('/stats');
    return response.data;
  },

  async getTodaySales() {
    const response = await api.get('/sales/today');
    return response.data;
  },

  async getActivities() {
    const response = await api.get('/activities');
    return response.data;
  }
};

// License service
export const licenseService = {
  async getStatus() {
    const response = await api.get('/license/status');
    return response.data;
  },

  async activate(codigoActivacion: string, organizacionId: number) {
    const response = await api.post('/license/activate', {
      codigoActivacion,
      organizacionId
    });
    return response.data;
  }
};

export default api;
