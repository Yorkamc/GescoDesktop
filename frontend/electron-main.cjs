const { app, BrowserWindow } = require('electron');
const path = require('path');
const isDev = process.env.NODE_ENV === 'development';

let mainWindow;

function createWindow() {
  console.log('🚀 Creando ventana de Electron...');
  
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 800,
    minHeight: 600,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      // Habilitar DevTools en desarrollo
      devTools: isDev,
    },
    show: false, // No mostrar hasta que esté listo
    icon: path.join(__dirname, 'build/icon.png'), // Opcional
  });

  // Mostrar cuando esté listo para prevenir flash
  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
    
    // Abrir DevTools solo en desarrollo
    if (isDev) {
      mainWindow.webContents.openDevTools();
    }
  });

  // Cargar la aplicación
  if (isDev) {
    console.log('📡 Modo desarrollo: cargando desde http://localhost:5173');
    mainWindow.loadURL('http://localhost:5173');
    
    // Recargar en caso de error de conexión
    mainWindow.webContents.on('did-fail-load', () => {
      console.log('❌ Error cargando, reintentando en 1 segundo...');
      setTimeout(() => {
        mainWindow.reload();
      }, 1000);
    });
  } else {
    console.log('📦 Modo producción: cargando desde archivo local');
    mainWindow.loadFile(path.join(__dirname, 'dist/index.html'));
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Log útiles para debugging
  mainWindow.webContents.on('did-finish-load', () => {
    console.log('✅ Aplicación cargada correctamente');
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    console.error('❌ Error cargando aplicación:', errorCode, errorDescription);
  });
}

app.whenReady().then(() => {
  console.log('🔧 Electron listo, creando ventana...');
  createWindow();
});

app.on('window-all-closed', () => {
  console.log('🚪 Cerrando aplicación...');
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  console.log('🔄 Reactivando aplicación...');
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// Log del entorno
console.log('🌍 Entorno:', isDev ? 'DESARROLLO' : 'PRODUCCIÓN');
console.log('📁 Directorio actual:', __dirname);