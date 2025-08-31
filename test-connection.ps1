# test-connection.ps1 - Probar conexion entre frontend y backend
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTICANDO CONEXION FRONTEND-BACKEND" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "PASO 1: Verificar que el backend responde..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5100/api/system/health" -Method GET -TimeoutSec 5
    Write-Host "   Backend responde correctamente" -ForegroundColor Green
    Write-Host "   Status: $($response.status)" -ForegroundColor Cyan
} catch {
    Write-Host "   ERROR: Backend no responde en http://localhost:5100" -ForegroundColor Red
    Write-Host "   Verifica que el backend este corriendo con: dotnet run" -ForegroundColor Gray
    exit 1
}

Write-Host ""
Write-Host "PASO 2: Probar endpoints criticos..." -ForegroundColor Yellow

# Probar endpoint de stats (sin autenticacion)
try {
    $statsResponse = Invoke-RestMethod -Uri "http://localhost:5100/api/stats" -Method GET -TimeoutSec 5
    Write-Host "   Endpoint /api/stats funciona" -ForegroundColor Green
} catch {
    Write-Host "   PROBLEMA: /api/stats no responde" -ForegroundColor Yellow
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "PASO 3: Analizar build del frontend..." -ForegroundColor Yellow

if (-not (Test-Path "frontend/dist/index.html")) {
    Write-Host "   ERROR: No existe frontend/dist/index.html" -ForegroundColor Red
    exit 1
}

# Analizar el contenido del index.html
$indexContent = Get-Content "frontend/dist/index.html" -Raw
Write-Host "   Analizando index.html..." -ForegroundColor Gray

# Verificar referencias a assets
if ($indexContent -match 'src="\.\/assets\/main\.js"') {
    Write-Host "   Referencias JS correctas (./ prefix)" -ForegroundColor Green
} elseif ($indexContent -match 'src="\/assets\/main\.js"') {
    Write-Host "   PROBLEMA: Referencias JS usan / absoluto" -ForegroundColor Red
    Write-Host "   Necesita ser ./assets/ para Electron" -ForegroundColor Yellow
} else {
    Write-Host "   ADVERTENCIA: No se encontraron referencias JS esperadas" -ForegroundColor Yellow
}

# Verificar que el main.js existe
$mainJsPath = "frontend/dist/assets/main.js"
if (Test-Path $mainJsPath) {
    $jsSize = [Math]::Round((Get-Item $mainJsPath).Length / 1KB, 1)
    Write-Host "   main.js encontrado: $jsSize KB" -ForegroundColor Green
    
    # Verificar contenido del JS
    $jsContent = Get-Content $mainJsPath -Raw
    if ($jsContent -match 'localhost:5100') {
        Write-Host "   JS contiene referencia a localhost:5100" -ForegroundColor Green
    } else {
        Write-Host "   ADVERTENCIA: JS no contiene localhost:5100" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ERROR: main.js no encontrado" -ForegroundColor Red
}

Write-Host ""
Write-Host "PASO 4: Crear version de prueba del electron-main.cjs..." -ForegroundColor Yellow

Set-Location "frontend"

# Crear version mejorada para debugging
$testElectronMain = @'
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
          console.log('✅ Backend conectado:', data);
        })
        .catch(error => {
          console.error('❌ Error conectando backend:', error);
        });
        
      // Verificar que React esta cargado
      setTimeout(() => {
        const root = document.getElementById('root');
        console.log('Root element:', root);
        console.log('Root content:', root ? root.innerHTML.substring(0, 200) : 'NO ENCONTRADO');
        
        if (!root || root.innerHTML.trim() === '') {
          console.error('❌ React no se cargo correctamente');
        } else {
          console.log('✅ React parece estar cargado');
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
'@

# Hacer backup y escribir nueva version
Copy-Item "electron-main.cjs" "electron-main.cjs.backup" -ErrorAction SilentlyContinue
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText((Join-Path (Get-Location) "electron-main.cjs"), $testElectronMain, $utf8NoBom)

Write-Host "   electron-main.cjs de prueba creado" -ForegroundColor Green

Set-Location ".."

Write-Host ""
Write-Host "PASO 5: Instrucciones para debugging..." -ForegroundColor Yellow

Write-Host ""
Write-Host "PARA DEBUGGEAR EL PROBLEMA:" -ForegroundColor White
Write-Host "1. Asegurate que el backend este corriendo" -ForegroundColor Gray
Write-Host "2. Abre la aplicacion Electron (ejecutable)" -ForegroundColor Gray
Write-Host "3. Las DevTools se abriran automaticamente" -ForegroundColor Gray
Write-Host "4. En DevTools, ve a la pestana Console" -ForegroundColor Gray
Write-Host "5. Busca estos mensajes:" -ForegroundColor Gray
Write-Host "   - '>>> CARGA INICIADA' y '>>> CARGA TERMINADA'" -ForegroundColor DarkGray
Write-Host "   - 'Backend conectado' o 'Error conectando backend'" -ForegroundColor DarkGray
Write-Host "   - 'React parece estar cargado' o 'React no se cargo'" -ForegroundColor DarkGray
Write-Host "6. Si hay errores rojos, copia el mensaje completo" -ForegroundColor Gray
Write-Host ""
Write-Host "ERRORES COMUNES A BUSCAR:" -ForegroundColor Yellow
Write-Host "   - 'Failed to fetch' = problema de conexion con backend" -ForegroundColor Gray
Write-Host "   - 'net::ERR_FILE_NOT_FOUND' = archivos no encontrados" -ForegroundColor Gray
Write-Host "   - 'SyntaxError' = problema en el JavaScript" -ForegroundColor Gray
Write-Host "   - 'Access to fetch blocked by CORS' = problema de CORS" -ForegroundColor Gray
Write-Host ""
Write-Host "REBUILD SI ES NECESARIO:" -ForegroundColor White
Write-Host "   cd frontend" -ForegroundColor Gray
Write-Host "   npm run build" -ForegroundColor Gray
Write-Host "   npm run electron:build" -ForegroundColor Gray

Write-Host ""
Write-Host "DIAGNOSTICO COMPLETADO" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "PROXIMOS PASOS:" -ForegroundColor White
Write-Host "   1. Ejecuta la aplicacion y revisa DevTools Console" -ForegroundColor Gray
Write-Host "   2. Reporta los errores especificos que veas" -ForegroundColor Gray
Write-Host "   3. Si no hay errores pero sigue en blanco, hay problema de routing" -ForegroundColor Gray
Write-Host "=============================================" -ForegroundColor Green