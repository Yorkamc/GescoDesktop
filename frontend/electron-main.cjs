// frontend/electron-main.cjs - SOLUCION PARA EJECUTABLE QUE NO CARGA
const { app, BrowserWindow } = require('electron');
const path = require('path');
const { readFileSync, existsSync } = require('fs');

// Detectar modo desarrollo de forma más precisa
const isDev = process.env.NODE_ENV === 'development' || 
              process.env.ELECTRON_IS_DEV === '1' || 
              !app.isPackaged ||
              process.defaultApp;

let mainWindow;

console.log('=== GESCO DESKTOP DEBUG ===');
console.log('isDev:', isDev);
console.log('app.isPackaged:', app.isPackaged);
console.log('process.defaultApp:', process.defaultApp);
console.log('__dirname:', __dirname);
console.log('process.resourcesPath:', process.resourcesPath);
console.log('app.getAppPath():', app.getAppPath());
console.log('===========================');

function findDistPath() {
  // Posibles ubicaciones del dist en diferentes configuraciones
  const possiblePaths = [
    path.join(__dirname, 'dist'),                           // Desarrollo
    path.join(__dirname, '..', 'dist'),                     // Build diferente
    path.join(process.resourcesPath, 'app', 'dist'),        // Producción empaquetada
    path.join(process.resourcesPath, 'dist'),               // Producción alternativa
    path.join(app.getAppPath(), 'dist'),                    // App path
    path.join(__dirname, '..', '..', 'dist')                // Otra estructura
  ];

  console.log('\n🔍 Buscando carpeta dist...');
  
  for (const distPath of possiblePaths) {
    console.log(`Verificando: ${distPath}`);
    
    if (existsSync(distPath)) {
      const indexPath = path.join(distPath, 'index.html');
      if (existsSync(indexPath)) {
        console.log(`✅ ENCONTRADO: ${distPath}`);
        console.log(`   index.html existe: ${indexPath}`);
        return distPath;
      } else {
        console.log(`❌ dist existe pero falta index.html: ${indexPath}`);
      }
    } else {
      console.log(`❌ No existe: ${distPath}`);
    }
  }

  console.error('❌ NO SE ENCONTRÓ carpeta dist válida');
  return null;
}

