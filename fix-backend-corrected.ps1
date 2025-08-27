# Script de Reparacion Rapida del Backend
Write-Host "Reparando Backend GESCO..." -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green

# 1. Crear archivo .env si no existe
if (-not (Test-Path "backend/.env")) {
    Write-Host "Creando archivo .env..." -ForegroundColor Yellow
    
    # Crear contenido del archivo .env sin caracteres especiales problematicos
    $envLines = @(
        "# GESCO Desktop - Variables de Entorno",
        "LOCAL_API_PORT=5100",
        "JWT_SECRET_KEY=TuClaveSecretaMuyLargaYSegura2024GescoDesktop12345",
        "SQLITE_DB_PATH=gesco_local.db",
        "SQLITE_PASSWORD=",
        "DATABASE_ENCRYPTION_KEY=",
        "LARAVEL_API_URL=https://api.example.com",
        "LARAVEL_API_KEY=",
        "ASPNETCORE_ENVIRONMENT=Development"
    )
    
    $envLines | Out-File -FilePath "backend/.env" -Encoding UTF8
    Write-Host "Archivo .env creado correctamente" -ForegroundColor Green
}

# 2. Crear directorio de datos
$dataPath = "backend/src/Gesco.Desktop.UI/data"
if (-not (Test-Path $dataPath)) {
    Write-Host "Creando directorio de datos..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $dataPath | Out-Null
    Write-Host "Directorio creado: $dataPath" -ForegroundColor Green
}

# 3. Limpiar y restaurar
Write-Host "Limpiando compilacion anterior..." -ForegroundColor Yellow
Push-Location "backend"

dotnet clean > $null 2>&1
Write-Host "Limpieza completada" -ForegroundColor Green

Write-Host "Restaurando paquetes NuGet..." -ForegroundColor Yellow
$restoreOutput = dotnet restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Paquetes restaurados exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error restaurando paquetes:" -ForegroundColor Red
    Write-Host $restoreOutput -ForegroundColor Yellow
    Pop-Location
    exit 1
}

# 4. Compilar
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
$buildOutput = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilacion exitosa" -ForegroundColor Green
} else {
    Write-Host "Error de compilacion:" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Yellow
    Pop-Location
    exit 1
}

Pop-Location

# 5. Verificar puerto
Write-Host "Verificando puerto 5100..." -ForegroundColor Yellow

$portInUse = $false
try {
    $connection = New-Object System.Net.Sockets.TcpClient
    $connection.Connect("localhost", 5100)
    $connection.Close()
    $portInUse = $true
} catch {
    $portInUse = $false
}

if ($portInUse) {
    Write-Host "ADVERTENCIA: Puerto 5100 ya esta en uso" -ForegroundColor Yellow
    $choice = Read-Host "Detener proceso y continuar? (y/n)"
    if ($choice -eq 'y') {
        # Intentar detener procesos en puerto 5100
        try {
            $processes = Get-NetTCPConnection -LocalPort 5100 -ErrorAction SilentlyContinue
            foreach ($process in $processes) {
                Stop-Process -Id $process.OwningProcess -Force -ErrorAction SilentlyContinue
            }
            Write-Host "Procesos detenidos" -ForegroundColor Green
        } catch {
            Write-Host "No se pudieron detener todos los procesos" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "BACKEND REPARADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host ""
Write-Host "Para iniciar el backend:" -ForegroundColor White
Write-Host "  cd backend/src/Gesco.Desktop.UI" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor Cyan
Write-Host ""
Write-Host "O usar el script completo:" -ForegroundColor White
Write-Host "  ./start-fullstack.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "Si tienes problemas, ejecuta el diagnostico:" -ForegroundColor Yellow
Write-Host "  ./diagnose-backend.ps1" -ForegroundColor Cyan