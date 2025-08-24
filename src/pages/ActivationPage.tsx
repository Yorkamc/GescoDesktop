import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const ActivationPage: React.FC = () => {
  const navigate = useNavigate();
  const [activationData, setActivationData] = useState({
    codigoActivacion: '',
    organizacionId: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const response = await fetch('http://localhost:5000/api/license/activate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          codigoActivacion: activationData.codigoActivacion,
          organizacionId: parseInt(activationData.organizacionId),
          dispositivoId: localStorage.getItem('deviceId') || 'desktop-device',
          nombreDispositivo: 'GESCO Desktop'
        })
      });

      const data = await response.json();

      if (data.success) {
        setSuccess('¡Licencia activada exitosamente!');
        setTimeout(() => navigate('/'), 2000);
      } else {
        setError(data.message || 'Error al activar la licencia');
      }
    } catch (err: any) {
      setError('Error de conexión con el servidor');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="card w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Activación de Licencia</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">Ingresa tu código de activación</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
              {error}
            </div>
          )}
          
          {success && (
            <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg">
              {success}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium mb-2">
              Código de Activación
            </label>
            <input
              type="text"
              className="input-field"
              placeholder="XXXX-XXXX-XXXX-XXXX"
              value={activationData.codigoActivacion}
              onChange={(e) => setActivationData({...activationData, codigoActivacion: e.target.value})}
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-2">
              ID de Organización
            </label>
            <input
              type="number"
              className="input-field"
              placeholder="1"
              value={activationData.organizacionId}
              onChange={(e) => setActivationData({...activationData, organizacionId: e.target.value})}
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full btn-primary"
          >
            {loading ? 'Activando...' : 'Activar Licencia'}
          </button>
        </form>

        <div className="mt-6 text-center">
          <button
            onClick={() => navigate('/')}
            className="text-gray-600 hover:text-gray-700 text-sm"
          >
            Volver al login
          </button>
        </div>
      </div>
    </div>
  );
};

export default ActivationPage;
