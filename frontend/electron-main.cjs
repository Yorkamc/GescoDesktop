// frontend/electron-main.cjs - VERSIÓN QUE FUNCIONA
const { app, BrowserWindow } = require('electron');
const path = require('path');

// Detectar modo desarrollo - MÚLTIPLES FORMAS
const isDev = process.env.NODE_ENV === 'development' || 
              process.env.ELECTRON_IS_DEV === '1' || 
              !app.isPackaged;

let mainWindow;

console.log('=== GESCO DESKTOP INICIANDO ===');
console.log('Modo desarrollo:', isDev);
console.log('NODE_ENV:', process.env.NODE_ENV);
console.log('ELECTRON_IS_DEV:', process.env.ELECTRON_IS_DEV);
console.log('app.isPackaged:', app.isPackaged);
console.log('===============================');

function createWindow() {
  console.log('Creando ventana principal...');
  
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1000,
    minHeight: 700,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: true, // SIEMPRE habilitar DevTools
      webSecurity: false // Desactivar para evitar CORS
    },
    show: false,
    backgroundColor: '#ffffff',
    titleBarStyle: 'default'
  });

  // SIEMPRE mostrar DevTools en desarrollo
  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  // Configurar URL a cargar
  let urlToLoad;
  
  if (isDev) {
    // DESARROLLO: Conectar al servidor Vite
    urlToLoad = 'http://localhost:5173';
    console.log('🌐 MODO DESARROLLO: Conectando a Vite dev server');
    console.log('URL:', urlToLoad);
  } else {
    // PRODUCCIÓN: Cargar archivos locales
    urlToLoad = path.join(__dirname, 'dist', 'index.html');
    console.log('📦 MODO PRODUCCIÓN: Cargando archivos locales');
    console.log('Path:', urlToLoad);
  }

  // Eventos para debugging
  mainWindow.webContents.on('did-start-loading', () => {
    console.log('⏳ Iniciando carga de página...');
  });

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('✅ Página cargada exitosamente');
    mainWindow.show();
    
    // Log adicional en desarrollo
    if (isDev) {
      console.log('🔍 DevTools habilitadas - presiona F12 para abrir');
    }
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL) => {
    console.error('❌ ERROR CARGANDO PÁGINA:');
    console.error('  Código:', errorCode);
    console.error('  Descripción:', errorDescription);
    console.error('  URL:', validatedURL);
    
    // Mostrar página de error personalizada
    const errorHtml = `
      <!DOCTYPE html>
      <html>
      <head>
        <title>GESCO Desktop - Error de Conexión</title>
        <style>
          body { 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            margin: 0; padding: 40px; min-height: 100vh; box-sizing: border-box;
            display: flex; align-items: center; justify-content: center;
          }
          .error-container { 
            background: white; border-radius: 12px; padding: 40px; 
            max-width: 600px; text-align: center; box-shadow: 0 20px 40px rgba(0,0,0,0.1);
          }
          .error-icon { font-size: 64px; margin-bottom: 20px; }
          .error-title { color: #dc3545; font-size: 28px; margin-bottom: 16px; font-weight: 600; }
          .error-description { color: #6c757d; margin-bottom: 24px; line-height: 1.6; }
          .error-details { 
            background: #f8f9fa; border-radius: 6px; padding: 16px; margin: 20px 0; 
            text-align: left; font-family: 'Monaco', 'Courier New', monospace; font-size: 14px; 
          }
          .retry-btn { 
            background: #007bff; color: white; border: none; padding: 12px 24px; 
            border-radius: 6px; font-size: 16px; cursor: pointer; margin: 8px;
            transition: background-color 0.2s;
          }
          .retry-btn:hover { background: #0056b3; }
          .help-text { 
            margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;
            color: #6c757d; font-size: 14px; text-align: left;
          }
          code { background: #e9ecef; padding: 2px 6px; border-radius: 3px; }
          .status-list { text-align: left; margin: 20px 0; }
          .status-item { margin: 8px 0; }
        </style>
      </head>
      <body>
        <div class="error-container">
          <div class="error-icon">⚠️</div>
          <div class="error-title">No se puede cargar la aplicación</div>
          <div class="error-description">
            GESCO Desktop no pudo conectarse a los servicios necesarios.
          </div>
          
          <div class="error-details">
            <strong>Error técnico:</strong><br>
            Código: ${errorCode}<br>
            Descripción: ${errorDescription}<br>
            URL: ${validatedURL}
          </div>
          
          <button class="retry-btn" onclick="location.reload()">🔄 Reintentar</button>
          <button class="retry-btn" onclick="window.location.href='http://localhost:5173'">🌐 Abrir en navegador</button>
          
          <div class="help-text">
            <strong>💡 Solución paso a paso:</strong>
            
            <div class="status-list">
              <div class="status-item">
                <strong>1. Verificar Backend:</strong><br>
                • Abre una terminal y ejecuta: <code>cd backend && dotnet run</code><br>
                • Verifica en: <a href="http://localhost:5100/swagger" target="_blank">http://localhost:5100/swagger</a>
              </div>
              
              <div class="status-item">
                <strong>2. Verificar Frontend:</strong><br>
                • Abre otra terminal y ejecuta: <code>cd frontend && npm run dev</code><br>
                • Verifica en: <a href="http://localhost:5173" target="_blank">http://localhost:5173</a>
              </div>
              
              <div class="status-item">
                <strong>3. Configuración actual:</strong><br>
                • Modo desarrollo: ${isDev ? 'Sí' : 'No'}<br>
                • URL esperada: ${isDev ? 'http://localhost:5173' : 'archivos locales'}<br>
                • NODE_ENV: ${process.env.NODE_ENV || 'no definido'}
              </div>
            </div>
            
            <strong>🔑 Credenciales de prueba:</strong><br>
            Usuario: <code>admin</code> | Contraseña: <code>admin123</code>
          </div>
        </div>
        
        <script>
          // Auto-retry cada 10 segundos
          setTimeout(() => {
            console.log('Auto-retry en 10 segundos...');
            location.reload();
          }, 10000);
        </script>
      </body>
      </html>
    `;
    
    mainWindow.loadURL('data:text/html;charset=utf-8,' + encodeURIComponent(errorHtml));
    mainWindow.show();
  });

  // Manejar requests adicionales
  mainWindow.webContents.on('did-start-navigation', (event, navigationUrl) => {
    console.log('🧭 Navegando a:', navigationUrl);
  });

  mainWindow.webContents.on('dom-ready', () => {
    console.log('📄 DOM listo');
  });

  // Cargar la aplicación
  console.log('🚀 Cargando aplicación...');
  if (isDev) {
    mainWindow.loadURL(urlToLoad);
  } else {
    mainWindow.loadFile(urlToLoad);
  }

  // Event handlers
  mainWindow.on('closed', () => {
    mainWindow = null;
    console.log('🚪 Ventana principal cerrada');
  });

  // Manejar links externos
  mainWindow.webContents.setWindowOpenHandler(({ url }) => {
    console.log('🔗 Abriendo enlace externo:', url);
    require('electron').shell.openExternal(url);
    return { action: 'deny' };
  });

  // Debug de requests en desarrollo
  if (isDev) {
    mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL, isMainFrame) => {
      console.log('🔍 DEBUG - Failed load:', { errorCode, errorDescription, validatedURL, isMainFrame });
    });
  }
}

// Manejo del ciclo de vida de la app
app.whenReady().then(() => {
  console.log('⚡ Electron listo - creando ventana...');
  createWindow();
});

app.on('window-all-closed', () => {
  console.log('🏁 Todas las ventanas cerradas');
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  console.log('🔄 App activada');
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// Manejo de errores globales
process.on('uncaughtException', (error) => {
  console.error('💥 Error no capturado:', error);
});

process.on('unhandledRejection', (reason, promise) => {
  console.error('💥 Promise rejection no manejada:', reason);
});

console.log('📱 GESCO Desktop configurado correctamente');