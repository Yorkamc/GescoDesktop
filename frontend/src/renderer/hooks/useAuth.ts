import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/api';

interface User {
  id: string;
  nombreUsuario: string;
  nombreCompleto: string;
  nombreRol: string;
  correo: string;
  organizacionId: string;
  rolId: string;
}

interface UseAuthReturn {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => Promise<void>;
  validateToken: () => Promise<boolean>;
  clearError: () => void;
}

export const useAuth = (): UseAuthReturn => {
  const navigate = useNavigate();
  const [user, setUser] = useState<User | null>(() => {
    const userData = localStorage.getItem('user');
    return userData ? JSON.parse(userData) : null;
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const login = useCallback(async (username: string, password: string): Promise<boolean> => {
    setIsLoading(true);
    setError('');
    
    try {
      const result = await authService.login(username, password);
      
      if (result.success && result.usuario) {
        setUser(result.usuario);
        return true;
      } else {
        setError(result.message || 'Error de autenticación');
        return false;
      }
    } catch (err: any) {
      const errorMessage = err.message || 'Error de conexión con el servidor';
      setError(errorMessage);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await authService.logout();
    } finally {
      setUser(null);
      navigate('/login');
    }
  }, [navigate]);

  const validateToken = useCallback(async (): Promise<boolean> => {
    try {
      const isValid = await authService.validateToken();
      if (!isValid) {
        setUser(null);
      }
      return isValid;
    } catch {
      setUser(null);
      return false;
    }
  }, []);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  return {
    user,
    isAuthenticated: !!user,
    isLoading,
    error,
    login,
    logout,
    validateToken,
    clearError,
  };
};