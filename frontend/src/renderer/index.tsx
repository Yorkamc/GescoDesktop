import React from 'react';
import ReactDOM from 'react-dom/client';
import { HashRouter } from 'react-router-dom'; // Cambio crítico: BrowserRouter → HashRouter
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import App from './App';
import './styles/globals.css';

console.log('🚀 INICIANDO APLICACIÓN REACT');
console.log('React version:', React.version);
console.log('Location:', window.location.href);
console.log('Protocol:', window.location.protocol);

// Verificar que tenemos el elemento root
const rootElement = document.getElementById('root');
console.log('Root element found:', !!rootElement);

if (!rootElement) {
  console.error('❌ ERROR CRÍTICO: No se encontró el elemento #root');
  document.body.innerHTML = `
    <div style="padding: 40px; text-align: center; font-family: Arial, sans-serif; background: #fee2e2; min-height: 100vh; display: flex; align-items: center; justify-content: center;">
      <div style="background: white; padding: 40px; border-radius: 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); max-width: 500px;">
        <h1 style="color: #dc2626; margin-bottom: 20px;">Error Crítico</h1>
        <p style="color: #7f1d1d; margin-bottom: 16px;">No se encontró el elemento root (#root)</p>
        <button onclick="location.reload()" style="background: #dc2626; color: white; border: none; padding: 12px 24px; border-radius: 6px; cursor: pointer;">Reintentar</button>
      </div>
    </div>
  `;
} else {
  console.log('✅ Elemento root encontrado, iniciando React...');
  console.log('🔧 Usando HashRouter para compatibilidad con Electron (file:// protocol)');

  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 1000 * 60 * 5, // 5 minutos
        retry: 2,
      },
    },
  });

  try {
    console.log('📦 Creando root de React...');
    const root = ReactDOM.createRoot(rootElement);
    
    console.log('🎨 Renderizando aplicación con HashRouter...');
    root.render(
      <React.StrictMode>
        <QueryClientProvider client={queryClient}>
          <HashRouter>
            <App />
          </HashRouter>
        </QueryClientProvider>
      </React.StrictMode>
    );
    
    console.log('✅ React renderizado exitosamente');
    
    // Verificar renderizado después de un momento
    setTimeout(() => {
      document.body.classList.add('react-loaded');
      console.log('✅ Clase react-loaded agregada');
      
      const rootContent = document.getElementById('root')?.innerHTML;
      console.log('Root content length after HashRouter render:', rootContent?.length || 0);
      
      if (!rootContent || rootContent.trim() === '') {
        console.error('❌ Incluso con HashRouter no se generó contenido');
        console.error('Esto puede indicar un problema más profundo...');
        
        // Fallback a renderizado directo sin router
        console.log('🚨 Intentando renderizado sin router...');
        root.render(
          <div style={{padding: '40px', textAlign: 'center', fontFamily: 'Arial, sans-serif'}}>
            <h1 style={{color: '#dc2626'}}>Router Fallback</h1>
            <p>HashRouter también falló. Problema con React routing.</p>
            <div style={{marginTop: '20px', padding: '20px', background: '#f3f4f6', borderRadius: '8px'}}>
              <p>URL actual: {window.location.href}</p>
              <p>Hash: {window.location.hash}</p>
            </div>
          </div>
        );
      } else {
        console.log('✅ HashRouter funcionó! Contenido generado correctamente');
        console.log('Hash actual:', window.location.hash);
      }
    }, 1000);
    
  } catch (error) {
    console.error('❌ Error crítico al renderizar React:', error);
    
    // Mostrar error específico
    rootElement.innerHTML = `
      <div style="padding: 40px; text-align: center; font-family: Arial, sans-serif; background: #fef2f2;">
        <h1 style="color: #dc2626; margin-bottom: 20px;">Error de Inicialización</h1>
        <p style="color: #7f1d1d; margin-bottom: 16px;">Error con HashRouter:</p>
        <pre style="background: #fee2e2; padding: 16px; border-radius: 6px; text-align: left; overflow-x: auto; font-size: 12px; color: #7f1d1d; margin: 16px 0;">${(error as Error).message}</pre>
        <button onclick="location.reload()" style="background: #dc2626; color: white; border: none; padding: 12px 24px; border-radius: 6px; cursor: pointer;">Reintentar</button>
      </div>
    `;
  }
}

// Debugging global
(window as any).debugReact = () => {
  console.log('=== DEBUG REACT ===');
  console.log('Root element:', document.getElementById('root'));
  console.log('Root content:', document.getElementById('root')?.innerHTML?.substring(0, 500));
  console.log('Hash:', window.location.hash);
  console.log('Full URL:', window.location.href);
};