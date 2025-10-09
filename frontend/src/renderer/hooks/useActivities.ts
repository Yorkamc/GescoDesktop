import { useState, useEffect, useCallback } from 'react';
import { activitiesService } from '../services/api';

export interface Activity {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
  location?: string;
  activityStatusId: number;
  statusName?: string;
  managerUserId?: string;
  managerName?: string;
  organizationId?: string;
  organizationName?: string;
  createdAt: string;
}

export interface CreateActivityRequest {
  name: string;
  description?: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
  location?: string;
  activityStatusId?: number;
  managerUserId?: string;
  organizationId?: string;
}

export interface UseActivitiesReturn {
  activities: Activity[];
  isLoading: boolean;
  error: string;
  createActivity: (data: CreateActivityRequest) => Promise<Activity | null>;
  updateActivity: (id: string, data: CreateActivityRequest) => Promise<Activity | null>;
  deleteActivity: (id: string) => Promise<boolean>;
  refreshActivities: () => Promise<void>;
  clearError: () => void;
}

export const useActivities = (organizationId?: string): UseActivitiesReturn => {
  const [activities, setActivities] = useState<Activity[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const loadActivities = useCallback(async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await activitiesService.getActivities(organizationId);
      setActivities(data);
    } catch (err: any) {
      setError(err.message || 'Error al cargar actividades');
      console.error('Error loading activities:', err);
    } finally {
      setIsLoading(false);
    }
  }, [organizationId]);

  useEffect(() => {
    loadActivities();
  }, [loadActivities]);

  const createActivity = useCallback(async (data: CreateActivityRequest): Promise<Activity | null> => {
    try {
      setError('');
      const newActivity = await activitiesService.createActivity(data);
      setActivities(prev => [newActivity, ...prev]);
      return newActivity;
    } catch (err: any) {
      setError(err.message || 'Error al crear actividad');
      console.error('Error creating activity:', err);
      return null;
    }
  }, []);

  const updateActivity = useCallback(async (id: string, data: CreateActivityRequest): Promise<Activity | null> => {
    try {
      setError('');
      const updatedActivity = await activitiesService.updateActivity(id, data);
      setActivities(prev => prev.map(a => a.id === id ? updatedActivity : a));
      return updatedActivity;
    } catch (err: any) {
      setError(err.message || 'Error al actualizar actividad');
      console.error('Error updating activity:', err);
      return null;
    }
  }, []);

  const deleteActivity = useCallback(async (id: string): Promise<boolean> => {
    try {
      setError('');
      await activitiesService.deleteActivity(id);
      setActivities(prev => prev.filter(a => a.id !== id));
      return true;
    } catch (err: any) {
      setError(err.message || 'Error al eliminar actividad');
      console.error('Error deleting activity:', err);
      return false;
    }
  }, []);

  const clearError = useCallback(() => {
    setError('');
  }, []);

  return {
    activities,
    isLoading,
    error,
    createActivity,
    updateActivity,
    deleteActivity,
    refreshActivities: loadActivities,
    clearError,
  };
};