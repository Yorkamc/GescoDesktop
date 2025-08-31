# dev.ps1 - Script principal para desarrollo (SIN EMOJIS)
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  GESCO DESKTOP - DESARROLLO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

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

# Verificaciones basicas
if (-not (Test-Path "frontend/package.json")) {
    Write-Host "ERROR: Ejecuta desde la raiz del proyecto" -ForegroundColor Red
    exit 1
}

# Mostrar informacion
Write-Host ""
Write-Host "Iniciando servicios de desarrollo..." -ForegroundColor Yellow
Write-Host "   Backend:  http://localhost:5100" -ForegroundColor Cyan
Write-Host "   Frontend: http://localhost:5173" -ForegroundColor Cyan
Write-Host "   Swagger:  http://localhost:5100/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Credenciales: admin / admin123" -ForegroundColor Yellow
Write-Host ""

# Abrir terminales separadas para cada servicio
Write-Host "Abriendo terminales..." -ForegroundColor Green

# Terminal Backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
Write-Host 'BACKEND - GESCO DESKTOP' -ForegroundColor Green;
Write-Host 'Puerto: 5100' -ForegroundColor Yellow;
Write-Host '';
cd '$PWD\backend';
dotnet run --project src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj
"@

Start-Sleep -Seconds 3

# Terminal Frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
Write-Host 'FRONTEND - GESCO DESKTOP' -ForegroundColor Blue;
Write-Host 'Puerto: 5173' -ForegroundColor Yellow;
Write-Host '';
cd '$PWD\frontend';
npm run dev
"@

Start-Sleep -Seconds 5

# Terminal Electron (opcional)
$openElectron = Read-Host "Abrir Electron automaticamente? (y/n)"
if ($openElectron -eq 'y' -or $openElectron -eq 'Y') {
    Start-Sleep -Seconds 5
    Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
Write-Host 'ELECTRON - GESCO DESKTOP' -ForegroundColor Magenta;
Write-Host 'Esperando servicios...' -ForegroundColor Yellow;
cd '$PWD\frontend';
Start-Sleep -Seconds 10;
Write-Host 'Iniciando Electron...' -ForegroundColor Green;
npm run electron
"@
}

Write-Host ""
Write-Host "Servicios iniciados en terminales separadas!" -ForegroundColor Green
Write-Host ""
Write-Host "Para Electron manual:" -ForegroundColor Yellow
Write-Host "   cd frontend && npm run electron" -ForegroundColor Gray
Write-Host ""
Write-Host "Para detener servicios: cierra las ventanas de terminal" -ForegroundColor Gray