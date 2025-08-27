#  Script de inicio sincronizado para desarrollo
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "   INICIANDO GESCO DESKTOP - FULL STACK" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Función para verificar si un puerto está en uso
function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    } catch {
        return $false
    }
}

# Verificar puertos
Write-Host "`n Verificando puertos..." -ForegroundColor Yellow

if (Test-Port -Port 5100) {
    Write-Host "    Puerto 5100 ya está en uso" -ForegroundColor Yellow
    $choice = Read-Host "   ¿Continuar de todas formas? (y/n)"
    if ($choice -ne 'y') { exit }
}

if (Test-Port -Port 5173) {
    Write-Host "    Puerto 5173 ya está en uso" -ForegroundColor Yellow
}

# Iniciar Backend
Write-Host "`n Iniciando Backend (.NET)..." -ForegroundColor Yellow
Write-Host "   Puerto: http://localhost:5100" -ForegroundColor Cyan
Write-Host "   Swagger: http://localhost:5100/" -ForegroundColor Cyan

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd backend/src/Gesco.Desktop.UI; Write-Host ' Iniciando Backend API...' -ForegroundColor Green; dotnet run"

# Esperar que el backend esté listo
Write-Host "`n⏳ Esperando que el backend inicie..." -ForegroundColor Yellow
do {
    Start-Sleep -Seconds 2
    Write-Host "   Verificando..." -ForegroundColor Gray
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5100/api/health" -TimeoutSec 3
        $backendReady = $true
    } catch {
        $backendReady = $false
    }
} while (-not $backendReady)

Write-Host "   ✅ Backend listo!" -ForegroundColor Green

# Iniciar Frontend
Write-Host "`n🎯 Iniciando Frontend (React + Vite)..." -ForegroundColor Yellow
Write-Host "   Puerto: http://localhost:5173" -ForegroundColor Cyan

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd frontend; Write-Host '🎯 Iniciando Frontend React...' -ForegroundColor Blue; npm run dev"

Write-Host "`n🎉 APLICACIÓN INICIADA!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "   🔧 Backend: http://localhost:5100" -ForegroundColor White
Write-Host "   📚 Swagger: http://localhost:5100/" -ForegroundColor White  
Write-Host "   🎯 Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Yellow
Write-Host "   - Espera unos segundos para que React compile" -ForegroundColor White
Write-Host "   - Si hay errores CORS, reinicia ambos servicios" -ForegroundColor White
Write-Host "   - Usa Ctrl+C para detener los servicios" -ForegroundColor White

# Mantener la ventana abierta
Read-Host "`nPresiona ENTER para cerrar este script (los servicios seguirán ejecutándose)"
