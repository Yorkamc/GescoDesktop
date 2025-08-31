# debug-blank-screen.ps1 - Diagnosticar pantalla en blanco
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTICANDO PANTALLA EN BLANCO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

if (-not (Test-Path "frontend/package.json")) {
    Write-Host "ERROR: Ejecuta desde la raiz del proyecto" -ForegroundColor Red
    exit 1
}

Set-Location "frontend"

Write-Host ""
Write-Host "PASO 1: Verificar build web..." -ForegroundColor Yellow

# Verificar que el build existe y es valido
if (Test-Path "dist/index.html") {
    $indexContent = Get-Content "dist/index.html" -Raw
    Write-Host "   index.html encontrado ($($indexContent.Length) caracteres)" -ForegroundColor Green
    
    # Verificar contenido del HTML
    if ($indexContent -match '<div id="root"></div>') {
        Write-Host "   Elemento root encontrado" -ForegroundColor Green
    } else {
        Write-Host "   ADVERTENCIA: No se encontro elemento root" -ForegroundColor Yellow
    }
    
    if ($indexContent -match '/assets/') {
        Write-Host "   Referencias a assets encontradas" -ForegroundColor Green
    } else {
        Write-Host "   ADVERTENCIA: No hay referencias a assets" -ForegroundColor Yellow
    }
    
    # Mostrar primeras lineas del HTML
    Write-Host "   Contenido del HTML:" -ForegroundColor Gray
    $lines = $indexContent -split "`n" | Select-Object -First 10
    foreach ($line in $lines) {
        Write-Host "     $line" -ForegroundColor DarkGray
    }
} else {
    Write-Host "   ERROR: dist/index.html no existe" -ForegroundColor Red
    Write-Host "   Regenerando build..." -ForegroundColor Gray
    npm run build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR en build" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
}

Write-Host ""
Write-Host "PASO 2: Verificar assets compilados..." -ForegroundColor Yellow

