# dev-electron-v2.ps1 - Version corregida del script de desarrollo
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  GESCO DESKTOP - DESARROLLO ELECTRON V2" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Variables
$backendPort = 5100
$frontendPort = 5173
$maxWaitTime = 60

# Funcion para verificar puerto
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

# Funcion para esperar servicio con mejor manejo de errores
function Wait-ForService {
    param(
        [string]$Url,
        [string]$Name,
        [int]$TimeoutSeconds = 60
    )
    
    Write-Host "   Esperando $Name..." -ForegroundColor Gray
    $elapsed = 0
    
    while ($elapsed -lt $TimeoutSeconds) {
        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec 3 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "   ✓ $Name disponible!" -ForegroundColor Green
                return $true
            }
        } catch {
            # Continuar esperando
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
        
        if ($elapsed % 10 -eq 0) {
            Write-Host "   ⏳ $Name... ($elapsed/$TimeoutSeconds segundos)" -ForegroundColor Yellow
        }
    }
    
    Write-Host "   ✗ $Name no respondio en $TimeoutSeconds segundos" -ForegroundColor Red
    return $false
}

# Verificaciones iniciales
Write-Host ""
Write-Host "Verificando proyecto..." -ForegroundColor Yellow

$requiredPaths = @(
    "backend/src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj",
    "frontend/package.json"
)

