# ====================================================
#  🚀 GESCO DESKTOP - STARTUP SCRIPT COMPLETO
# ====================================================
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  🚀 INICIANDO GESCO DESKTOP - FULL STACK" -ForegroundColor Cyan  
Write-Host "=============================================" -ForegroundColor Cyan

# Configurar variables de entorno
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:NODE_ENV = "development"

# Función para verificar si un puerto está en uso
function Test-Port {
    param([int]$Port)
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("127.0.0.1", $Port)
        $tcpClient.Close()
        return $true
    } catch {
        return $false
    }
}

# Función para esperar que un servicio esté listo
function Wait-ForService {
    param(
        [string]$Url,
        [int]$TimeoutSeconds = 60
    )
    
    $elapsed = 0
    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                return $true
            }
        } catch {
            # Continuar intentando
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "   ⏳ Esperando... ($elapsed/$TimeoutSeconds segundos)" -ForegroundColor Gray
    } while ($elapsed -lt $TimeoutSeconds)
    
    return $false
}

# Verificar estructura del proyecto
Write-Host "`n📁 Verificando estructura del proyecto..." -ForegroundColor Yellow

if (-not (Test-Path "backend/src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj")) {
    Write-Host "❌ Error: No se encuentra el proyecto backend" -ForegroundColor Red
    Write-Host "   Asegúrate de ejecutar desde la raíz del proyecto" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path "frontend/package.json")) {
    Write-Host "❌ Error: No se encuentra el proyecto frontend" -ForegroundColor Red
    Write-Host "   Asegúrate de que existe la carpeta frontend/" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Estructura del proyecto verificada" -ForegroundColor Green

# Verificar puertos disponibles
Write-Host "`n🔍 Verificando puertos..." -ForegroundColor Yellow

$backendPort = 5100
$frontendPort = 5173

if (Test-Port -Port $backendPort) {
    Write-Host "⚠️ Puerto $backendPort ya está en uso" -ForegroundColor Yellow
    $choice = Read-Host "   ¿Deseas continuar de todas formas? (y/n)"
    if ($choice -ne 'y' -and $choice -ne 'Y') {
        Write-Host "👋 Operación cancelada" -ForegroundColor Yellow
        exit 0
    }
}

if (Test-Port -Port $frontendPort) {
    Write-Host "⚠️ Puerto $frontendPort ya está en uso" -ForegroundColor Yellow
}

# Verificar dependencias del backend
Write-Host "`n📦 Verificando dependencias del backend..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: .NET SDK no encontrado" -ForegroundColor Red
    Write-Host "   Instala .NET 8 SDK desde: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Restaurar paquetes NuGet
Write-Host "`n📥 Restaurando paquetes NuGet..." -ForegroundColor Yellow
Set-Location "backend"
$restoreResult = dotnet restore --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error restaurando paquetes NuGet" -ForegroundColor Red
    Set-Location ".."
    exit 1
}
Set-Location ".."
Write-Host "✅ Paquetes NuGet restaurados" -ForegroundColor Green

# Verificar dependencias del frontend
Write-Host "`n🎯 Verificando dependencias del frontend..." -ForegroundColor Yellow
try {
    Set-Location "frontend"
    $nodeVersion = node --version
    $npmVersion = npm --version
    Write-Host "   ✅ Node.js: $nodeVersion" -ForegroundColor Green
    Write-Host "   ✅ npm: $npmVersion" -ForegroundColor Green
    
    # Verificar si node_modules existe
    if (-not (Test-Path "node_modules")) {
        Write-Host "   📥 Instalando dependencias de npm..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Error instalando dependencias npm" -ForegroundColor Red
            Set-Location ".."
            exit 1
        }
    }
    Set-Location ".."
    Write-Host "✅ Dependencias del frontend listas" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: Node.js no encontrado" -ForegroundColor Red
    Write-Host "   Instala Node.js desde: https://nodejs.org/" -ForegroundColor Yellow
    Set-Location ".."
    exit 1
}

# Compilar backend
Write-Host "`n🔨 Compilando backend..." -ForegroundColor Yellow
Set-Location "backend"
$buildResult = dotnet build --configuration Debug --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error compilando el backend" -ForegroundColor Red
    Set-Location ".."
    exit 1
}
Set-Location ".."
Write-Host "✅ Backend compilado exitosamente" -ForegroundColor Green

# Preparar base de datos
Write-Host "`n🗄️ Preparando base de datos..." -ForegroundColor Yellow
$dataDir = "backend/src/Gesco.Desktop.UI/data"
if (-not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Write-Host "   📁 Directorio de datos creado" -ForegroundColor Green
}

# ====================================================
# INICIAR SERVICIOS
# ====================================================

Write-Host "`n🚀 Iniciando servicios..." -ForegroundColor Green

# 1. Iniciar Backend
Write-Host "`n⚙️ Iniciando Backend (.NET API)..." -ForegroundColor Yellow
Write-Host "   🌐 URL: http://localhost:$backendPort" -ForegroundColor Cyan
Write-Host "   📚 Swagger: http://localhost:$backendPort/swagger" -ForegroundColor Cyan
Write-Host "   🔍 Health: http://localhost:$backendPort/api/system/health" -ForegroundColor Cyan

