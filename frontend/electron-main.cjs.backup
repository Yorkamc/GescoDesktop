const { app, BrowserWindow } = require('electron');
const path = require('path');
const { existsSync } = require('fs');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;

console.log('=== GESCO DESKTOP ELECTRON DEBUG ===');
console.log('isDev:', isDev);
console.log('app.isPackaged:', app.isPackaged);
console.log('__dirname:', __dirname);
console.log('process.resourcesPath:', process.resourcesPath);

function createWindow() {
  console.log('Creando ventana...');
  
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: true,
      webSecurity: false
    },
    show: false,
    backgroundColor: '#ffffff'
  });

  // Siempre abrir DevTools para debugging
  mainWindow.webContents.openDevTools();

  let urlToLoad;
  
  if (isDev) {
    urlToLoad = 'http://localhost:5173';
    console.log('MODO DESARROLLO - URL:', urlToLoad);
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
      console.log('Probando ruta:', testPath);
      if (existsSync(testPath)) {
        indexPath = testPath;
        console.log('ARCHIVO ENCONTRADO:', indexPath);
        break;
      } else {
        console.log('No existe:', testPath);
      }
    }
    
    if (indexPath) {
      urlToLoad = 'file://' + indexPath.replace(/\\/g, '/');
      console.log('URL final:', urlToLoad);
    } else {
      console.error('ERROR: No se encontro index.html');
      urlToLoad = createErrorPage();
    }
  }

  // Eventos de debugging detallados
  mainWindow.webContents.on('did-start-loading', () => {
    console.log('INICIANDO CARGA...');
  });

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('CARGA COMPLETADA');
    mainWindow.show();
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL) => {
    console.error('FALLO EN CARGA:');
    console.error('  Codigo:', errorCode);
    console.error('  Descripcion:', errorDescription);
    console.error('  URL:', validatedURL);
    mainWindow.show();
  });

  mainWindow.webContents.on('console-message', (event, level, message, line, sourceId) => {
    console.log('[RENDERER ' + level.toUpperCase() + ']', message, '(' + sourceId + ':' + line + ')');
  });

  mainWindow.webContents.on('dom-ready', () => {
    console.log('DOM LISTO');
    
    // Inyectar script para verificar el estado de React
    mainWindow.webContents.executeJavaScript(
      console.log('=== ESTADO DEL DOM ===');
      console.log('Elemento root:', document.getElementById('root'));
      console.log('Contenido root:', document.getElementById('root')?.innerHTML?.substring(0, 200));
      console.log('Scripts cargados:', document.scripts.length);
      console.log('Hojas de estilo:', document.styleSheets.length);
      console.log('=== FIN ESTADO ===');
    );
  });

  console.log('Cargando URL:', urlToLoad);
  mainWindow.loadURL(urlToLoad).catch(error => {
    console.error('Error en loadURL:', error);
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

function createErrorPage() {
  const errorHtml = '<!DOCTYPE html><html><head><title>Debug - GESCO Desktop</title><style>body{font-family:monospace;padding:20px;background:#f0f0f0;}pre{background:#fff;padding:10px;border:1px solid #ccc;}</style></head><body><h1>Debug Info</h1><pre>__dirname: ' + __dirname + '\napp.isPackaged: ' + app.isPackaged + '\nprocess.resourcesPath: ' + process.resourcesPath + '</pre><button onclick="location.reload()">Reload</button></body></html>';
  return 'data:text/html;charset=utf-8,' + encodeURIComponent(errorHtml);
}

app.whenReady().then(() => {
  console.log('ELECTRON LISTO');
  createWindow();
});

app.on('window-all-closed', () => {
  console.log('TODAS LAS VENTANAS CERRADAS');
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

console.log('Electron main script cargado');