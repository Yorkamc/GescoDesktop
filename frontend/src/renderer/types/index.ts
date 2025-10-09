// ============================================
// AUTH TYPES
// ============================================
export interface User {
  id: string;
  nombreUsuario: string;
  correo: string;
  nombreCompleto: string;
  organizacionId: string;
  rolId: string;
  nombreRol: string;
}

export interface LoginResponse {
  success: boolean;
  message?: string;
  usuario?: User;
  token?: string;
  isOffline?: boolean;
  tokenExpiration?: string;
}

// ============================================
// ACTIVITY TYPES
// ============================================
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
  updatedAt?: string;
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

export type ActivityStatus = 'not_started' | 'in_progress' | 'completed' | 'cancelled';

export const ActivityStatusId = {
  NOT_STARTED: 1,
  IN_PROGRESS: 2,
  COMPLETED: 3,
  CANCELLED: 4,
} as const;

// ============================================
// DASHBOARD TYPES
// ============================================
export interface DashboardStats {
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

// ============================================
// LICENSE TYPES
// ============================================
export interface LicenseStatus {
  isActive: boolean;
  message: string;
  fechaActivacion?: string;
  fechaExpiracion?: string;
  diasRestantes: number;
  maxUsuarios: number;
  organizacionId?: number;
}

export interface ActivationResponse {
  success: boolean;
  message?: string;
  fechaExpiracion?: string;
  diasRestantes?: number;
}

// ============================================
// API TYPES
// ============================================
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
  timestamp: string;
}

export interface ApiError {
  message: string;
  code?: string;
  statusCode?: number;
  details?: Record<string, any>;
}

// ============================================
// SYSTEM TYPES
// ============================================
export interface SystemStatus {
  database: boolean;
  api: boolean;
  sync: boolean;
  latency?: number;
}

export interface HealthCheckResponse {
  status: 'healthy' | 'unhealthy';
  timestamp: string;
  services: {
    database: boolean;
    cache: boolean;
    storage: boolean;
  };
}

// ============================================
// FORM TYPES
// ============================================
export interface FormState<T> {
  data: T;
  errors: Partial<Record<keyof T, string>>;
  isSubmitting: boolean;
  isDirty: boolean;
}

export type FormMode = 'create' | 'edit' | 'view';

// ============================================
// UI TYPES
// ============================================
export type AlertType = 'error' | 'warning' | 'success' | 'info';

export type LoadingSize = 'small' | 'medium' | 'large';

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost';

// ============================================
// UTILITY TYPES
// ============================================
export type Nullable<T> = T | null;

export type Optional<T> = T | undefined;

export type DeepPartial<T> = {
  [P in keyof T]?: DeepPartial<T[P]>;
};

export type Awaited<T> = T extends Promise<infer U> ? U : T;

// ============================================
// PAGINATION TYPES
// ============================================
export interface PaginationParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
}

// ============================================
// FILTER TYPES
// ============================================
export interface DateRange {
  startDate: string;
  endDate: string;
}

export interface SearchFilters {
  query?: string;
  status?: ActivityStatus;
  dateRange?: DateRange;
  organizationId?: string;
}

// ============================================
// EVENT TYPES
// ============================================
export interface CustomEventMap {
  'backend-connected': CustomEvent<{ latency: number }>;
  'backend-connection-failed': CustomEvent;
  'auth-token-expired': CustomEvent;
  'license-expired': CustomEvent<{ daysRemaining: number }>;
}