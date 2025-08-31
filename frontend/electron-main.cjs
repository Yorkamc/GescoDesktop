const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const { existsSync } = require('fs');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;

console.log('=== GESCO DESKTOP ELECTRON TEST ===');
console.log('isDev:', isDev);
console.log('__dirname:', __dirname);

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: true,
      webSecurity: false,  // TEMPORAL: para debugging
      allowRunningInsecureContent: true
    },
    show: false,
    backgroundColor: '#f8fafc'
  });

  // SIEMPRE abrir DevTools para ver errores
  mainWindow.webContents.openDevTools();

  let urlToLoad;
  
  if (isDev) {
    urlToLoad = 'http://localhost:5173';
    console.log('MODO DESARROLLO');
  } else {
    console.log('MODO PRODUCCION - Buscando archivos...');
    
    const possiblePaths = [
      path.join(__dirname, 'dist', 'index.html'),
      path.join(__dirname, '..', 'dist', 'index.html'),
      path.join(process.resourcesPath, 'app', 'dist', 'index.html'),
      path.join(app.getAppPath(), 'dist', 'index.html')
    ];
    
    let indexPath = null;
    for (const testPath of possiblePaths) {
      console.log('Probando:', testPath);
      if (existsSync(testPath)) {
        indexPath = testPath;
        console.log('ENCONTRADO:', indexPath);
        break;
      }
    }
    
    if (indexPath) {
      urlToLoad = 'file://' + indexPath.replace(/\\/g, '/');
    } else {
      console.error('ERROR: index.html no encontrado');
      urlToLoad = 'data:text/html,<h1>Error: index.html no encontrado</h1>';
    }
  }

  console.log('URL a cargar:', urlToLoad);

  // Eventos detallados de debugging
  mainWindow.webContents.on('did-start-loading', () => {
    console.log('>>> CARGA INICIADA');
  });

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('>>> CARGA TERMINADA');
    
    // Inyectar script de prueba
    mainWindow.webContents.executeJavaScript(`
      console.log('=== TEST DE CONECTIVIDAD ===');
      
      // Probar fetch al backend
      fetch('http://localhost:5100/api/system/health')
        .then(response => response.json())
        .then(data => {
          console.log('âœ… Backend conectado:', data);
        })
        .catch(error => {
          console.error('âŒ Error conectando backend:', error);
        });
        
      // Verificar que React esta cargado
      setTimeout(() => {
        const root = document.getElementById('root');
        console.log('Root element:', root);
        console.log('Root content:', root ? root.innerHTML.substring(0, 200) : 'NO ENCONTRADO');
        
        if (!root || root.innerHTML.trim() === '') {
          console.error('âŒ React no se cargo correctamente');
        } else {
          console.log('âœ… React parece estar cargado');
        }
      }, 2000);
    `);
    
    mainWindow.show();
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL) => {
    console.error('>>> FALLO EN CARGA');
    console.error('Codigo:', errorCode);
    console.error('Descripcion:', errorDescription);
    console.error('URL:', validatedURL);
    mainWindow.show();
  });

  mainWindow.webContents.on('console-message', (event, level, message, line, sourceId) => {
    const prefix = level === 1 ? '[WARN]' : level === 2 ? '[ERROR]' : '[LOG]';
    console.log(`${prefix} ${message} (${sourceId}:${line})`);
  });

  // Interceptar requests de red
  mainWindow.webContents.session.webRequest.onBeforeRequest((details, callback) => {
    console.log('>>> REQUEST:', details.method, details.url);
    callback({});
  });

  mainWindow.webContents.session.webRequest.onErrorOccurred((details) => {
    console.error('>>> REQUEST ERROR:', details.url, details.error);
  });

  // Cargar la pagina
  mainWindow.loadURL(urlToLoad).catch(error => {
    console.error('Error en loadURL:', error);
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});