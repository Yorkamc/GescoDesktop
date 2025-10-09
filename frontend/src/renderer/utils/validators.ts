/**
 * Utilidades de validación para la aplicación
 */

// ============================================
// VALIDACIONES BÁSICAS
// ============================================

export const isEmpty = (value: any): boolean => {
  if (value === null || value === undefined) return true;
  if (typeof value === 'string') return value.trim().length === 0;
  if (Array.isArray(value)) return value.length === 0;
  if (typeof value === 'object') return Object.keys(value).length === 0;
  return false;
};

export const isNotEmpty = (value: any): boolean => {
  return !isEmpty(value);
};

export const isEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

export const isUrl = (url: string): boolean => {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
};

export const isNumeric = (value: string): boolean => {
  return !isNaN(parseFloat(value)) && isFinite(Number(value));
};

export const isInteger = (value: string): boolean => {
  return Number.isInteger(Number(value));
};

// ============================================
// VALIDACIONES DE LONGITUD
// ============================================

export const minLength = (value: string, min: number): boolean => {
  return value.length >= min;
};

export const maxLength = (value: string, max: number): boolean => {
  return value.length <= max;
};

export const lengthBetween = (value: string, min: number, max: number): boolean => {
  return value.length >= min && value.length <= max;
};

// ============================================
// VALIDACIONES DE RANGO
// ============================================

export const minValue = (value: number, min: number): boolean => {
  return value >= min;
};

export const maxValue = (value: number, max: number): boolean => {
  return value <= max;
};

export const valueBetween = (value: number, min: number, max: number): boolean => {
  return value >= min && value <= max;
};

// ============================================
// VALIDACIONES DE FECHA
// ============================================

export const isValidDate = (dateString: string): boolean => {
  const date = new Date(dateString);
  return !isNaN(date.getTime());
};

export const isDateInFuture = (dateString: string): boolean => {
  const date = new Date(dateString);
  return date.getTime() > Date.now();
};

export const isDateInPast = (dateString: string): boolean => {
  const date = new Date(dateString);
  return date.getTime() < Date.now();
};

export const isDateBefore = (date1: string, date2: string): boolean => {
  return new Date(date1).getTime() < new Date(date2).getTime();
};

export const isDateAfter = (date1: string, date2: string): boolean => {
  return new Date(date1).getTime() > new Date(date2).getTime();
};

// ============================================
// VALIDACIONES ESPECÍFICAS DE CR
// ============================================

export const isValidCedulaCR = (cedula: string): boolean => {
  const cleaned = cedula.replace(/\D/g, '');
  
  // Cédula física: 9 dígitos
  if (cleaned.length === 9) {
    return /^[1-9]\d{8}$/.test(cleaned);
  }
  
  // Cédula jurídica: 10 dígitos
  if (cleaned.length === 10) {
    return /^[1-9]\d{9}$/.test(cleaned);
  }
  
  // DIMEX/NITE: 11-12 dígitos
  if (cleaned.length >= 11 && cleaned.length <= 12) {
    return /^\d{11,12}$/.test(cleaned);
  }
  
  return false;
};

export const isValidPhoneCR = (phone: string): boolean => {
  const cleaned = phone.replace(/\D/g, '');
  
  // Teléfono fijo o celular: 8 dígitos
  return cleaned.length === 8 && /^[2-8]\d{7}$/.test(cleaned);
};

// ============================================
// VALIDACIONES DE CONTRASEÑA
// ============================================

export const isStrongPassword = (password: string): boolean => {
  // Al menos 8 caracteres, 1 mayúscula, 1 minúscula, 1 número
  const hasMinLength = password.length >= 8;
  const hasUpperCase = /[A-Z]/.test(password);
  const hasLowerCase = /[a-z]/.test(password);
  const hasNumber = /\d/.test(password);
  
  return hasMinLength && hasUpperCase && hasLowerCase && hasNumber;
};

