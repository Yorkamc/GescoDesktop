import React from 'react';

export interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
  loading?: boolean;
  subtitle?: string;
}

export const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  icon,
  color,
  loading,
  subtitle
}) => {
  return (
    <div className="bg-white overflow-hidden shadow-sm rounded-lg border hover:shadow-md transition-shadow">
      <div className="p-6">
        <div className="flex items-center">
          <div className="flex-shrink-0">
            <div className={`w-8 h-8 ${color} rounded-lg flex items-center justify-center`}>
              {icon}
            </div>
          </div>
          <div className="ml-4 flex-1">
            <p className="text-sm font-medium text-gray-900">{title}</p>
            <div className="flex items-baseline">
              <p className={`text-2xl font-bold ${color.replace('bg-', 'text-').replace('-100', '-600')}`}>
                {loading ? (
                  <div className="animate-pulse bg-gray-200 h-8 w-16 rounded"></div>
                ) : (
                  typeof value === 'number' && value > 999 ? value.toLocaleString() : value
                )}
              </p>
            </div>
            {subtitle && (
              <p className="text-xs text-gray-500 mt-1">{subtitle}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};