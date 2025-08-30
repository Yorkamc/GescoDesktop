# ====================================================
# GESCO DESKTOP - GENERAR SOLO EJECUTABLE ELECTRON
# ====================================================

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  GENERANDO EJECUTABLE GESCO DESKTOP" -ForegroundColor Cyan  
Write-Host "=============================================" -ForegroundColor Cyan

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "frontend/package.json")) {
    Write-Host "Error: No se encuentra frontend/package.json" -ForegroundColor Red
    Write-Host "Ejecuta este script desde la raiz del proyecto" -ForegroundColor Yellow
    exit 1
}

# Ir al directorio frontend
Set-Location "frontend"

Write-Host "`nVerificando dependencias..." -ForegroundColor Yellow

# Verificar Node.js
try {
    $nodeVersion = node --version
    $npmVersion = npm --version
    Write-Host "   Node.js: $nodeVersion" -ForegroundColor Green
    Write-Host "   npm: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "Error: Node.js no encontrado" -ForegroundColor Red
    Write-Host "Instala Node.js desde: https://nodejs.org/" -ForegroundColor Yellow
    Set-Location ".."
    exit 1
}

# Verificar/Instalar dependencias npm
if (-not (Test-Path "node_modules")) {
    Write-Host "`nInstalando dependencias npm..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error instalando dependencias" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
    Write-Host "   Dependencias instaladas" -ForegroundColor Green
}

# Limpiar build anterior de Electron
if (Test-Path "dist-electron") {
    Write-Host "`nLimpiando build anterior..." -ForegroundColor Yellow
    Remove-Item -Path "dist-electron" -Recurse -Force
    Write-Host "   Build anterior limpiado" -ForegroundColor Green
}

# Compilar aplicación web primero
Write-Host "`nCompilando aplicacion web..." -ForegroundColor Yellow
$env:NODE_ENV = "production"
npm run build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error compilando aplicacion web" -ForegroundColor Red
    Set-Location ".."
    exit 1
}
Write-Host "   Aplicacion web compilada" -ForegroundColor Green

# Generar ejecutable Electron
Write-Host "`nGenerando ejecutable Electron..." -ForegroundColor Green
npm run electron:build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generando ejecutable Electron" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Volver al directorio raiz
Set-Location ".."

# Mostrar resultados
Write-Host "`nEJECUTABLE GENERADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

if (Test-Path "frontend/dist-electron") {
    Write-Host ""
    Write-Host "Archivos generados en: frontend/dist-electron/" -ForegroundColor Cyan
    Write-Host ""
    
    $files = Get-ChildItem -Path "frontend/dist-electron" -File
    foreach ($file in $files) {
        $size = [Math]::Round($file.Length / 1MB, 2)
        if ($file.Extension -eq ".exe") {
            Write-Host "   $($file.Name) ($size MB) - EJECUTABLE WINDOWS" -ForegroundColor Green
        }
        elseif ($file.Extension -eq ".AppImage") {
            Write-Host "   $($file.Name) ($size MB) - EJECUTABLE LINUX" -ForegroundColor Green
        }
        elseif ($file.Extension -eq ".dmg") {
            Write-Host "   $($file.Name) ($size MB) - INSTALADOR MACOS" -ForegroundColor Green
        }
        else {
            Write-Host "   $($file.Name) ($size MB)" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "Para usar:" -ForegroundColor Yellow
    Write-Host "   1. Localiza el archivo .exe en la carpeta frontend/dist-electron/" -ForegroundColor White
    Write-Host "   2. Ejecuta GESCO-Desktop-Setup.exe para instalar" -ForegroundColor White
    Write-Host "   3. O ejecuta directamente el .exe portable" -ForegroundColor White
    
} else {
    Write-Host "Error: No se generaron archivos en dist-electron" -ForegroundColor Red
}

Write-Host "=============================================" -ForegroundColor Green