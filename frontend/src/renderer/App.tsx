import { Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import { Login } from './pages/Login/Login';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { LicenseActivation } from './pages/LicenseActivation/LicenseActivation';
import React from 'react';

function TestPage() {
  const navigate = useNavigate();
  const location = useLocation();
  
  console.log('🧪 TestPage renderizando...', { 
    path: location.pathname, 
    hash: window.location.hash,
    fullUrl: window.location.href 
  });
  
  return (
    <div style={{padding: '20px', fontFamily: 'Arial', textAlign: 'center', backgroundColor: '#f0f9ff', minHeight: '100vh'}}>
      <div style={{maxWidth: '600px', margin: '0 auto', backgroundColor: 'white', padding: '40px', borderRadius: '8px', boxShadow: '0 4px 6px rgba(0,0,0,0.1)'}}>
        <h1 style={{color: '#1e40af', marginBottom: '20px'}}>✅ GESCO Desktop Funciona!</h1>
        <p style={{color: '#64748b', marginBottom: '30px'}}>React y React Router están funcionando correctamente</p>
        
        <div style={{marginBottom: '30px'}}>
          <p><strong>Ruta actual:</strong> {location.pathname}</p>
          <p><strong>Hash:</strong> {window.location.hash}</p>
          <p><strong>URL completa:</strong> {window.location.href}</p>
          <p><strong>Router type:</strong> HashRouter (Electron compatible)</p>
        </div>

        <div style={{marginTop: '20px'}}>
          <button 
            onClick={() => navigate('/login')} 
            style={{
              margin: '10px', 
              padding: '12px 24px', 
              backgroundColor: '#3b82f6', 
              color: 'white', 
              border: 'none', 
              borderRadius: '6px', 
              cursor: 'pointer',
              fontSize: '16px'
            }}
          >
            🔐 Ir a Login
          </button>
          <button 
            onClick={() => navigate('/dashboard')} 
            style={{
              margin: '10px', 
              padding: '12px 24px', 
              backgroundColor: '#10b981', 
              color: 'white', 
              border: 'none', 
              borderRadius: '6px', 
              cursor: 'pointer',
              fontSize: '16px'
            }}
          >
            📊 Ir a Dashboard
          </button>
          <button 
            onClick={() => navigate('/license-activation')} 
            style={{
              margin: '10px', 
              padding: '12px 24px', 
              backgroundColor: '#8b5cf6', 
              color: 'white', 
              border: 'none', 
              borderRadius: '6px', 
              cursor: 'pointer',
              fontSize: '16px'
            }}
          >
            🔑 Activar Licencia
          </button>
        </div>

        <div style={{marginTop: '40px', padding: '20px', backgroundColor: '#f8fafc', borderRadius: '6px', textAlign: 'left'}}>
          <h3 style={{color: '#374151', marginBottom: '10px'}}>Estado del Sistema:</h3>
          <ul style={{color: '#6b7280', fontSize: '14px', listStyle: 'none', padding: 0}}>
            <li>✅ React: {React.version}</li>
            <li>✅ React Router: Funcionando</li>
            <li>✅ Tailwind CSS: {document.querySelector('style') ? 'Cargado' : 'No detectado'}</li>
            <li>✅ Electron: {typeof window !== 'undefined' && (window as any).require ? 'Sí' : 'No detectado'}</li>
          </ul>
        </div>
      </div>
    </div>
  );
}

// Componente de error fallback
function ErrorBoundary({ error }: { error: Error }) {
  console.error('🚨 Error en App:', error);
  return (
    <div style={{padding: '40px', textAlign: 'center', fontFamily: 'Arial'}}>
      <h1 style={{color: '#dc2626'}}>Error en la aplicación</h1>
      <p>Se produjo un error: {error.message}</p>
      <button 
        onClick={() => window.location.reload()} 
        style={{padding: '10px 20px', backgroundColor: '#dc2626', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer'}}
      >
        Recargar
      </button>
    </div>
  );
}

function App() {
  console.log('📱 App component renderizando con HashRouter...');
  console.log('Current hash:', window.location.hash);
  console.log('Current pathname would be:', window.location.hash.replace('#', '') || '/');
  
  try {
    return (
      <Routes>
        <Route path="/test" element={<TestPage />} />
        <Route path="/login" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/license-activation" element={<LicenseActivation />} />
        <Route path="/" element={<Navigate to="/test" replace />} />
        <Route path="*" element={<Navigate to="/test" replace />} />
      </Routes>
    );
  } catch (error) {
    console.error('🚨 Error renderizando App:', error);
    return <ErrorBoundary error={error as Error} />;
  }
}

export default App;