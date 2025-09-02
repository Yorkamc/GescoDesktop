const { app, BrowserWindow } = require('electron');
const path = require('path');
const { existsSync } = require('fs');
const { spawn } = require('child_process');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;
let backendProcess;

console.log('=== GESCO DESKTOP STANDALONE ===');
console.log('isDev:', isDev);

function startBackend() {
  if (isDev) {
    console.log('🔧 Modo desarrollo: backend externo esperado');
    return Promise.resolve();
  }

  return new Promise((resolve, reject) => {
    console.log('🚀 Iniciando backend integrado...');
    
    // Buscar el ejecutable del backend
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
      reject(new Error('Backend no encontrado'));
      return;
    }

    // Iniciar el backend
    backendProcess = spawn(backendPath, [], {
      detached: false,
      stdio: ['ignore', 'pipe', 'pipe']
    });

    backendProcess.stdout.on('data', (data) => {
      const output = data.toString();
      console.log('[BACKEND]', output);
      
      // Detectar cuando el backend está listo
      if (output.includes('Now listening on: http://localhost:5100')) {
        console.log('✅ Backend listo');
        resolve();
      }
    });

    backendProcess.stderr.on('data', (data) => {
      console.error('[BACKEND ERROR]', data.toString());
    });

    backendProcess.on('error', (error) => {
      console.error('❌ Error iniciando backend:', error);
      reject(error);
    });

    backendProcess.on('exit', (code) => {
      console.log(`🔄 Backend terminó con código: ${code}`);
    });

    // Timeout si el backend no inicia en 30 segundos
    setTimeout(() => {
      if (backendProcess && !backendProcess.killed) {
        console.log('⏰ Backend tardó mucho, continuando de cualquier manera...');
        resolve();
      }
    }, 30000);
  });
}

function createWindow() {
  console.log('📱 Creando ventana principal...');
  
  mainWindow = new BrowserWindow({
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
    icon: path.join(__dirname, 'assets', 'icon.png'),
    title: 'GESCO Desktop'
  });

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
    
    // Verificar conectividad con backend
    mainWindow.webContents.executeJavaScript(`
      console.log('🔍 Verificando conectividad con backend...');
      
      fetch('http://localhost:5100/api/system/health')
        .then(response => response.json())
        .then(data => {
          console.log('✅ Backend conectado:', data);
          document.title = 'GESCO Desktop - Conectado';
        })
        .catch(error => {
          console.error('❌ Backend no conectado:', error);
          document.title = 'GESCO Desktop - Sin Conexión';
          
          // Mostrar mensaje de error si no hay backend
          const root = document.getElementById('root');
          if (root && root.innerHTML.trim() === '') {
            root.innerHTML = \`
              <div style="display: flex; align-items: center; justify-content: center; min-height: 100vh; font-family: Arial;">
                <div style="text-align: center; background: white; padding: 40px; border-radius: 12px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);">
                  <h1 style="color: #dc2626; margin-bottom: 20px;">🚫 Sin Conexión al Servidor</h1>
                  <p style="margin-bottom: 20px;">No se puede conectar al servidor backend.</p>
                  <p style="margin-bottom: 20px; color: #6b7280;">Verifica que el servidor esté corriendo en localhost:5100</p>
                  <button onclick="location.reload()" style="background: #3b82f6; color: white; border: none; padding: 12px 24px; border-radius: 6px; cursor: pointer;">
                    🔄 Reintentar
                  </button>
                </div>
              </div>
            \`;
          }
        });
    `);
    
    setTimeout(() => {
      mainWindow.show();
    }, 1000);
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
        <button onclick="require('electron').shell.openExternal('https://github.com/tu-usuario/gesco-desktop/issues')">
          📋 Reportar Problema
        </button>
      </div>
    </body>
  </html>`;
  
  return 'data:text/html;charset=utf-8,' + encodeURIComponent(errorHtml);
}

// Secuencia de inicio
app.whenReady().then(async () => {
  console.log('⚡ Electron listo');
  
  try {
    // Iniciar backend primero (solo en producción)
    await startBackend();
    
    // Crear ventana
    createWindow();
    
    console.log('🎉 GESCO Desktop iniciado completamente');
    
  } catch (error) {
    console.error('❌ Error durante el inicio:', error);
    
    // Crear ventana de cualquier manera
    createWindow();
  }
});

app.on('window-all-closed', () => {
  // Terminar el backend si está corriendo
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

// Limpiar al salir
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