$assetsDir = "dist/assets"
if (Test-Path $assetsDir) {
    $jsFiles = Get-ChildItem $assetsDir -Filter "*.js"
    $cssFiles = Get-ChildItem $assetsDir -Filter "*.css"
    
    Write-Host "   Assets encontrados:" -ForegroundColor Green
    Write-Host "     Archivos JS: $($jsFiles.Count)" -ForegroundColor Cyan
    Write-Host "     Archivos CSS: $($cssFiles.Count)" -ForegroundColor Cyan
    
    if ($jsFiles.Count -eq 0) {
        Write-Host "   ERROR: No hay archivos JavaScript compilados" -ForegroundColor Red
    }
    
    # Verificar el archivo JS principal
    $mainJs = $jsFiles | Where-Object { $_.Name -match "main|index" } | Select-Object -First 1
    if ($mainJs) {
        $jsSize = [Math]::Round($mainJs.Length / 1KB, 1)
        Write-Host "     JS principal: $($mainJs.Name) ($jsSize KB)" -ForegroundColor Cyan
        
        if ($jsSize -lt 10) {
            Write-Host "   ADVERTENCIA: Archivo JS muy pequeno, posible build incompleto" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   ERROR: Carpeta assets no existe" -ForegroundColor Red
}

Write-Host ""
Write-Host "PASO 3: Verificar configuracion de Vite..." -ForegroundColor Yellow

$viteConfig = "vite.config.ts"
if (Test-Path $viteConfig) {
    $configContent = Get-Content $viteConfig -Raw
    
    if ($configContent -match "base:\s*['""]\.\/['""]") {
        Write-Host "   Base configurado correctamente (./)" -ForegroundColor Green
    } else {
        Write-Host "   PROBLEMA: Base no configurado correctamente" -ForegroundColor Red
        Write-Host "   Corrigiendo vite.config.ts..." -ForegroundColor Gray
        
        # Leer y corregir vite.config.ts
        $fixedConfig = $configContent -replace "base:\s*['""][^'""]*['""]", "base: './'";
        if (-not ($configContent -match "base:")) {
            $fixedConfig = $configContent -replace "(plugins:\s*\[[^\]]*\],)", "`$1`n  base: './',"
        }
        
        $fixedConfig | Set-Content $viteConfig -Encoding UTF8
        Write-Host "   vite.config.ts corregido" -ForegroundColor Green
        
        # Rebuild necesario
        Write-Host "   Rebuilding debido a cambio de configuracion..." -ForegroundColor Gray
        npm run build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR en rebuild" -ForegroundColor Red
            Set-Location ".."
            exit 1
        }
    }
} else {
    Write-Host "   ADVERTENCIA: vite.config.ts no encontrado" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "PASO 4: Crear version de electron-main.cjs con mas debugging..." -ForegroundColor Yellow

$debugElectronMain = @"
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
    mainWindow.webContents.executeJavaScript(`
      console.log('=== ESTADO DEL DOM ===');
      console.log('Elemento root:', document.getElementById('root'));
      console.log('Contenido root:', document.getElementById('root')?.innerHTML?.substring(0, 200));
      console.log('Scripts cargados:', document.scripts.length);
      console.log('Hojas de estilo:', document.styleSheets.length);
      console.log('=== FIN ESTADO ===');
    `);
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
"@

# Escribir electron-main.cjs con debug mejorado
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText((Join-Path (Get-Location) "electron-main.cjs"), $debugElectronMain, $utf8NoBom)
Write-Host "   electron-main.cjs con debug creado" -ForegroundColor Green

Write-Host ""
Write-Host "PASO 5: Test rapido con electron en desarrollo..." -ForegroundColor Yellow

Write-Host "   Probando en modo desarrollo..." -ForegroundColor Gray
Write-Host "   IMPORTANTE: Deja esto corriendo y abre otra terminal para el backend" -ForegroundColor Yellow

# Mostrar instrucciones
Write-Host ""
Write-Host "INSTRUCCIONES PARA DEBUGGING:" -ForegroundColor White
Write-Host "   1. Abre otra terminal PowerShell" -ForegroundColor Gray
Write-Host "   2. Ve al directorio: cd backend" -ForegroundColor Gray
Write-Host "   3. Ejecuta: dotnet run" -ForegroundColor Gray
Write-Host "   4. Vuelve aqui y presiona Enter para probar Electron" -ForegroundColor Gray
Write-Host ""
Write-Host "Presiona Enter cuando el backend este corriendo..." -ForegroundColor Yellow
Read-Host

Write-Host "   Iniciando Electron en modo desarrollo..." -ForegroundColor Gray

# Iniciar electron en modo desarrollo para ver que pasa
$env:NODE_ENV = "development"
Start-Process -FilePath "npx" -ArgumentList "electron", "." -NoNewWindow -Wait

Set-Location ".."

Write-Host ""
Write-Host "DEBUGGING COMPLETADO" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "PROXIMOS PASOS:" -ForegroundColor White
Write-Host "   1. Revisa los logs en la consola de DevTools" -ForegroundColor Gray
Write-Host "   2. Busca errores en rojo en la consola" -ForegroundColor Gray
Write-Host "   3. Verifica que React se este cargando correctamente" -ForegroundColor Gray
Write-Host "   4. Si funciona en desarrollo, rebuild el ejecutable:" -ForegroundColor Gray
Write-Host "      cd frontend && npm run electron:build" -ForegroundColor Gray
Write-Host ""
Write-Host "ERRORES COMUNES:" -ForegroundColor Yellow
Write-Host "   - Rutas de assets incorrectas (base: './' en vite.config)" -ForegroundColor Gray
Write-Host "   - JavaScript no se carga (verificar archivos en dist/assets/)" -ForegroundColor Gray
Write-Host "   - Errores de React en la consola (verificar en DevTools)" -ForegroundColor Gray
Write-Host "=============================================" -ForegroundColor Green