foreach ($path in $requiredPaths) {
    if (-not (Test-Path $path)) {
        Write-Host "✗ Error: No se encuentra $path" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✓ Estructura del proyecto OK" -ForegroundColor Green

# Verificar dependencias
Write-Host ""
Write-Host "Verificando dependencias..." -ForegroundColor Yellow

# .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "   ✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ .NET SDK no encontrado" -ForegroundColor Red
    exit 1
}

# Node.js
try {
    Set-Location "frontend"
    $nodeVersion = node --version
    $npmVersion = npm --version
    Write-Host "   ✓ Node.js: $nodeVersion" -ForegroundColor Green
    Write-Host "   ✓ npm: $npmVersion" -ForegroundColor Green
    
    # Instalar dependencias si es necesario
    if (-not (Test-Path "node_modules")) {
        Write-Host "   📦 Instalando dependencias npm..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "   ✗ Error instalando dependencias npm" -ForegroundColor Red
            Set-Location ".."
            exit 1
        }
    }
    
    Set-Location ".."
} catch {
    Write-Host "   ✗ Node.js no encontrado" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# INICIAR SERVICIOS
Write-Host ""
Write-Host "Iniciando servicios..." -ForegroundColor Green

# Variables para los trabajos
$backendJob = $null
$frontendJob = $null

try {
    # 1. Backend
    Write-Host ""
    Write-Host "1. Iniciando Backend..." -ForegroundColor Yellow
    
    if (-not (Test-Port -Port $backendPort)) {
        # Crear trabajo para el backend
        $backendJob = Start-Job -ScriptBlock {
            param($ProjectPath, $Port)
            Set-Location $ProjectPath
            $env:ASPNETCORE_ENVIRONMENT = "Development"
            $env:ASPNETCORE_URLS = "http://localhost:$Port"
            dotnet run --project "src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj"
        } -ArgumentList (Get-Location).Path, $backendPort
        
        # Esperar que inicie
        $backendReady = Wait-ForService -Url "http://localhost:$backendPort/api/system/health" -Name "Backend" -TimeoutSeconds $maxWaitTime
        
        if (-not $backendReady) {
            throw "Backend no pudo iniciarse en $maxWaitTime segundos"
        }
    } else {
        Write-Host "   ✓ Backend ya esta ejecutandose" -ForegroundColor Green
    }
    
    # 2. Frontend
    Write-Host ""
    Write-Host "2. Iniciando Frontend..." -ForegroundColor Yellow
    
    if (-not (Test-Port -Port $frontendPort)) {
        # Crear trabajo para el frontend
        $frontendJob = Start-Job -ScriptBlock {
            param($FrontendPath)
            Set-Location $FrontendPath
            $env:NODE_ENV = "development"
            npm run dev
        } -ArgumentList "$((Get-Location).Path)/frontend"
        
        # Esperar que compile
        Write-Host "   ⏳ Esperando que Vite compile..." -ForegroundColor Gray
        Start-Sleep -Seconds 10
        
        $frontendReady = Wait-ForService -Url "http://localhost:$frontendPort" -Name "Frontend" -TimeoutSeconds 30
        
        if (-not $frontendReady) {
            Write-Host "   ⚠ Frontend tardo en iniciar, pero continuando..." -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ✓ Frontend ya esta ejecutandose" -ForegroundColor Green
    }
    
    # 3. Verificar ambos servicios
    Write-Host ""
    Write-Host "3. Verificando servicios..." -ForegroundColor Yellow
    
    $backendOK = $false
    $frontendOK = $false
    
    # Test Backend
    try {
        $healthCheck = Invoke-RestMethod -Uri "http://localhost:$backendPort/api/system/health" -TimeoutSec 5
        $backendOK = ($healthCheck.status -eq "healthy")
        Write-Host "   ✓ Backend API: $($healthCheck.status)" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Backend API: No responde" -ForegroundColor Red
    }
    
    # Test Frontend
    try {
        $frontendResponse = Invoke-WebRequest -Uri "http://localhost:$frontendPort" -TimeoutSec 5 -ErrorAction Stop
        $frontendOK = ($frontendResponse.StatusCode -eq 200)
        Write-Host "   ✓ Frontend: Disponible (Status: $($frontendResponse.StatusCode))" -ForegroundColor Green
    } catch {
        Write-Host "   ✗ Frontend: No responde" -ForegroundColor Red
    }
    
    # 4. Iniciar Electron si todo esta OK
    if ($backendOK -and $frontendOK) {
        Write-Host ""
        Write-Host "4. Iniciando Electron..." -ForegroundColor Green
        Write-Host "   🚀 Se abrira la aplicacion de escritorio" -ForegroundColor Cyan
        Write-Host ""
        
        Set-Location "frontend"
        
        # Configurar entorno
        $env:NODE_ENV = "development"
        $env:ELECTRON_ENABLE_LOGGING = "1"
        
        # Iniciar Electron
        Write-Host "Ejecutando: npm run electron" -ForegroundColor Gray
        npm run electron
        
        Set-Location ".."
        
    } else {
        Write-Host ""
        Write-Host "✗ No se puede iniciar Electron" -ForegroundColor Red
        Write-Host "   Backend OK: $backendOK" -ForegroundColor Gray  
        Write-Host "   Frontend OK: $frontendOK" -ForegroundColor Gray
        Write-Host ""
        Write-Host "URLs para verificar manualmente:" -ForegroundColor Yellow
        Write-Host "   Backend:  http://localhost:$backendPort/swagger" -ForegroundColor Cyan
        Write-Host "   Frontend: http://localhost:$frontendPort" -ForegroundColor Cyan
        Write-Host "   Health:   http://localhost:$backendPort/api/system/health" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host ""
    Write-Host "✗ Error durante el inicio: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Para diagnosticar el problema ejecuta:" -ForegroundColor Yellow
    Write-Host "   .\diagnose-backend.ps1" -ForegroundColor Cyan
    
} finally {
    # Limpiar trabajos
    Write-Host ""
    Write-Host "Limpiando trabajos en background..." -ForegroundColor Gray
    
    if ($backendJob) {
        Stop-Job $backendJob -ErrorAction SilentlyContinue
        Remove-Job $backendJob -ErrorAction SilentlyContinue
        Write-Host "   Backend job limpiado" -ForegroundColor Gray
    }
    
    if ($frontendJob) {
        Stop-Job $frontendJob -ErrorAction SilentlyContinue  
        Remove-Job $frontendJob -ErrorAction SilentlyContinue
        Write-Host "   Frontend job limpiado" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "INFORMACION FINAL" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Si Electron no se abrio:" -ForegroundColor Yellow
Write-Host "1. Verifica que los servicios sigan corriendo" -ForegroundColor White
Write-Host "2. Prueba manualmente en http://localhost:$frontendPort" -ForegroundColor White
Write-Host "3. Revisa errores en la consola de Electron (F12)" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales de login:" -ForegroundColor Yellow
Write-Host "   Usuario: admin" -ForegroundColor White
Write-Host "   Contraseña: admin123" -ForegroundColor White
Write-Host ""
Write-Host "Para reiniciar manualmente:" -ForegroundColor Yellow
Write-Host "   .\start-backend-only.ps1    # En una terminal" -ForegroundColor Gray
Write-Host "   cd frontend && npm run dev  # En otra terminal" -ForegroundColor Gray
Write-Host "   cd frontend && npm run electron  # Cuando ambos funcionen" -ForegroundColor Gray