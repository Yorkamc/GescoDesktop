const { app, BrowserWindow } = require('electron');
const path = require('path');
const { existsSync } = require('fs');

const isDev = process.env.NODE_ENV === 'development' || !app.isPackaged;
let mainWindow;

function createWindow() {
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
    autoHideMenuBar: !isDev,
    icon: path.join(__dirname, 'assets', 'icon.png')
  });

  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  let urlToLoad;
  
  if (isDev) {
    urlToLoad = 'http://localhost:5173';
  } else {
    const possiblePaths = [
      path.join(__dirname, 'dist', 'index.html'),
      path.join(__dirname, '..', 'dist', 'index.html'),
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
      urlToLoad = createErrorPage();
    }
  }

  mainWindow.webContents.on('did-finish-load', () => {
    mainWindow.show();
    
    // Verificación post-carga solo en desarrollo
    if (isDev) {
      mainWindow.webContents.executeJavaScript(`
        setTimeout(() => {
          const root = document.getElementById('root');
          if (!root || root.innerHTML.trim() === '') {
            console.error('React no se cargó correctamente');
          }
        }, 2000);
      `);
    }
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    console.error('Error cargando página:', errorCode, errorDescription);
    mainWindow.show();
  });

  mainWindow.webContents.on('will-navigate', (event, navigationUrl) => {
    if (!isDev && !navigationUrl.startsWith('file://') && !navigationUrl.startsWith('http://localhost')) {
      event.preventDefault();
    }
  });

  mainWindow.loadURL(urlToLoad).catch(console.error);

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  mainWindow.webContents.setWindowOpenHandler(() => {
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
          font-family: system-ui, sans-serif;
          padding: 40px; 
          background: #f8fafc; 
          color: #374151;
          text-align: center;
        }
        .container {
          background: white;
          padding: 40px;
          border-radius: 12px;
          box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
          max-width: 500px;
          margin: 0 auto;
        }
        h1 { color: #dc2626; }
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
        <h1>Error de Carga</h1>
        <p>No se pudo cargar la aplicación React.</p>
        <p>Asegúrate de haber ejecutado <code>npm run build</code></p>
        <button onclick="location.reload()">Reintentar</button>
      </div>
    </body>
  </html>`;
  
  return 'data:text/html;charset=utf-8,' + encodeURIComponent(errorHtml);
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

process.on('uncaughtException', console.error);
process.on('unhandledRejection', console.error);