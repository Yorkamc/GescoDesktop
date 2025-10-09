import React from 'react';

interface LicenseStatus {
  isActive: boolean;
  message: string;
  fechaActivacion?: string;
  fechaExpiracion?: string;
  diasRestantes: number;
  maxUsuarios: number;
  organizacionId?: number;
}

interface LicenseStatusCardProps {
  status: LicenseStatus;
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('es-ES', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

export const LicenseStatusCard: React.FC<LicenseStatusCardProps> = ({ status }) => {
  const isActive = status.isActive;
  const isExpiringSoon = status.diasRestantes < 30;

  const getStatusColor = () => {
    if (isActive && !isExpiringSoon) return 'bg-green-50 border-green-200';
    if (isActive && isExpiringSoon) return 'bg-yellow-50 border-yellow-200';
    return 'bg-red-50 border-red-200';
  };

  const getIconColor = () => {
    if (isActive && !isExpiringSoon) return 'text-green-500';
    if (isActive && isExpiringSoon) return 'text-yellow-500';
    return 'text-red-500';
  };

  const getTextColor = () => {
    if (isActive && !isExpiringSoon) return 'text-green-800';
    if (isActive && isExpiringSoon) return 'text-yellow-800';
    return 'text-red-800';
  };

  const getIcon = () => {
    if (isActive && !isExpiringSoon) {
      return (
        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
        </svg>
      );
    }
    
    return (
      <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
      </svg>
    );
  };

  return (
    <div className={`mb-6 p-4 rounded-lg border ${getStatusColor()}`}>
      <div className="flex items-center mb-2">
        <div className={getIconColor()}>
          {getIcon()}
        </div>
        <span className={`font-semibold ml-2 ${getTextColor()}`}>
          {status.message}
        </span>
      </div>
      
      {isActive && (
        <div className="text-sm text-gray-600 space-y-1 mt-3">
          {status.fechaExpiracion && (
            <div className="flex justify-between">
              <span>Expira:</span>
              <span className="font-medium">{formatDate(status.fechaExpiracion)}</span>
            </div>
          )}
          <div className="flex justify-between">
            <span>Días restantes:</span>
            <span className={`font-semibold ${isExpiringSoon ? 'text-yellow-700' : 'text-green-700'}`}>
              {status.diasRestantes}
            </span>
          </div>
          <div className="flex justify-between">
            <span>Máximo usuarios:</span>
            <span className="font-medium">{status.maxUsuarios}</span>
          </div>
        </div>
      )}

      {isExpiringSoon && isActive && (
        <div className="mt-3 pt-3 border-t border-yellow-300">
          <p className="text-xs text-yellow-800">
            Tu licencia está por vencer. Considera renovarla pronto.
          </p>
        </div>
      )}
    </div>
  );
};