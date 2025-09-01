import { Routes, Route, Navigate } from 'react-router-dom';
import { Login } from './pages/Login/Login';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { LicenseActivation } from './pages/LicenseActivation/LicenseActivation';


function ErrorFallback({ error }: { error: Error }) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center p-8 bg-white rounded-lg shadow-md max-w-md">
        <h1 className="text-xl font-bold text-red-600 mb-4">Error en la aplicación</h1>
        <p className="text-gray-600 mb-4">{error.message}</p>
        <button 
          onClick={() => window.location.reload()} 
          className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
        >
          Recargar aplicación
        </button>
      </div>
    </div>
  );
}

function App() {
  try {
    return (
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/license-activation" element={<LicenseActivation />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    );
  } catch (error) {
    return <ErrorFallback error={error as Error} />;
  }
}

export default App;