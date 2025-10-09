import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/api';
import type { User } from '../types';

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string;
  login: (username: string, password: string) => Promise<boolean>;
  logout: () => Promise<void>;
  validateToken: () => Promise<boolean>;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: React.ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const navigate = useNavigate();
  const [user, setUser] = useState<User | null>(() => {
    const userData = localStorage.getItem('user');
    return userData ? JSON.parse(userData) : null;
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  // Validar token al cargar la aplicación
  useEffect(() => {
    const validateStoredToken = async () => {
      const token = localStorage.getItem('token');
      if (token && !user) {
        const isValid = await validateToken();
        if (!isValid) {
          setUser(null);
        }
      }
    };

    validateStoredToken();
  }, []);

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

  const value: AuthContextValue = {
    user,
    isAuthenticated: !!user,
    isLoading,
    error,
    login,
    logout,
    validateToken,
    clearError,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth debe ser usado dentro de un AuthProvider');
  }
  
  return context;
};

// Hook alternativo para componentes que pueden funcionar sin auth
export const useOptionalAuth = (): AuthContextValue | null => {
  return useContext(AuthContext) ?? null;
};