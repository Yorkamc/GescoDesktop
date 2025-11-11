const { app, BrowserWindow } = require('electron');
const path = require('path');
const { existsSync } = require('fs');
const { spawn } = require('child_process');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;
let backendProcess;

// IMPORTANTE: Forzar el nombre de la app sin versión
const fixedAppName = 'GESCODesktop';
const currentPath = app.getPath('userData');
if (currentPath.includes('GESCO Desktop') || currentPath.includes('gesco-desktop')) {
  const newPath = path.join(app.getPath('appData'), fixedAppName);
  app.setPath('userData', newPath);
}

function getIconPath() {
  const platform = process.platform;
  let iconName;
  let iconSubfolder;
  
  if (platform === 'win32') {
    iconName = 'icon.ico';
    iconSubfolder = 'win';
  } else if (platform === 'darwin') {
    iconName = 'icon.icns';
    iconSubfolder = 'mac';
  } else {
    iconName = 'icon-512.png';
    iconSubfolder = 'png';
  }
  
  const possiblePaths = [
    // En producción empaquetado
    path.join(process.resourcesPath, 'build', 'icons', iconSubfolder, iconName),
    path.join(process.resourcesPath, 'app.asar.unpacked', 'build', 'icons', iconSubfolder, iconName),
    
    // En desarrollo
    path.join(__dirname, 'build', 'icons', iconSubfolder, iconName),
    path.join(__dirname, 'build', 'icons', 'png', 'icon-512.png'),
    path.join(__dirname, 'build', iconName),
  ];
  
  for (const iconPath of possiblePaths) {
    if (existsSync(iconPath)) {
      return iconPath;
    }
  }
  
  return null;
}

function startBackend() {
  if (isDev) {
    return Promise.resolve();
  }

  return new Promise((resolve) => {
    const backendPaths = [
      path.join(process.resourcesPath, 'backend', 'Gesco.Desktop.UI.exe'),
      path.join(process.resourcesPath, 'app.asar.unpacked', 'backend', 'Gesco.Desktop.UI.exe'),
      path.join(__dirname, 'backend', 'Gesco.Desktop.UI.exe'),
    ];
    
    let backendPath = null;
    for (const testPath of backendPaths) {
      if (existsSync(testPath)) {
        backendPath = testPath;
        break;
      }
    }
    
    if (!backendPath) {
      console.error('Backend no encontrado');
      resolve();
      return;
    }

    backendProcess = spawn(backendPath, [], {
      detached: false,
      stdio: ['ignore', 'pipe', 'pipe']
    });

    backendProcess.stdout.on('data', (data) => {
      const output = data.toString();
      if (isDev) console.log('[BACKEND]', output);
      
      if (output.includes('Now listening on: http://localhost:5100')) {
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('backend-ready');
        }
      }
    });

    backendProcess.stderr.on('data', (data) => {
      if (isDev) console.error('[BACKEND ERROR]', data.toString());
    });

    backendProcess.on('error', (error) => {
      console.error('Error iniciando backend:', error);
    });

    setTimeout(resolve, 500);
  });
}

function createWindow() {
  const iconPath = getIconPath();
  
  const windowConfig = {
    width: 1400,
    height: 900,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: isDev, // ✅ Solo en desarrollo
      webSecurity: !isDev,
      backgroundThrottling: false,
      preload: path.join(__dirname, 'preload.cjs')
    },
    show: false,
    backgroundColor: '#f8fafc',
    autoHideMenuBar: true,
    title: 'GESCO Desktop'
  };
  
  if (iconPath) {
    windowConfig.icon = iconPath;
  }
  
  mainWindow = new BrowserWindow(windowConfig);

  // ✅ Configurar icono específicamente para Windows
  if (process.platform === 'win32' && iconPath) {
    mainWindow.setIcon(iconPath);
    // ✅ CRÍTICO: Configurar overlay icon para la barra de tareas
    mainWindow.setOverlayIcon(iconPath, 'GESCO Desktop');
  }

  // ✅ Solo abrir DevTools en desarrollo
  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  let urlToLoad;
  
  if (isDev) {
    urlToLoad = 'http://localhost:5173';
  } else {
    const possiblePaths = [
      path.join(__dirname, 'dist', 'index.html'),
      path.join(process.resourcesPath, 'app', 'dist', 'index.html'),
      path.join(app.getAppPath(), 'dist', 'index.html')
    ];
    
    let indexPath = null;
    for (const testPath of possiblePaths) {
      if (existsSync(testPath)) {
        indexPath = testPath;
        break;
      }
    }
    
    if (indexPath) {
      urlToLoad = 'file://' + indexPath.replace(/\\/g, '/');
    } else {
      urlToLoad = createErrorPage('Frontend no encontrado');
    }
  }

  mainWindow.webContents.on('did-finish-load', () => {
    mainWindow.show();
    checkBackendConnection();
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    if (isDev) console.error('Error cargando ventana:', errorCode, errorDescription);
    mainWindow.show();
  });

  mainWindow.loadURL(urlToLoad).catch(error => {
    console.error('Error en loadURL:', error);
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

function checkBackendConnection() {
  if (!mainWindow || mainWindow.isDestroyed()) return;
  
  mainWindow.webContents.executeJavaScript(`
    (async () => {
      let retries = 0;
      const maxRetries = 30;
      
      const checkConnection = async () => {
        try {
          const response = await fetch('http://localhost:5100/api/system/health', {
            signal: AbortSignal.timeout(3000)
          });
          
          if (response.ok) {
            window.dispatchEvent(new CustomEvent('backend-connected'));
            return true;
          }
        } catch (error) {
          // Silencioso en producción
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
  createWindow();
  
  if (!isDev) {
    await startBackend();
  }
});

app.on('window-all-closed', () => {
  if (backendProcess && !backendProcess.killed) {
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
    backendProcess.kill();
  }
});

process.on('exit', () => {
  if (backendProcess && !backendProcess.killed) {
    backendProcess.kill();
  }
});