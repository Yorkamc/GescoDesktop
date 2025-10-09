import { useState, useCallback, ChangeEvent } from 'react';

interface UseFormOptions<T> {
  initialValues: T;
  onSubmit: (values: T) => void | Promise<void>;
  validate?: (values: T) => Partial<Record<keyof T, string>>;
}

interface UseFormReturn<T> {
  values: T;
  errors: Partial<Record<keyof T, string>>;
  touched: Partial<Record<keyof T, boolean>>;
  isSubmitting: boolean;
  isDirty: boolean;
  handleChange: (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => void;
  handleBlur: (e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => void;
  handleSubmit: (e: React.FormEvent) => Promise<void>;
  setFieldValue: (field: keyof T, value: any) => void;
  setFieldError: (field: keyof T, error: string) => void;
  setFieldTouched: (field: keyof T, touched: boolean) => void;
  resetForm: () => void;
  setValues: (values: T) => void;
}

export const useForm = <T extends Record<string, any>>({
  initialValues,
  onSubmit,
  validate,
}: UseFormOptions<T>): UseFormReturn<T> => {
  const [values, setValues] = useState<T>(initialValues);
  const [errors, setErrors] = useState<Partial<Record<keyof T, string>>>({});
  const [touched, setTouched] = useState<Partial<Record<keyof T, boolean>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDirty, setIsDirty] = useState(false);

  const handleChange = useCallback((
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value, type } = e.target;
    
    let finalValue: any = value;
    
    // Manejar checkboxes
    if (type === 'checkbox' && 'checked' in e.target) {
      finalValue = (e.target as HTMLInputElement).checked;
    }
    
    // Manejar números
    if (type === 'number') {
      finalValue = value === '' ? '' : Number(value);
    }

    setValues(prev => ({
      ...prev,
      [name]: finalValue,
    }));
    
    setIsDirty(true);
    
    // Limpiar error del campo al cambiar
    if (errors[name as keyof T]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[name as keyof T];
        return newErrors;
      });
    }
  }, [errors]);

  const handleBlur = useCallback((
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name } = e.target;
    
    setTouched(prev => ({
      ...prev,
      [name]: true,
    }));
    
    // Validar el campo al perder el foco
    if (validate) {
      const fieldErrors = validate(values);
      if (fieldErrors[name as keyof T]) {
        setErrors(prev => ({
          ...prev,
          [name]: fieldErrors[name as keyof T],
        }));
      }
    }
  }, [values, validate]);

  const handleSubmit = useCallback(async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validar todos los campos
    if (validate) {
      const validationErrors = validate(values);
      setErrors(validationErrors);
      
      if (Object.keys(validationErrors).length > 0) {
        // Marcar todos los campos como touched
        const allTouched = Object.keys(values).reduce((acc, key) => ({
          ...acc,
          [key]: true,
        }), {});
        setTouched(allTouched);
        return;
      }
    }
    
    setIsSubmitting(true);
    
    try {
      await onSubmit(values);
      setIsDirty(false);
    } catch (error: any) {
      console.error('Form submission error:', error);
      
      // Si el error tiene información de campos, establecerlos
      if (error.fieldErrors) {
        setErrors(error.fieldErrors);
      }
    } finally {
      setIsSubmitting(false);
    }
  }, [values, validate, onSubmit]);

  const setFieldValue = useCallback((field: keyof T, value: any) => {
    setValues(prev => ({
      ...prev,
      [field]: value,
    }));
    setIsDirty(true);
  }, []);

  const setFieldError = useCallback((field: keyof T, error: string) => {
    setErrors(prev => ({
      ...prev,
      [field]: error,
    }));
  }, []);

  const setFieldTouched = useCallback((field: keyof T, isTouched: boolean) => {
    setTouched(prev => ({
      ...prev,
      [field]: isTouched,
    }));
  }, []);

  const resetForm = useCallback(() => {
    setValues(initialValues);
    setErrors({});
    setTouched({});
    setIsSubmitting(false);
    setIsDirty(false);
  }, [initialValues]);

  const setValuesCallback = useCallback((newValues: T) => {
    setValues(newValues);
    setIsDirty(true);
  }, []);

  return {
    values,
    errors,
    touched,
    isSubmitting,
    isDirty,
    handleChange,
    handleBlur,
    handleSubmit,
    setFieldValue,
    setFieldError,
    setFieldTouched,
    resetForm,
    setValues: setValuesCallback,
  };
};

// Hook para manejar inputs individuales
export const useFormField = <T,>(
  name: string,
  form: UseFormReturn<any>
) => {
  const value = form.values[name] as T;
  const error = form.errors[name];
  const touched = form.touched[name];

  return {
    name,
    value,
    error,
    touched,
    showError: touched && !!error,
    onChange: form.handleChange,
    onBlur: form.handleBlur,
  };
};