export const hasSpecialChar = (password: string): boolean => {
  return /[!@#$%^&*(),.?":{}|<>]/.test(password);
};

// ============================================
// VALIDACIONES DE ACTIVIDADES
// ============================================

export interface ActivityValidationErrors {
  name?: string;
  startDate?: string;
  endDate?: string;
  startTime?: string;
  endTime?: string;
}

export const validateActivity = (data: {
  name: string;
  startDate: string;
  startTime?: string;
  endDate?: string;
  endTime?: string;
}): ActivityValidationErrors => {
  const errors: ActivityValidationErrors = {};
  
  // Nombre requerido
  if (isEmpty(data.name)) {
    errors.name = 'El nombre es requerido';
  } else if (!minLength(data.name.trim(), 3)) {
    errors.name = 'El nombre debe tener al menos 3 caracteres';
  } else if (!maxLength(data.name.trim(), 100)) {
    errors.name = 'El nombre no puede exceder 100 caracteres';
  }
  
  // Fecha de inicio requerida
  if (isEmpty(data.startDate)) {
    errors.startDate = 'La fecha de inicio es requerida';
  } else if (!isValidDate(data.startDate)) {
    errors.startDate = 'Fecha de inicio inválida';
  }
  
  // Validar fecha de fin si existe
  if (data.endDate) {
    if (!isValidDate(data.endDate)) {
      errors.endDate = 'Fecha de fin inválida';
    } else if (data.startDate && !isDateBefore(data.startDate, data.endDate)) {
      errors.endDate = 'La fecha de fin debe ser posterior a la fecha de inicio';
    }
  }
  
  // Validar horas si están en el mismo día
  if (data.startDate && data.endDate && data.startDate === data.endDate) {
    if (data.startTime && data.endTime) {
      const startDateTime = new Date(`${data.startDate}T${data.startTime}`);
      const endDateTime = new Date(`${data.endDate}T${data.endTime}`);
      
      if (endDateTime <= startDateTime) {
        errors.endTime = 'La hora de fin debe ser posterior a la hora de inicio';
      }
    }
  }
  
  return errors;
};

// ============================================
// VALIDACIONES DE LOGIN
// ============================================

export interface LoginValidationErrors {
  username?: string;
  password?: string;
}

export const validateLogin = (username: string, password: string): LoginValidationErrors => {
  const errors: LoginValidationErrors = {};
  
  if (isEmpty(username)) {
    errors.username = 'El usuario es requerido';
  }
  
  if (isEmpty(password)) {
    errors.password = 'La contraseña es requerida';
  }
  
  return errors;
};

// ============================================
// VALIDACIONES DE LICENCIA
// ============================================

export interface LicenseValidationErrors {
  code?: string;
  organizationId?: string;
}

export const validateLicenseActivation = (
  code: string,
  organizationId: number
): LicenseValidationErrors => {
  const errors: LicenseValidationErrors = {};
  
  if (isEmpty(code)) {
    errors.code = 'El código de activación es requerido';
  } else if (code.replace(/[-\s]/g, '').length < 10) {
    errors.code = 'El código de activación es inválido';
  }
  
  if (!organizationId || organizationId < 1) {
    errors.organizationId = 'El ID de organización debe ser mayor a 0';
  }
  
  return errors;
};

// ============================================
// UTILIDAD PARA VERIFICAR SI HAY ERRORES
// ============================================

export const hasErrors = (errors: Record<string, any>): boolean => {
  return Object.keys(errors).length > 0;
};

export const getFirstError = (errors: Record<string, any>): string | undefined => {
  const keys = Object.keys(errors);
  return keys.length > 0 ? errors[keys[0]] : undefined;
};

// ============================================
// SANITIZACIÓN
// ============================================

export const sanitizeString = (value: string): string => {
  return value
    .trim()
    .replace(/[<>]/g, '') // Remove < and >
    .replace(/\s+/g, ' '); // Normalize whitespace
};

export const sanitizeHtml = (html: string): string => {
  const div = document.createElement('div');
  div.textContent = html;
  return div.innerHTML;
};