import React from 'react';
import ReactDOM from 'react-dom/client';
import { HashRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import App from './App';
import './styles/globals.css';

const rootElement = document.getElementById('root');

if (!rootElement) {
  document.body.innerHTML = `
    <div style="padding: 40px; text-align: center; font-family: Arial;">
      <h1 style="color: #dc2626;">Error Crítico</h1>
      <p>No se encontró el elemento root</p>
      <button onclick="location.reload()" style="padding: 10px 20px; background: #dc2626; color: white; border: none; border-radius: 4px;">
        Reintentar
      </button>
    </div>
  `;
} else {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 1000 * 60 * 5,
        retry: 2,
      },
    },
  });

  try {
    const root = ReactDOM.createRoot(rootElement);
    
    root.render(
      <React.StrictMode>
        <QueryClientProvider client={queryClient}>
          <HashRouter>
            <App />
          </HashRouter>
        </QueryClientProvider>
      </React.StrictMode>
    );
    
    // Ocultar loading después del render
    setTimeout(() => {
      document.body.classList.add('react-loaded');
    }, 100);
    
  } catch (error) {
    console.error('Error inicializando React:', error);
    rootElement.innerHTML = `
      <div style="padding: 40px; text-align: center; font-family: Arial;">
        <h1 style="color: #dc2626;">Error de Inicialización</h1>
        <p>Error iniciando la aplicación: ${(error as Error).message}</p>
        <button onclick="location.reload()" style="padding: 10px 20px; background: #dc2626; color: white; border: none; border-radius: 4px;">
          Reintentar
        </button>
      </div>
    `;
  }
}