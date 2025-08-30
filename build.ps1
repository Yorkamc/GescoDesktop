# ====================================================
#  📦 GESCO DESKTOP - BUILD SCRIPT PARA PRODUCCIÓN
# ====================================================
param(
    [string]$Configuration = "Release",
    [switch]$SkipTests = $false,
    [switch]$BuildElectron = $false,
    [switch]$Clean = $false
)

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  📦 BUILDING GESCO DESKTOP - PRODUCCIÓN" -ForegroundColor Cyan  
Write-Host "=============================================" -ForegroundColor Cyan

# Variables
$BackendProject = "backend/src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj"
$FrontendPath = "frontend"
$OutputPath = "dist"
$BackendOutput = "$OutputPath/backend"
$FrontendOutput = "$OutputPath/frontend"
$ElectronOutput = "$FrontendPath/dist-electron"

# Limpiar builds anteriores si se solicita
if ($Clean) {
    Write-Host "`n🧹 Limpiando builds anteriores..." -ForegroundColor Yellow
    
    if (Test-Path $OutputPath) {
        Remove-Item -Path $OutputPath -Recurse -Force
        Write-Host "   ✅ Directorio dist/ limpiado" -ForegroundColor Green
    }
    
    if (Test-Path "$FrontendPath/dist") {
        Remove-Item -Path "$FrontendPath/dist" -Recurse -Force
        Write-Host "   ✅ Frontend dist/ limpiado" -ForegroundColor Green
    }
    
    if (Test-Path $ElectronOutput) {
        Remove-Item -Path $ElectronOutput -Recurse -Force
        Write-Host "   ✅ Electron build limpiado" -ForegroundColor Green
    }
    
    # Limpiar backend
    Set-Location "backend"
    dotnet clean --verbosity quiet | Out-Null
    Set-Location ".."
    Write-Host "   ✅ Backend limpiado" -ForegroundColor Green
}

# Crear directorios de salida
Write-Host "`n📁 Preparando directorios de salida..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
New-Item -ItemType Directory -Path $BackendOutput -Force | Out-Null
New-Item -ItemType Directory -Path $FrontendOutput -Force | Out-Null

# Verificar dependencias
Write-Host "`n🔍 Verificando dependencias..." -ForegroundColor Yellow

try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: .NET SDK no encontrado" -ForegroundColor Red
    exit 1
}

try {
    Set-Location $FrontendPath
    $nodeVersion = node --version
    $npmVersion = npm --version
    Write-Host "   ✅ Node.js: $nodeVersion" -ForegroundColor Green
    Write-Host "   ✅ npm: $npmVersion" -ForegroundColor Green
    Set-Location ".."
} catch {
    Write-Host "❌ Error: Node.js no encontrado" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# ====================================================
# BUILD BACKEND
# ====================================================
Write-Host "`n⚙️ Building Backend (.NET)..." -ForegroundColor Green

# Restaurar paquetes
Write-Host "   📥 Restaurando paquetes NuGet..." -ForegroundColor Yellow
Set-Location "backend"
dotnet restore --verbosity quiet | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error restaurando paquetes NuGet" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Ejecutar tests (opcional)
if (-not $SkipTests) {
    Write-Host "   🧪 Ejecutando tests..." -ForegroundColor Yellow
    dotnet test --verbosity quiet --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Advertencia: Algunos tests fallaron" -ForegroundColor Yellow
        $choice = Read-Host "   ¿Continuar con el build? (y/n)"
        if ($choice -ne 'y' -and $choice -ne 'Y') {
            Set-Location ".."
            exit 1
        }
    } else {
        Write-Host "   ✅ Todos los tests pasaron" -ForegroundColor Green
    }
}

# Build y publish
Write-Host "   🔨 Compilando para producción..." -ForegroundColor Yellow
dotnet publish $BackendProject -c $Configuration -o "../$BackendOutput" --verbosity quiet --no-restore | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error compilando backend" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Set-Location ".."
Write-Host "   ✅ Backend compilado exitosamente" -ForegroundColor Green

# ====================================================
# BUILD FRONTEND
# ====================================================
Write-Host "`n🎯 Building Frontend (React + Vite)..." -ForegroundColor Green

Set-Location $FrontendPath

# Instalar dependencias si es necesario
if (-not (Test-Path "node_modules")) {
    Write-Host "   📥 Instalando dependencias npm..." -ForegroundColor Yellow
    npm install | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Error instalando dependencias npm" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
}

# Build web
Write-Host "   🔨 Compilando aplicación web..." -ForegroundColor Yellow
$env:NODE_ENV = "production"
npm run build | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error compilando frontend" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Copiar al directorio de salida
if (Test-Path "dist") {
    Copy-Item -Path "dist/*" -Destination "../$FrontendOutput" -Recurse -Force
    Write-Host "   ✅ Frontend web compilado exitosamente" -ForegroundColor Green
} else {
    Write-Host "❌ Error: No se encontró el directorio dist del frontend" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Build Electron (opcional)
if ($BuildElectron) {
    Write-Host "`n🖥️ Building aplicación Electron..." -ForegroundColor Green
    
    Write-Host "   📦 Compilando aplicación de escritorio..." -ForegroundColor Yellow
    npm run electron:build | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Advertencia: Error compilando Electron app" -ForegroundColor Yellow
    } else {
        Write-Host "   ✅ Aplicación Electron compilada" -ForegroundColor Green
        Write-Host "   📁 Ubicación: $FrontendPath/dist-electron/" -ForegroundColor Cyan
    }
}

