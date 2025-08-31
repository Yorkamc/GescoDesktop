# build-electron-debug.ps1 - Build con diagnóstico completo
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  GESCO DESKTOP - BUILD CON DIAGNOSTICO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

if (-not (Test-Path "frontend/package.json")) {
    Write-Host "ERROR: Ejecuta desde la raíz del proyecto" -ForegroundColor Red
    exit 1
}

Set-Location "frontend"

# PASO 0: Diagnóstico inicial
Write-Host ""
Write-Host "DIAGNOSTICO INICIAL:" -ForegroundColor Cyan
Write-Host "   Node version: $(node --version)" -ForegroundColor Gray
Write-Host "   NPM version: $(npm --version)" -ForegroundColor Gray
Write-Host "   Working directory: $PWD" -ForegroundColor Gray

# PASO 1: Limpieza EXTREMA
Write-Host ""
Write-Host "PASO 1: Limpieza extrema..." -ForegroundColor Yellow

$pathsToClean = @("dist", "dist-electron", ".vite", "node_modules\.vite", ".cache")
foreach ($path in $pathsToClean) {
    if (Test-Path $path) {
        Write-Host "   Eliminando: $path" -ForegroundColor Gray
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 1
        
        if (Test-Path $path) {
            Write-Host "   ADVERTENCIA: No se pudo eliminar completamente $path" -ForegroundColor Yellow
        } else {
            Write-Host "   OK: $path eliminado" -ForegroundColor Green
        }
    }
}

# Limpiar cache npm y Electron
Write-Host "   Limpiando caches..." -ForegroundColor Gray
npm cache clean --force | Out-Null
if (Get-Command "npx" -ErrorAction SilentlyContinue) {
    npx electron-builder clean | Out-Null
}

Write-Host "   Limpieza completada" -ForegroundColor Green

# PASO 2: Verificar dependencias
Write-Host ""
Write-Host "PASO 2: Verificar dependencias..." -ForegroundColor Yellow

if (-not (Test-Path "node_modules")) {
    Write-Host "   Instalando dependencias desde cero..." -ForegroundColor Gray
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR instalando dependencias" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
} else {
    Write-Host "   node_modules existe, verificando..." -ForegroundColor Gray
    
    # Verificar que existen dependencias clave
    $criticalDeps = @("react", "react-dom", "vite", "electron", "electron-builder")
    $missing = @()
    
    foreach ($dep in $criticalDeps) {
        if (-not (Test-Path "node_modules/$dep")) {
            $missing += $dep
        }
    }
    
    if ($missing.Count -gt 0) {
        Write-Host "   DEPENDENCIAS FALTANTES: $($missing -join ', ')" -ForegroundColor Red
        Write-Host "   Reinstalando..." -ForegroundColor Gray
        Remove-Item "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR reinstalando dependencias" -ForegroundColor Red
            Set-Location ".."
            exit 1
        }
    } else {
        Write-Host "   Dependencias OK" -ForegroundColor Green
    }
}

# PASO 3: Build web CON DEBUGGING
Write-Host ""
Write-Host "PASO 3: Build web con debugging..." -ForegroundColor Yellow

# Configurar variables de entorno
$env:NODE_ENV = "production"
$env:VITE_APP_VERSION = "1.0.0"

Write-Host "   Variables de entorno:" -ForegroundColor Gray
Write-Host "     NODE_ENV: $env:NODE_ENV" -ForegroundColor Gray
Write-Host "     PWD: $PWD" -ForegroundColor Gray

# Build con logs detallados
Write-Host "   Ejecutando: npm run build" -ForegroundColor Gray
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en build web" -ForegroundColor Red
    Write-Host "Revisa los mensajes de error arriba" -ForegroundColor Yellow
    Set-Location ".."
    exit 1
}

# PASO 4: Verificar build web
Write-Host ""
Write-Host "PASO 4: Verificar build web..." -ForegroundColor Yellow

$distPath = "dist"
if (-not (Test-Path $distPath)) {
    Write-Host "ERROR: No se generó la carpeta dist/" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

$indexPath = "$distPath/index.html"
if (-not (Test-Path $indexPath)) {
    Write-Host "ERROR: No se generó dist/index.html" -ForegroundColor Red
    Write-Host "Contenido de dist/:" -ForegroundColor Gray
    Get-ChildItem $distPath | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor Gray }
    Set-Location ".."
    exit 1
}

# Verificar contenido del index.html
$indexContent = Get-Content $indexPath -Raw
$indexSize = $indexContent.Length

