const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const { existsSync } = require('fs');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;

console.log('=== GESCO DESKTOP ELECTRON INICIANDO ===');
console.log('isDev:', isDev);
console.log('__dirname:', __dirname);
console.log('process.cwd():', process.cwd());

function createWindow() {
  console.log('Creando ventana principal...');
  
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: true,
      webSecurity: !isDev, // Solo desactivar webSecurity en desarrollo
      allowRunningInsecureContent: isDev,
      enableRemoteModule: false,
      backgroundThrottling: false
    },
    show: false,
    backgroundColor: '#f8fafc',
    titleBarStyle: 'default',
    autoHideMenuBar: !isDev, // Ocultar menu en producción
    icon: path.join(__dirname, 'assets', 'icon.png') // Si tienes icono
  });

  // Abrir DevTools solo en desarrollo
  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  let urlToLoad;
  
  if (isDev) {
    urlToLoad = 'http://localhost:5173';
    console.log('MODO DESARROLLO - URL:', urlToLoad);
  } else {
    console.log('MODO PRODUCCIÓN - Buscando archivos construidos...');
    
    // Rutas posibles para el archivo index.html construido
    const possiblePaths = [
      path.join(__dirname, 'dist', 'index.html'),
      path.join(__dirname, '..', 'dist', 'index.html'),
      path.join(process.resourcesPath, 'app', 'dist', 'index.html'),
      path.join(app.getAppPath(), 'dist', 'index.html'),
      // Rutas adicionales
      path.resolve(__dirname, 'dist', 'index.html'),
      path.resolve(process.cwd(), 'dist', 'index.html')
    ];
    
    let indexPath = null;
    for (const testPath of possiblePaths) {
      console.log('Probando ruta:', testPath);
      if (existsSync(testPath)) {
        indexPath = testPath;
        console.log('✅ ARCHIVO ENCONTRADO:', indexPath);
        break;
      } else {
        console.log('❌ No existe:', testPath);
      }
    }
    
    if (indexPath) {
      // Convertir ruta de Windows a URL correcta
      const normalizedPath = indexPath.replace(/\\/g, '/');
      urlToLoad = `file://${normalizedPath}`;
      console.log('URL final construida:', urlToLoad);
    } else {
      console.error('❌ ERROR CRÍTICO: index.html no encontrado en ninguna ubicación');
      urlToLoad = createErrorPage();
    }
  }

  console.log('🚀 Cargando URL:', urlToLoad);

  // === EVENTOS DE DEBUGGING DETALLADOS ===
  mainWindow.webContents.on('did-start-loading', () => {
    console.log('📋 INICIO DE CARGA');
  });

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('✅ CARGA COMPLETADA');
    
    // Inyectar script de diagnóstico
    mainWindow.webContents.executeJavaScript(`
      console.log('=== DIAGNÓSTICO POST-CARGA ===');
      
      // 1. Verificar elementos básicos
      const root = document.getElementById('root');
      console.log('Elemento root encontrado:', !!root);
      console.log('Contenido del root:', root ? root.innerHTML.substring(0, 200) + '...' : 'NO ENCONTRADO');
      
      // 2. Verificar scripts cargados
      console.log('Scripts en el documento:', document.scripts.length);
      Array.from(document.scripts).forEach((script, i) => {
        console.log(\`Script \${i+1}: \${script.src || 'inline'}\`);
      });
      
      // 3. Verificar CSS cargado
      console.log('Hojas de estilo:', document.styleSheets.length);
      
      // 4. Probar conectividad con backend (solo si no es archivo local)
      if (!window.location.protocol.includes('file')) {
        fetch('http://localhost:5100/api/system/health')
          .then(response => response.json())
          .then(data => console.log('✅ Backend accesible:', data))
          .catch(error => console.log('⚠️ Backend no accesible:', error.message));
      }
      
      // 5. Verificar si React se cargó después de un momento
      setTimeout(() => {
        const rootContent = document.getElementById('root')?.innerHTML;
        if (!rootContent || rootContent.trim() === '') {
          console.error('❌ PROBLEMA: React no se cargó correctamente después de 3 segundos');
          console.log('Contenido actual del body:', document.body.innerHTML.substring(0, 500));
        } else {
          console.log('✅ React parece haberse cargado correctamente');
          document.body.classList.add('react-loaded');
        }
      }, 3000);
      
      console.log('=== FIN DIAGNÓSTICO ===');
    `);
    
    // Mostrar ventana después de un breve delay
    setTimeout(() => {
      mainWindow.show();
      if (isDev) {
        mainWindow.focus();
      }
    }, 500);
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL) => {
    console.error('❌ FALLO EN CARGA:');
    console.error('  Código de error:', errorCode);
    console.error('  Descripción:', errorDescription);
    console.error('  URL que falló:', validatedURL);
    
    // Mostrar ventana incluso si falló la carga
    mainWindow.show();
    
    // Si es un archivo local y falló, mostrar página de error
    if (validatedURL.startsWith('file://')) {
      mainWindow.loadURL(createErrorPage());
    }
  });

  // Capturar logs de la consola del renderer
  mainWindow.webContents.on('console-message', (event, level, message, line, sourceId) => {
    const levelNames = ['LOG', 'WARN', 'ERROR'];
    const levelName = levelNames[level] || 'INFO';
    console.log(`[RENDERER ${levelName}] ${message} (${sourceId}:${line})`);
  });

  // Interceptar requests de red para debugging
  mainWindow.webContents.session.webRequest.onBeforeRequest((details, callback) => {
    if (isDev) {
      console.log('🌐 REQUEST:', details.method, details.url);
    }
    callback({});
  });

  mainWindow.webContents.session.webRequest.onErrorOccurred((details) => {
    console.error('❌ REQUEST ERROR:', details.url, details.error);
  });

  // Manejar navegación (para SPAs)
  mainWindow.webContents.on('will-navigate', (event, navigationUrl) => {
    if (isDev) {
      console.log('🧭 Navegando a:', navigationUrl);
    }
    
    // En producción, prevenir navegación a URLs externas
    if (!isDev && !navigationUrl.startsWith('file://') && !navigationUrl.startsWith('http://localhost')) {
      event.preventDefault();
      console.log('🚫 Navegación externa bloqueada:', navigationUrl);
    }
  });

  // Cargar la página
  mainWindow.loadURL(urlToLoad).catch(error => {
    console.error('❌ Error crítico en loadURL:', error);
    mainWindow.show();
  });

  // Evento de cierre
  mainWindow.on('closed', () => {
    console.log('🔄 Ventana principal cerrada');
    mainWindow = null;
  });

  // Prevenir nuevas ventanas
  mainWindow.webContents.setWindowOpenHandler(({ url }) => {
    console.log('🔗 Intento de abrir nueva ventana:', url);
    return { action: 'deny' };
  });
}