$backendJob = Start-Job -ScriptBlock {
    param($ProjectPath)
    Set-Location $ProjectPath
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://localhost:5100"
    dotnet run --project "src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj"
} -ArgumentList (Get-Location).Path

Write-Host "   ⏳ Esperando que el backend inicie..." -ForegroundColor Gray

# Esperar que el backend esté listo
$backendReady = Wait-ForService -Url "http://localhost:$backendPort/api/system/health" -TimeoutSeconds 60

if (-not $backendReady) {
    Write-Host "❌ Error: Backend no pudo iniciarse en 60 segundos" -ForegroundColor Red
    Stop-Job $backendJob -Force
    Remove-Job $backendJob -Force
    exit 1
}

Write-Host "   ✅ Backend iniciado correctamente!" -ForegroundColor Green

# 2. Iniciar Frontend
Write-Host "`n🎯 Iniciando Frontend (React + Vite)..." -ForegroundColor Yellow
Write-Host "   🌐 URL: http://localhost:$frontendPort" -ForegroundColor Cyan

$frontendJob = Start-Job -ScriptBlock {
    param($FrontendPath)
    Set-Location $FrontendPath
    $env:NODE_ENV = "development"
    npm run dev
} -ArgumentList "$((Get-Location).Path)/frontend"

Write-Host "   ⏳ Esperando que el frontend compile..." -ForegroundColor Gray

# Esperar que el frontend esté listo (Vite tarda más en compilar)
Start-Sleep -Seconds 10

$frontendReady = Wait-ForService -Url "http://localhost:$frontendPort" -TimeoutSeconds 45

if (-not $frontendReady) {
    Write-Host "⚠️ Advertencia: Frontend tardó más de lo esperado" -ForegroundColor Yellow
    Write-Host "   Revisa manualmente: http://localhost:$frontendPort" -ForegroundColor Gray
} else {
    Write-Host "   ✅ Frontend iniciado correctamente!" -ForegroundColor Green
}

# ====================================================
# INFORMACIÓN FINAL
# ====================================================

Write-Host "`n🎉 GESCO DESKTOP INICIADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 URLs de la aplicación:" -ForegroundColor White
Write-Host "   🎯 Aplicación Web:    http://localhost:$frontendPort" -ForegroundColor Cyan
Write-Host "   ⚙️ API Backend:       http://localhost:$backendPort" -ForegroundColor Cyan  
Write-Host "   📚 Documentación:     http://localhost:$backendPort/swagger" -ForegroundColor Cyan
Write-Host "   🏥 Health Check:      http://localhost:$backendPort/api/system/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔑 Credenciales por defecto:" -ForegroundColor White
Write-Host "   👤 Usuario: admin" -ForegroundColor Yellow
Write-Host "   🔐 Contraseña: admin123" -ForegroundColor Yellow
Write-Host ""
Write-Host "💡 Comandos útiles:" -ForegroundColor White
Write-Host "   🚀 Electron: cd frontend && npm run dev:electron" -ForegroundColor Gray
Write-Host "   📦 Build: npm run build" -ForegroundColor Gray
Write-Host "   🧪 Tests: cd backend && dotnet test" -ForegroundColor Gray
Write-Host ""
Write-Host "⚠️ Para detener los servicios:" -ForegroundColor Yellow
Write-Host "   Presiona Ctrl+C o cierra esta ventana" -ForegroundColor Gray
Write-Host "=============================================" -ForegroundColor Green

# Mostrar logs de los trabajos
Write-Host "`n📊 Monitoreando servicios (Ctrl+C para detener)..." -ForegroundColor Cyan

try {
    # Monitorear hasta que el usuario presione Ctrl+C
    while ($true) {
        Start-Sleep -Seconds 5
        
        # Verificar estado de los trabajos
        $backendState = Get-Job $backendJob | Select-Object -ExpandProperty State
        $frontendState = Get-Job $frontendJob | Select-Object -ExpandProperty State
        
        Write-Host "$(Get-Date -Format 'HH:mm:ss') - Backend: $backendState | Frontend: $frontendState" -ForegroundColor Gray
        
        # Si algún trabajo falla, reportarlo
        if ($backendState -eq "Failed") {
            Write-Host "❌ Backend falló - revisando logs..." -ForegroundColor Red
            Receive-Job $backendJob
        }
        
        if ($frontendState -eq "Failed") {
            Write-Host "❌ Frontend falló - revisando logs..." -ForegroundColor Red  
            Receive-Job $frontendJob
        }
    }
} catch [System.Management.Automation.PipelineStoppedException] {
    # Capturar Ctrl+C
    Write-Host "`n`n⏹️ Deteniendo servicios..." -ForegroundColor Yellow
} finally {
    # Limpiar trabajos
    Write-Host "🧹 Limpiando procesos..." -ForegroundColor Gray
    
    Stop-Job $backendJob -Force -ErrorAction SilentlyContinue
    Remove-Job $backendJob -Force -ErrorAction SilentlyContinue
    
    Stop-Job $frontendJob -Force -ErrorAction SilentlyContinue  
    Remove-Job $frontendJob -Force -ErrorAction SilentlyContinue
    
    Write-Host "✅ Servicios detenidos correctamente" -ForegroundColor Green
    Write-Host "👋 ¡Hasta luego!" -ForegroundColor Cyan
}