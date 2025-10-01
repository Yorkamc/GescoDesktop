const { app, BrowserWindow } = require('electron');
const path = require('path');
const { existsSync } = require('fs');
const { spawn } = require('child_process');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;
let backendProcess;
let backendReady = false;

console.log('=== GESCO DESKTOP STANDALONE ===');
console.log('isDev:', isDev);
console.log('isPackaged:', app.isPackaged);
console.log('Platform:', process.platform);

function getIconPath() {
  const platform = process.platform;
  let iconName;
  
  // Determinar el nombre del icono según la plataforma
  if (platform === 'win32') {
    iconName = 'icon.ico';
  } else if (platform === 'darwin') {
    iconName = 'icon.icns';
  } else {
    iconName = 'icon-512.png';
  }
  
  // Rutas posibles para buscar el icono
  const possiblePaths = [
    // En desarrollo
    path.join(__dirname, 'build', iconName),
    path.join(__dirname, 'build', 'icon-512.png'), // Fallback a PNG
    
    // En producción (empaquetado)
    path.join(process.resourcesPath, 'build', iconName),
    path.join(process.resourcesPath, 'app.asar', 'build', iconName),
    path.join(process.resourcesPath, 'app', 'build', iconName),
    
    // Otras ubicaciones
    path.join(app.getAppPath(), 'build', iconName),
    path.join(app.getAppPath(), 'build', 'icon-512.png')
  ];
  
  // Buscar el icono en las rutas posibles
  for (const iconPath of possiblePaths) {
    if (existsSync(iconPath)) {
      console.log('🎨 Icono encontrado:', iconPath);
      return iconPath;
    }
  }
  
  console.warn('⚠️ No se encontró icono personalizado en ninguna ruta');
  console.warn('Rutas verificadas:', possiblePaths);
  
  // En Windows, si no hay icono, Electron usa el predeterminado
  // No retornar undefined para evitar errores
  return null;
}

function startBackend() {
  if (isDev) {
    console.log('🔧 Modo desarrollo: backend externo esperado');
    return Promise.resolve();
  }

  return new Promise((resolve) => {
    console.log('🚀 Iniciando backend integrado...');
    
    const backendPaths = [
      path.join(process.resourcesPath, 'backend', 'Gesco.Desktop.UI.exe'),
      path.join(__dirname, 'backend', 'Gesco.Desktop.UI.exe'),
      path.join(process.cwd(), 'backend', 'Gesco.Desktop.UI.exe')
    ];
    
    let backendPath = null;
    for (const testPath of backendPaths) {
      if (existsSync(testPath)) {
        backendPath = testPath;
        console.log('✅ Backend encontrado:', backendPath);
        break;
      }
    }
    
    if (!backendPath) {
      console.error('❌ Backend no encontrado');
      resolve();
      return;
    }

    backendProcess = spawn(backendPath, [], {
      detached: false,
      stdio: ['ignore', 'pipe', 'pipe']
    });

    backendProcess.stdout.on('data', (data) => {
      const output = data.toString();
      console.log('[BACKEND]', output);
      
      if (output.includes('Now listening on: http://localhost:5100')) {
        console.log('✅ Backend listo');
        backendReady = true;
        
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('backend-ready');
        }
      }
    });

    backendProcess.stderr.on('data', (data) => {
      console.error('[BACKEND ERROR]', data.toString());
    });

    backendProcess.on('error', (error) => {
      console.error('❌ Error iniciando backend:', error);
    });

    backendProcess.on('exit', (code) => {
      console.log(`🔄 Backend terminó con código: ${code}`);
      backendReady = false;
    });

    console.log('⚡ Backend iniciándose en segundo plano...');
    setTimeout(resolve, 500);
  });
}

