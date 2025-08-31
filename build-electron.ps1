# build-electron.ps1 - Build Electron actualizado (SIN EMOJIS)
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  GESCO DESKTOP - BUILD ELECTRON" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Verificar ubicacion
if (-not (Test-Path "frontend/package.json")) {
    Write-Host "ERROR: Ejecuta desde la raiz del proyecto" -ForegroundColor Red
    exit 1
}

Set-Location "frontend"

Write-Host ""
Write-Host "PASO 1: Limpieza completa..." -ForegroundColor Yellow

# Eliminar TODOS los archivos de build anteriores
$pathsToClean = @("dist", "dist-electron", ".vite", "node_modules\.vite")
foreach ($path in $pathsToClean) {
    if (Test-Path $path) {
        Write-Host "   Eliminando: $path" -ForegroundColor Gray
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Limpiar npm cache
Write-Host "   Limpiando cache npm..." -ForegroundColor Gray
npm cache clean --force | Out-Null

Write-Host "   Limpieza completa OK" -ForegroundColor Green

Write-Host ""
Write-Host "PASO 2: Verificar dependencias..." -ForegroundColor Yellow

# Verificar que node_modules este actualizado
if (-not (Test-Path "node_modules")) {
    Write-Host "   Instalando dependencias..." -ForegroundColor Gray
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR instalando dependencias" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
} else {
    Write-Host "   Dependencias OK" -ForegroundColor Green
}

Write-Host ""
Write-Host "PASO 3: Build web sin cache..." -ForegroundColor Yellow

# Configurar variables para build fresco
$env:NODE_ENV = "production"

# Build web sin cache
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en build web" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Verificar que el build sea reciente
$indexPath = "dist/index.html"
if (Test-Path $indexPath) {
    $buildTime = (Get-Item $indexPath).LastWriteTime
    $minutesAgo = [Math]::Round(((Get-Date) - $buildTime).TotalMinutes, 1)
    Write-Host "   Build completado hace $minutesAgo minutos" -ForegroundColor Green
    
    $fileCount = Get-ChildItem 'dist' -Recurse -File | Measure-Object | Select-Object -ExpandProperty Count
    Write-Host "   Archivos generados: $fileCount" -ForegroundColor Cyan
} else {
    Write-Host "ERROR: No se genero index.html" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Write-Host ""
Write-Host "PASO 4: Build Electron..." -ForegroundColor Yellow

# Eliminar build anterior de Electron
if (Test-Path "dist-electron") {
    Remove-Item "dist-electron" -Recurse -Force
    Write-Host "   Build anterior eliminado" -ForegroundColor Gray
}

# Build Electron
npm run electron:build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en build Electron" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Write-Host ""
Write-Host "PASO 5: Verificacion..." -ForegroundColor Yellow

if (Test-Path "dist-electron") {
    $exeFiles = Get-ChildItem "dist-electron" -Filter "*.exe" -File
    
    if ($exeFiles.Count -gt 0) {
        foreach ($exe in $exeFiles) {
            $exeTime = $exe.LastWriteTime
            $exeMinutesAgo = [Math]::Round(((Get-Date) - $exeTime).TotalMinutes, 1)
            $exeSize = [Math]::Round($exe.Length / 1MB, 2)
            
            Write-Host "   Ejecutable: $($exe.Name)" -ForegroundColor Green
            Write-Host "   Tamano: $exeSize MB" -ForegroundColor Cyan
            Write-Host "   Creado: hace $exeMinutesAgo minutos" -ForegroundColor Cyan
        }
        
        # Verificar integridad comparando timestamps
        $distTime = (Get-Item "dist/index.html").LastWriteTime
        $exeTime = $exeFiles[0].LastWriteTime
        
        if ($exeTime -ge $distTime) {
            Write-Host "   VERSION MAS RECIENTE confirmada" -ForegroundColor Green
        } else {
            Write-Host "   ADVERTENCIA: Posible desincronizacion" -ForegroundColor Yellow
        }
    } else {
        Write-Host "ERROR: No se generaron archivos .exe" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
} else {
    Write-Host "ERROR: No se genero carpeta dist-electron" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Set-Location ".."

Write-Host ""
Write-Host "EJECUTABLE GENERADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Ubicacion: frontend/dist-electron/" -ForegroundColor Cyan
Write-Host ""
Write-Host "PROBLEMA SOLUCIONADO:" -ForegroundColor Yellow
Write-Host "   - Cache completamente limpiado" -ForegroundColor White
Write-Host "   - Build web forzado sin cache" -ForegroundColor White
Write-Host "   - Ejecutable contiene version mas reciente" -ForegroundColor White
Write-Host ""
Write-Host "Para futuras actualizaciones, usa este script" -ForegroundColor Gray
Write-Host "en lugar de 'npm run electron:build'" -ForegroundColor Gray
Write-Host "=============================================" -ForegroundColor Green