Set-Location ".."

# ====================================================
# CREAR DOCUMENTACIÓN DE BUILD
# ====================================================
Write-Host "`n📄 Generando documentación de build..." -ForegroundColor Yellow

$buildInfo = @{
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    configuration = $Configuration
    dotnetVersion = $dotnetVersion
    nodeVersion = $nodeVersion.TrimStart('v')
    npmVersion = $npmVersion
    electronBuild = $BuildElectron
    testsSkipped = $SkipTests
}

$buildInfoJson = $buildInfo | ConvertTo-Json -Depth 3
$buildInfoJson | Out-File -FilePath "$OutputPath/build-info.json" -Encoding UTF8

# Crear README de distribución
$desktopSection = ""
$desktopInstructions = "No compilada en este build"

if ($BuildElectron) {
    $desktopSection = "### Aplicación de Escritorio`n- 📁 ``$FrontendPath/dist-electron/`` - Aplicaciones Electron compiladas"
    $desktopInstructions = "1. Distribuir ejecutables desde ``$FrontendPath/dist-electron/``"
}

$readmeContent = @"
# GESCO Desktop - Build de Producción

**Fecha de build:** $((Get-Date).ToString('dd/MM/yyyy HH:mm:ss'))
**Configuración:** $Configuration
**Versión .NET:** $dotnetVersion  
**Versión Node.js:** $nodeVersion

## Archivos incluidos

### Backend (.NET)
- 📁 ``backend/`` - API REST compilada
- 🔧 ``backend/Gesco.Desktop.UI.exe`` - Ejecutable principal
- 📋 ``backend/appsettings.json`` - Configuración

### Frontend (React)
- 📁 ``frontend/`` - Aplicación web compilada
- 📄 ``frontend/index.html`` - Página principal
- 📦 ``frontend/assets/`` - Recursos estáticos

$desktopSection

## Instrucciones de despliegue

### Servidor Web
1. Desplegar contenido de ``frontend/`` en servidor web
2. Configurar proxy hacia API backend
3. Configurar variables de entorno de producción

### API Backend  
1. Copiar contenido de ``backend/`` al servidor
2. Configurar cadenas de conexión
3. Ejecutar: ``dotnet Gesco.Desktop.UI.dll``

### Aplicación de Escritorio
$desktopInstructions

---
*Generado automáticamente por build.ps1*
"@

$readmeContent | Out-File -FilePath "$OutputPath/README.md" -Encoding UTF8

# ====================================================
# RESUMEN FINAL
# ====================================================
Write-Host "`n🎉 BUILD COMPLETADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Resumen del build:" -ForegroundColor White
Write-Host "   📁 Output: $OutputPath/" -ForegroundColor Cyan
Write-Host "   ⚙️ Backend: $BackendOutput/" -ForegroundColor Cyan
Write-Host "   🎯 Frontend: $FrontendOutput/" -ForegroundColor Cyan
if ($BuildElectron) {
    Write-Host "   🖥️ Electron: $FrontendPath/dist-electron/" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "📋 Archivos generados:" -ForegroundColor White
Write-Host "   📄 $OutputPath/build-info.json" -ForegroundColor Gray
Write-Host "   📄 $OutputPath/README.md" -ForegroundColor Gray
Write-Host ""

# Mostrar tamaños de archivos
if (Test-Path $BackendOutput) {
    $backendSize = (Get-ChildItem -Path $BackendOutput -Recurse | Measure-Object -Property Length -Sum).Sum
    Write-Host "💾 Tamaños:" -ForegroundColor White
    Write-Host "   ⚙️ Backend: $([Math]::Round($backendSize / 1MB, 2)) MB" -ForegroundColor Gray
}

if (Test-Path $FrontendOutput) {
    $frontendSize = (Get-ChildItem -Path $FrontendOutput -Recurse | Measure-Object -Property Length -Sum).Sum
    Write-Host "   🎯 Frontend: $([Math]::Round($frontendSize / 1MB, 2)) MB" -ForegroundColor Gray
}

if ($BuildElectron -and (Test-Path $ElectronOutput)) {
    $electronSize = (Get-ChildItem -Path $ElectronOutput -Recurse | Measure-Object -Property Length -Sum).Sum
    Write-Host "   🖥️ Electron: $([Math]::Round($electronSize / 1MB, 2)) MB" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🚀 Para desplegar:" -ForegroundColor Yellow
Write-Host "   1. Subir contenido de '$OutputPath/' al servidor" -ForegroundColor White
Write-Host "   2. Configurar variables de entorno de producción" -ForegroundColor White
Write-Host "   3. Ejecutar: dotnet Gesco.Desktop.UI.dll" -ForegroundColor White

if ($BuildElectron) {
    Write-Host ""
    Write-Host "🖥️ Para distribuir aplicación de escritorio:" -ForegroundColor Yellow
    Write-Host "   📁 Ejecutables en: $FrontendPath/dist-electron/" -ForegroundColor White
    Write-Host "   🪟 Windows: GESCO-Desktop-Setup.exe" -ForegroundColor White
    Write-Host "   🐧 Linux: GESCO-Desktop.AppImage" -ForegroundColor White
    Write-Host "   🍎 macOS: GESCO-Desktop.dmg" -ForegroundColor White
}

Write-Host "=============================================" -ForegroundColor Green