function createWindow() {
  console.log('📱 Creando ventana principal...');
  
  const iconPath = getIconPath();
  
  // Configuración de la ventana
  const windowConfig = {
    width: 1400,
    height: 900,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: isDev,
      webSecurity: !isDev,
      backgroundThrottling: false
    },
    show: false,
    backgroundColor: '#f8fafc',
    autoHideMenuBar: true,
    title: 'GESCO Desktop'
  };
  
  // Solo agregar icon si existe
  if (iconPath) {
    windowConfig.icon = iconPath;
  }
  
  mainWindow = new BrowserWindow(windowConfig);

  // En Windows, también configurar el icono de la aplicación
  if (process.platform === 'win32' && iconPath) {
    mainWindow.setIcon(iconPath);
  }

  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  let urlToLoad;
  
  if (isDev) {
    urlToLoad = 'http://localhost:5173';
    console.log('🔧 MODO DESARROLLO - URL:', urlToLoad);
  } else {
    console.log('📦 MODO PRODUCCIÓN - Buscando archivos...');
    
    const possiblePaths = [
      path.join(__dirname, 'dist', 'index.html'),
      path.join(process.resourcesPath, 'app', 'dist', 'index.html'),
      path.join(app.getAppPath(), 'dist', 'index.html')
    ];
    
    let indexPath = null;
    for (const testPath of possiblePaths) {
      if (existsSync(testPath)) {
        indexPath = testPath;
        console.log('✅ Frontend encontrado:', indexPath);
        break;
      }
    }
    
    if (indexPath) {
      urlToLoad = 'file://' + indexPath.replace(/\\/g, '/');
    } else {
      console.error('❌ Frontend no encontrado');
      urlToLoad = createErrorPage('Frontend no encontrado');
    }
  }

  console.log('🌐 Cargando URL:', urlToLoad);

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('✅ Ventana cargada');
    mainWindow.show();
    checkBackendConnection();
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    console.error('❌ Error cargando ventana:', errorCode, errorDescription);
    mainWindow.show();
  });

  mainWindow.loadURL(urlToLoad).catch(error => {
    console.error('❌ Error en loadURL:', error);
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

function checkBackendConnection() {
  if (!mainWindow || mainWindow.isDestroyed()) return;
  
  mainWindow.webContents.executeJavaScript(`
    (async () => {
      console.log('🔍 Verificando conectividad con backend...');
      
      let retries = 0;
      const maxRetries = 30;
      
      const checkConnection = async () => {
        try {
          const response = await fetch('http://localhost:5100/api/system/health', {
            signal: AbortSignal.timeout(3000)
          });
          
          if (response.ok) {
            const data = await response.json();
            console.log('✅ Backend conectado:', data);
            document.title = 'GESCO Desktop - Conectado';
            window.dispatchEvent(new CustomEvent('backend-connected', { detail: data }));
            return true;
          }
        } catch (error) {
          console.log(\`🔄 Intento \${retries + 1}/\${maxRetries} - Backend no responde aún...\`);
        }
        return false;
      };
      
      if (await checkConnection()) return;
      
      const interval = setInterval(async () => {
        retries++;
        
        if (await checkConnection()) {
          clearInterval(interval);
          return;
        }
        
        if (retries >= maxRetries) {
          clearInterval(interval);
          console.error('❌ Backend no conectado después de 30 segundos');
          document.title = 'GESCO Desktop - Sin Conexión';
          window.dispatchEvent(new CustomEvent('backend-connection-failed'));
        }
      }, 1000);
    })();
  `);
}

function createErrorPage(message) {
  const errorHtml = `<!DOCTYPE html>
  <html>
    <head>
      <title>Error - GESCO Desktop</title>
      <style>
        body { 
          font-family: system-ui, sans-serif;
          padding: 40px; 
          background: #f8fafc; 
          display: flex;
          align-items: center;
          justify-content: center;
          min-height: 100vh;
          margin: 0;
        }
        .container {
          background: white;
          padding: 40px;
          border-radius: 12px;
          box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
          text-align: center;
          max-width: 500px;
        }
        h1 { color: #dc2626; margin-bottom: 20px; }
        button {
          background: #3b82f6;
          color: white;
          border: none;
          padding: 12px 24px;
          border-radius: 6px;
          cursor: pointer;
          margin: 10px;
        }
      </style>
    </head>
    <body>
      <div class="container">
        <h1>⚠️ Error</h1>
        <p>${message}</p>
        <button onclick="location.reload()">🔄 Reintentar</button>
      </div>
    </body>
  </html>`;
  
  return 'data:text/html;charset=utf-8,' + encodeURIComponent(errorHtml);
}

app.whenReady().then(async () => {
  console.log('⚡ Electron listo');
  
  createWindow();
  console.log('✅ Ventana creada, cargando frontend...');
  
  if (!isDev) {
    startBackend().catch(error => {
      console.error('❌ Error iniciando backend:', error);
    });
  }
  
  console.log('🎉 GESCO Desktop iniciando en modo rápido');
});

app.on('window-all-closed', () => {
  if (backendProcess && !backendProcess.killed) {
    console.log('🔄 Terminando backend...');
    backendProcess.kill();
  }
  
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

app.on('before-quit', () => {
  if (backendProcess && !backendProcess.killed) {
    console.log('🔄 Limpieza: terminando backend...');
    backendProcess.kill();
  }
});

process.on('exit', () => {
  if (backendProcess && !backendProcess.killed) {
    backendProcess.kill();
  }
});

console.log('✅ Script principal cargado');