function createErrorPage() {
  const errorHtml = `<!DOCTYPE html>
  <html>
    <head>
      <title>Error - GESCO Desktop</title>
      <style>
        body { 
          font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
          padding: 40px; 
          background: #f8fafc; 
          color: #374151;
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          min-height: 90vh;
          text-align: center;
        }
        .container {
          background: white;
          padding: 40px;
          border-radius: 12px;
          box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
          max-width: 600px;
        }
        h1 { color: #dc2626; margin-bottom: 20px; }
        pre { 
          background: #f3f4f6; 
          padding: 20px; 
          border-radius: 8px; 
          margin: 20px 0;
          text-align: left;
          font-size: 12px;
          overflow-x: auto;
        }
        button {
          background: #3b82f6;
          color: white;
          border: none;
          padding: 12px 24px;
          border-radius: 6px;
          cursor: pointer;
          font-size: 16px;
          margin-top: 20px;
        }
        button:hover { background: #2563eb; }
        .status { margin: 10px 0; padding: 10px; border-radius: 6px; }
        .error { background: #fef2f2; border: 1px solid #fecaca; color: #dc2626; }
        .info { background: #eff6ff; border: 1px solid #bfdbfe; color: #1d4ed8; }
      </style>
    </head>
    <body>
      <div class="container">
        <h1>⚠️ Error de Carga</h1>
        <p>No se pudo cargar la aplicación React correctamente.</p>
        
        <div class="status error">
          <strong>Problema:</strong> El archivo index.html construido no se encontró o no se pudo cargar.
        </div>
        
        <div class="status info">
          <strong>Solución:</strong> Asegúrate de haber ejecutado <code>npm run build</code> antes de usar Electron.
        </div>
        
        <pre>Información de Debug:
__dirname: ${__dirname}
app.isPackaged: ${app.isPackaged}
process.resourcesPath: ${process.resourcesPath}
app.getAppPath(): ${app.getAppPath()}
process.cwd(): ${process.cwd()}</pre>
        
        <button onclick="location.reload()">🔄 Intentar de Nuevo</button>
        <button onclick="require('electron').shell.openExternal('http://localhost:5173')" style="margin-left: 10px;">🌐 Modo Desarrollo</button>
      </div>
    </body>
  </html>`;
  
  return 'data:text/html;charset=utf-8,' + encodeURIComponent(errorHtml);
}

// === EVENTOS DE LA APLICACIÓN ===
app.whenReady().then(() => {
  console.log('⚡ ELECTRON LISTO - Creando ventana...');
  createWindow();
});

app.on('window-all-closed', () => {
  console.log('🔄 Todas las ventanas cerradas');
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    console.log('🔄 Reactivando aplicación - Creando nueva ventana');
    createWindow();
  }
});

// Manejar errores no capturados
process.on('uncaughtException', (error) => {
  console.error('❌ Error no capturado:', error);
});

process.on('unhandledRejection', (reason, promise) => {
  console.error('❌ Promesa rechazada no manejada:', reason);
});

console.log('✅ Script principal de Electron cargado correctamente');