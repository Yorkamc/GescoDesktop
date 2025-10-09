import { useState, useEffect, useCallback } from 'react';
import { licenseService } from '../services/api';

interface LicenseStatus {
  isActive: boolean;
  message: string;
  fechaActivacion?: string;
  fechaExpiracion?: string;
  diasRestantes: number;
  maxUsuarios: number;
  organizacionId?: number;
}

interface UseLicenseReturn {
  licenseStatus: LicenseStatus | null;
  isLoading: boolean;
  error: string;
  message: string;
  messageType: 'success' | 'error' | '';
  activateLicense: (code: string, orgId: number) => Promise<boolean>;
  checkStatus: () => Promise<void>;
  clearMessages: () => void;
}

export const useLicense = (): UseLicenseReturn => {
  const [licenseStatus, setLicenseStatus] = useState<LicenseStatus | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState<'success' | 'error' | ''>('');

  const checkStatus = useCallback(async () => {
    try {
      const status = await licenseService.getStatus();
      setLicenseStatus(status);
    } catch (err: any) {
      console.error('Error checking license status:', err);
      setError(err.message || 'Error al verificar estado de licencia');
    }
  }, []);

  useEffect(() => {
    checkStatus();
  }, [checkStatus]);

  const activateLicense = useCallback(async (code: string, orgId: number): Promise<boolean> => {
    setIsLoading(true);
    setMessage('');
    setError('');
    
    try {
      const result = await licenseService.activate(code, orgId);
      
      if (result.success) {
        setMessage(result.message || 'Licencia activada correctamente');
        setMessageType('success');
        await checkStatus();
        return true;
      } else {
        setMessage(result.message || 'Error al activar la licencia');
        setMessageType('error');
        return false;
      }
    } catch (err: any) {
      setMessage('Error de conexión con el servidor');
      setMessageType('error');
      setError(err.message || 'Error al activar licencia');
      console.error('Activation error:', err);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [checkStatus]);

  const clearMessages = useCallback(() => {
    setMessage('');
    setError('');
    setMessageType('');
  }, []);

  return {
    licenseStatus,
    isLoading,
    error,
    message,
    messageType,
    activateLicense,
    checkStatus,
    clearMessages,
  };
};