function createWindow() {
  console.log('\n🪟 Creando ventana principal...');
  
  mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1000,
    minHeight: 700,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      devTools: true,
      webSecurity: false, // Temporalmente desactivado para debugging
      allowRunningInsecureContent: true
    },
    show: true, // Mostrar inmediatamente para debugging
    backgroundColor: '#f0f0f0',
    titleBarStyle: 'default'
  });

  // Abrir DevTools inmediatamente para debugging
  mainWindow.webContents.openDevTools();

  let urlToLoad;
  
  if (isDev) {
    // DESARROLLO: Servidor Vite
    urlToLoad = 'http://localhost:5173';
    console.log('🌐 MODO DESARROLLO');
    console.log('URL:', urlToLoad);
  } else {
    // PRODUCCIÓN: Buscar archivos locales
    console.log('📦 MODO PRODUCCIÓN');
    
    const distPath = findDistPath();
    
    if (distPath) {
      const indexPath = path.join(distPath, 'index.html');
      urlToLoad = `file://${indexPath}`;
      
      console.log(`📄 Cargando desde: ${urlToLoad}`);
      
      // Verificar contenido del archivo
      try {
        const content = readFileSync(indexPath, 'utf8');
        console.log(`📋 Tamaño del archivo: ${content.length} caracteres`);
        console.log(`📋 Primeros 200 caracteres: ${content.substring(0, 200)}`);
      } catch (error) {
        console.error('❌ Error leyendo index.html:', error.message);
      }
    } else {
      // Crear página de error si no encontramos los archivos
      urlToLoad = createEmergencyPage();
    }
  }

  // Eventos detallados para debugging
  mainWindow.webContents.on('did-start-loading', () => {
    console.log('⏳ Iniciando carga...');
  });

  mainWindow.webContents.on('did-finish-load', () => {
    console.log('✅ Carga completada');
  });

  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription, validatedURL) => {
    console.error('\n❌ FALLO AL CARGAR:');
    console.error('  Código:', errorCode);
    console.error('  Descripción:', errorDescription);
    console.error('  URL validada:', validatedURL);
    
    // Mostrar página de diagnostico
    showDiagnosticPage(errorCode, errorDescription, validatedURL);
  });

  // Log de errores de consola
  mainWindow.webContents.on('console-message', (event, level, message, line, sourceId) => {
    console.log(`[RENDERER ${level}] ${message} (${sourceId}:${line})`);
  });

  // Log de navegación
  mainWindow.webContents.on('did-start-navigation', (event, url) => {
    console.log('🧭 Navegando a:', url);
  });

  mainWindow.webContents.on('dom-ready', () => {
    console.log('📄 DOM listo');
  });

  // Capturar todos los errores posibles
  mainWindow.webContents.on('crashed', () => {
    console.error('💥 RENDERER CRASHED');
  });

  mainWindow.webContents.on('unresponsive', () => {
    console.error('💥 RENDERER UNRESPONSIVE');
  });

  // Cargar la URL
  console.log(`🚀 Intentando cargar: ${urlToLoad}`);
  mainWindow.loadURL(urlToLoad).catch(error => {
    console.error('❌ Error en loadURL:', error);
    showDiagnosticPage(-1, error.message, urlToLoad);
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

function createEmergencyPage() {
  const emergencyHtml = `
    <!DOCTYPE html>
    <html lang="es">
    <head>
      <meta charset="UTF-8">
      <title>GESCO Desktop - Diagnóstico</title>
      <style>
        body { 
          font-family: system-ui, -apple-system, sans-serif; 
          margin: 0; padding: 20px; 
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          min-height: 100vh; display: flex; align-items: center; justify-content: center;
        }
        .container { 
          background: white; padding: 40px; border-radius: 10px; 
          max-width: 600px; box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        }
        .title { color: #dc3545; font-size: 24px; margin-bottom: 20px; }
        .info { background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0; }
        .paths { font-family: monospace; font-size: 12px; line-height: 1.4; }
      </style>
    </head>
    <body>
      <div class="container">
        <div class="title">🔧 GESCO Desktop - Diagnóstico</div>
        <p>No se pudieron encontrar los archivos de la aplicación.</p>
        
        <div class="info">
          <strong>Información del sistema:</strong><br>
          <div class="paths">
            __dirname: ${__dirname}<br>
            process.resourcesPath: ${process.resourcesPath}<br>
            app.getAppPath(): ${app.getAppPath()}<br>
            app.isPackaged: ${app.isPackaged}
          </div>
        </div>

        <p><strong>Posibles soluciones:</strong></p>
        <ol>
          <li>Regenera el ejecutable con: <code>build-electron.ps1</code></li>
          <li>Verifica que la carpeta dist/ contiene index.html</li>
          <li>Contacta al soporte técnico</li>
        </ol>

        <button onclick="window.location.reload()" 
                style="background: #007bff; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer;">
          Reintentar
        </button>
      </div>
    </body>
    </html>
  `;

  return 'data:text/html;charset=utf-8,' + encodeURIComponent(emergencyHtml);
}

function showDiagnosticPage(errorCode, errorDescription, validatedURL) {
  const diagnosticHtml = `
    <!DOCTYPE html>
    <html lang="es">
    <head>
      <meta charset="UTF-8">
      <title>GESCO Desktop - Error de Carga</title>
      <style>
        body { 
          font-family: system-ui, -apple-system, sans-serif; 
          margin: 0; padding: 20px; 
          background: linear-gradient(135deg, #dc3545 0%, #fd7e14 100%);
          min-height: 100vh; display: flex; align-items: center; justify-content: center;
        }
        .container { 
          background: white; padding: 40px; border-radius: 10px; 
          max-width: 700px; box-shadow: 0 10px 30px rgba(0,0,0,0.3);
        }
        .title { color: #dc3545; font-size: 24px; margin-bottom: 20px; }
        .error-box { background: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 5px; margin: 15px 0; }
        .code { font-family: monospace; background: #e9ecef; padding: 2px 5px; border-radius: 3px; }
      </style>
    </head>
    <body>
      <div class="container">
        <div class="title">❌ Error al cargar GESCO Desktop</div>
        
        <div class="error-box">
          <strong>Detalles del error:</strong><br>
          Código: <span class="code">${errorCode}</span><br>
          Descripción: <span class="code">${errorDescription}</span><br>
          URL: <span class="code">${validatedURL}</span>
        </div>

        <p><strong>Información de debugging:</strong></p>
        <ul>
          <li>Modo desarrollo: ${isDev}</li>
          <li>App empaquetada: ${app.isPackaged}</li>
          <li>Directorio actual: <span class="code">${__dirname}</span></li>
        </ul>

        <p><strong>Pasos para solucionar:</strong></p>
        <ol>
          <li>Cierra esta aplicación</li>
          <li>Ejecuta: <span class="code">build-electron.ps1</span></li>
          <li>Vuelve a abrir el ejecutable generado</li>
        </ol>

        <button onclick="window.location.reload()" 
                style="background: #dc3545; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer; margin-right: 10px;">
          Reintentar
        </button>
        
        <button onclick="require('electron').shell.openExternal('http://localhost:5173')" 
                style="background: #28a745; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer;">
          Abrir en navegador
        </button>
      </div>
    </body>
    </html>
  `;

  mainWindow.loadURL('data:text/html;charset=utf-8,' + encodeURIComponent(diagnosticHtml));
}

// Configuración de la app
app.whenReady().then(() => {
  console.log('⚡ Electron listo');
  createWindow();
});

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

console.log('📱 Electron configurado');