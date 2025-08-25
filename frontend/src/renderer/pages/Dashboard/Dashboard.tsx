import React from 'react';

export const Dashboard: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-100">
      <div className="py-12">
        <div className="max-w-7xl mx-auto sm:px-6 lg:px-8">
          <div className="bg-white overflow-hidden shadow-xl sm:rounded-lg p-6">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">
              Dashboard - GESCO Desktop
            </h1>
            <p className="text-gray-600">
              Bienvenido al sistema de gestión de actividades
            </p>
            <div className="mt-6 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              <div className="bg-blue-50 p-6 rounded-lg">
                <h3 className="text-lg font-medium text-blue-900">Actividades</h3>
                <p className="mt-2 text-3xl font-bold text-blue-600">0</p>
              </div>
              <div className="bg-green-50 p-6 rounded-lg">
                <h3 className="text-lg font-medium text-green-900">Ventas Hoy</h3>
                <p className="mt-2 text-3xl font-bold text-green-600">0</p>
              </div>
              <div className="bg-purple-50 p-6 rounded-lg">
                <h3 className="text-lg font-medium text-purple-900">Transacciones</h3>
                <p className="mt-2 text-3xl font-bold text-purple-600">0</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
