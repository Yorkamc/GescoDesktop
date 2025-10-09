import { useState, useEffect, useCallback } from 'react';
import { statsService } from '../services/api';

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

interface UseDashboardStatsReturn {
  stats: DashboardStats;
  isLoading: boolean;
  error: string;
  lastUpdate: Date;
  refreshStats: () => Promise<void>;
  clearError: () => void;
}

const defaultStats: DashboardStats = {
  totalActivities: 0,
  activeActivities: 0,
  todaySales: 0,
  todayTransactions: 0,
  monthSales: 0,
  monthTransactions: 0,
  totalUsers: 0,
  activeUsers: 0,
  totalProducts: 0,
  activeProducts: 0,
  lowStockProducts: 0,
  queryDate: '',
  reportPeriod: ''
};

export const useDashboardStats = (autoRefresh: boolean = true, refreshInterval: number = 5 * 60 * 1000): UseDashboardStatsReturn => {
  const [stats, setStats] = useState<DashboardStats>(defaultStats);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [lastUpdate, setLastUpdate] = useState(new Date());

  const loadStats = useCallback(async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await statsService.getStats();
      setStats(data);
      setLastUpdate(new Date());
    } catch (err: any) {
      setError(err.message || 'Error al cargar estadísticas');
      console.error('Stats error:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadStats();

    if (autoRefresh) {
      const interval = setInterval(loadStats, refreshInterval);
      return () => clearInterval(interval);
    }
  }, [loadStats, autoRefresh, refreshInterval]);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  return {
    stats,
    isLoading,
    error,
    lastUpdate,
    refreshStats: loadStats,
    clearError,
  };
};