Write-Host "   dist/index.html creado correctamente" -ForegroundColor Green
Write-Host "   Tamaño: $indexSize caracteres" -ForegroundColor Cyan
Write-Host "   Timestamp: $((Get-Item $indexPath).LastWriteTime)" -ForegroundColor Cyan

# Mostrar primeras líneas del HTML para verificar
$firstLines = ($indexContent -split "`n")[0..5] -join "`n"
Write-Host "   Primeras líneas:" -ForegroundColor Gray
Write-Host $firstLines -ForegroundColor DarkGray

# Listar todos los archivos generados
Write-Host "   Archivos generados en dist/:" -ForegroundColor Gray
Get-ChildItem $distPath -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Replace("$PWD\$distPath\", "")
    $size = [Math]::Round($_.Length / 1KB, 1)
    Write-Host "     $relativePath ($size KB)" -ForegroundColor DarkGray
}

# PASO 5: Build Electron
Write-Host ""
Write-Host "PASO 5: Build Electron..." -ForegroundColor Yellow

Write-Host "   Eliminando build anterior de Electron..." -ForegroundColor Gray
if (Test-Path "dist-electron") {
    Remove-Item "dist-electron" -Recurse -Force
}

Write-Host "   Ejecutando: npm run electron:build" -ForegroundColor Gray
npm run electron:build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en build Electron" -ForegroundColor Red
    Write-Host "Revisa los mensajes de error arriba" -ForegroundColor Yellow
    Set-Location ".."
    exit 1
}

# PASO 6: Verificar ejecutable
Write-Host ""
Write-Host "PASO 6: Verificar ejecutable..." -ForegroundColor Yellow

if (-not (Test-Path "dist-electron")) {
    Write-Host "ERROR: No se generó dist-electron/" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Write-Host "   Contenido de dist-electron/:" -ForegroundColor Gray
Get-ChildItem "dist-electron" -File | ForEach-Object {
    $size = [Math]::Round($_.Length / 1MB, 2)
    $timestamp = $_.LastWriteTime
    
    if ($_.Extension -eq ".exe") {
        Write-Host "   EJECUTABLE: $($_.Name)" -ForegroundColor Green
    } else {
        Write-Host "   ARCHIVO: $($_.Name)" -ForegroundColor Gray
    }
    Write-Host "     Tamaño: $size MB" -ForegroundColor Cyan
    Write-Host "     Creado: $timestamp" -ForegroundColor Cyan
    Write-Host ""
}

# PASO 7: Verificar integridad
Write-Host ""
Write-Host "PASO 7: Verificar integridad..." -ForegroundColor Yellow

$distTime = (Get-Item $indexPath).LastWriteTime
$exeFiles = Get-ChildItem "dist-electron" -Filter "*.exe"

if ($exeFiles.Count -eq 0) {
    Write-Host "ERROR: No se generaron archivos .exe" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

$exeTime = $exeFiles[0].LastWriteTime
$timeDiff = ($exeTime - $distTime).TotalMinutes

Write-Host "   Build web: $distTime" -ForegroundColor Cyan
Write-Host "   Ejecutable: $exeTime" -ForegroundColor Cyan
Write-Host "   Diferencia: $([Math]::Round($timeDiff, 1)) minutos" -ForegroundColor Cyan

if ($exeTime -ge $distTime.AddMinutes(-2)) {
    Write-Host "   INTEGRIDAD: OK - Ejecutable contiene build reciente" -ForegroundColor Green
} else {
    Write-Host "   ADVERTENCIA: Ejecutable podría estar desactualizado" -ForegroundColor Yellow
}

Set-Location ".."

# RESUMEN FINAL
Write-Host ""
Write-Host "BUILD COMPLETADO CON DIAGNOSTICO!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "ARCHIVOS GENERADOS:" -ForegroundColor White
Write-Host "   Web build: frontend/dist/index.html ($indexSize chars)" -ForegroundColor Cyan
Write-Host "   Ejecutable: frontend/dist-electron/*.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO:" -ForegroundColor Yellow
Write-Host "   1. Ejecuta el .exe desde: frontend/dist-electron/" -ForegroundColor White
Write-Host "   2. Si no funciona, envía los logs de esta ventana" -ForegroundColor White
Write-Host "   3. El ejecutable mostrará DevTools para más debugging" -ForegroundColor White
Write-Host ""
Write-Host "DEBUGGING:" -ForegroundColor Yellow
Write-Host "   - El .exe abrirá DevTools automáticamente" -ForegroundColor White
Write-Host "   - Revisa la consola para errores específicos" -ForegroundColor White
Write-Host "   - Si no carga, mostrará una página de diagnóstico" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor Green