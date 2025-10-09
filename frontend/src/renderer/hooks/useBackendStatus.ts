import { useState, useEffect, useCallback } from 'react';
import { healthService } from '../services/api';

type BackendStatus = 'checking' | 'connected' | 'disconnected';

interface UseBackendStatusReturn {
  status: BackendStatus;
  latency?: number;
  checkStatus: () => Promise<void>;
  isConnected: boolean;
  isChecking: boolean;
}

export const useBackendStatus = (autoCheck: boolean = true): UseBackendStatusReturn => {
  const [status, setStatus] = useState<BackendStatus>('checking');
  const [latency, setLatency] = useState<number>();

  const checkStatus = useCallback(async () => {
    setStatus('checking');
    
    try {
      const result = await healthService.checkBackendConnection();
      setStatus(result.connected ? 'connected' : 'disconnected');
      setLatency(result.latency);
    } catch {
      setStatus('disconnected');
      setLatency(undefined);
    }
  }, []);

  useEffect(() => {
    if (autoCheck) {
      checkStatus();

      // Escuchar eventos del proceso principal
      const handleBackendConnected = () => {
        setStatus('connected');
      };
      
      const handleBackendFailed = () => {
        setStatus('disconnected');
      };
      
      window.addEventListener('backend-connected', handleBackendConnected);
      window.addEventListener('backend-connection-failed', handleBackendFailed);
      
      return () => {
        window.removeEventListener('backend-connected', handleBackendConnected);
        window.removeEventListener('backend-connection-failed', handleBackendFailed);
      };
    }
  }, [autoCheck, checkStatus]);

  return {
    status,
    latency,
    checkStatus,
    isConnected: status === 'connected',
    isChecking: status === 'checking',
  };
};