import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/api';
import { storageService } from '../services/storage';
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
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  // Cargar sesión al iniciar la aplicación
  useEffect(() => {
    const loadSession = async () => {
      try {
        console.log('🔍 Buscando sesión guardada...');
        
        // Verificar si electronAPI está disponible
        const hasElectronAPI = typeof window !== 'undefined' && 
                              !!(window as any).electronAPI?.storage;
        
        if (hasElectronAPI) {
          console.log('✅ API de Electron disponible, usando storage persistente');
        } else {
          console.log('⚠️ API de Electron NO disponible, usando localStorage como fallback');
        }
        
        // Migrar del formato antiguo si existe
        await storageService.migrateFromOldFormat();
        
        // Cargar datos de autenticación
        const authData = await storageService.loadAuthData();
        
        if (authData && authData.token && authData.user) {
          console.log(`✅ Sesión encontrada (guardada en v${authData.appVersion})`);
          
          // Guardar en localStorage para compatibilidad con código existente
          localStorage.setItem('token', authData.token);
          localStorage.setItem('user', JSON.stringify(authData.user));
          
          // Actualizar estado
          setUser(authData.user);
          
          // Validar que el token siga siendo válido
          const isValid = await authService.validateToken();
          if (!isValid) {
            console.log('⚠️ Token expirado, limpiando sesión');
            await storageService.clearAuthData();
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            setUser(null);
          }
        } else {
          console.log('📝 No hay sesión guardada');
        }
      } catch (error) {
        console.error('❌ Error cargando sesión:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadSession();
  }, []);

  const login = useCallback(async (username: string, password: string): Promise<boolean> => {
    setIsLoading(true);
    setError('');
    
    try {
      console.log('🔐 Intentando login para:', username);
      const result = await authService.login(username, password);
      
      if (result.success && result.usuario && result.token) {
        console.log('✅ Login exitoso');
        
        // Guardar en almacenamiento persistente
        try {
          await storageService.saveAuthData(result.token, result.usuario);
          console.log('✅ Sesión guardada en almacenamiento persistente');
        } catch (storageError) {
          console.warn('⚠️ Error guardando en storage persistente, usando localStorage:', storageError);
        }
        
        // También guardar en localStorage para compatibilidad
        localStorage.setItem('token', result.token);
        localStorage.setItem('user', JSON.stringify(result.usuario));
        
        // Actualizar estado
        setUser(result.usuario);
        
        // Debug: Verificar que se guardó
        const savedData = await storageService.loadAuthData();
        if (savedData) {
          console.log('✅ Sesión verificada en almacenamiento');
          
          // Debug en desarrollo: mostrar ruta de almacenamiento
          if ((window as any).electronAPI?.debug?.getStoragePath) {
            const storagePath = (window as any).electronAPI.debug.getStoragePath();
            console.log('📂 Archivo de sesión:', storagePath);
          } else {
            console.log('📂 Sesión guardada en localStorage (fallback)');
          }
        } else {
          console.log('✅ Sesión guardada en localStorage');
        }
        
        return true;
      } else {
        const errorMsg = result.message || 'Error de autenticación';
        console.error('❌ Login fallido:', errorMsg);
        setError(errorMsg);
        return false;
      }
    } catch (err: any) {
      const errorMessage = err.message || 'Error de conexión con el servidor';
      console.error('❌ Error en login:', errorMessage);
      setError(errorMessage);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      console.log('👋 Cerrando sesión...');
      await authService.logout();
    } catch (error) {
      console.warn('⚠️ Error en logout del servidor:', error);
    } finally {
      // Limpiar almacenamiento persistente
      try {
        await storageService.clearAuthData();
      } catch (error) {
        console.warn('⚠️ Error limpiando storage persistente:', error);
      }
      
      // Limpiar localStorage
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      // Limpiar estado
      setUser(null);
      
      console.log('✅ Sesión cerrada');
      navigate('/login');
    }
  }, [navigate]);

  const validateToken = useCallback(async (): Promise<boolean> => {
    const token = localStorage.getItem('token');
    
    if (!token) {
      console.log('📝 No hay token para validar');
      return false;
    }

    try {
      const isValid = await authService.validateToken();
      
      if (isValid) {
        console.log('✅ Token válido');
        
        // Si el token es válido pero no hay usuario en estado, restaurarlo
        if (!user) {
          const authData = await storageService.loadAuthData();
          if (authData && authData.user) {
            setUser(authData.user);
          }
        }
      } else {
        console.warn('⚠️ Token inválido');
        await storageService.clearAuthData();
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        setUser(null);
      }
      
      return isValid;
    } catch (error) {
      console.error('❌ Error validando token:', error);
      await storageService.clearAuthData();
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      setUser(null);
      return false;
    }
  }, [user]);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  const value: AuthContextValue = {
    user,
    isAuthenticated: !!user && !!localStorage.getItem('token'),
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