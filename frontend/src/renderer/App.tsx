import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { Login } from './pages/Login/Login';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { LicenseActivation } from './pages/LicenseActivation/LicenseActivation';

function TestPage() {
  const navigate = useNavigate();
  
  return (
    <div style={{padding: '20px', fontFamily: 'Arial', textAlign: 'center'}}>
      <h1>GESCO Desktop - Navegación Test</h1>
      <div style={{marginTop: '20px'}}>
        <button onClick={() => navigate('/login')} style={{margin: '10px', padding: '10px 20px'}}>
          Ir a Login
        </button>
        <button onClick={() => navigate('/dashboard')} style={{margin: '10px', padding: '10px 20px'}}>
          Ir a Dashboard
        </button>
        <button onClick={() => navigate('/license-activation')} style={{margin: '10px', padding: '10px 20px'}}>
          Activar Licencia
        </button>
      </div>
      <p style={{marginTop: '20px'}}>
        Si los botones funcionan, React Router está OK.
      </p>
    </div>
  );
}

function App() {
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
}

export default App;