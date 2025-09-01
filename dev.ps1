# dev.ps1 - Script de desarrollo limpio
Write-Host "Iniciando GESCO Desktop - Desarrollo" -ForegroundColor Green

if (-not (Test-Path "frontend/package.json")) {
    Write-Host "Error: Ejecuta desde la raíz del proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "Backend: http://localhost:5100" -ForegroundColor Cyan
Write-Host "Frontend: http://localhost:5173" -ForegroundColor Cyan
Write-Host "Credenciales: admin / admin123" -ForegroundColor Yellow

# Terminal Backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
Write-Host 'BACKEND GESCO' -ForegroundColor Green;
cd '$PWD\backend';
dotnet run --project src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj
"@

Start-Sleep -Seconds 2

# Terminal Frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", @"
Write-Host 'FRONTEND GESCO' -ForegroundColor Blue;
cd '$PWD\frontend';
npm run dev
"@

Write-Host "Servicios iniciados en terminales separadas" -ForegroundColor Green

# =============================================================================

# clean.ps1 - Script de limpieza limpio
Write-Host "Limpiando proyecto GESCO Desktop..." -ForegroundColor Green

Write-Host "Limpiando backend..." -ForegroundColor Yellow
if (Test-Path "backend") {
    Set-Location "backend"
    dotnet clean --verbosity quiet | Out-Null
    Set-Location ".."
}

Write-Host "Limpiando frontend..." -ForegroundColor Yellow
if (Test-Path "frontend") {
    Set-Location "frontend"
    
    @("node_modules", "dist", "dist-electron", ".vite") | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  Eliminado: $_" -ForegroundColor Gray
        }
    }
    
    Set-Location ".."
}

if (Test-Path "dist") {
    Remove-Item "dist" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Eliminado: dist/" -ForegroundColor Gray
}

Write-Host "Reinstalando dependencias..." -ForegroundColor Yellow
Set-Location "frontend"
npm install | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Dependencias reinstaladas" -ForegroundColor Green
}
Set-Location ".."

Write-Host "Limpieza completada" -ForegroundColor Green

# =============================================================================

# reset-database.ps1 - Script de base de datos limpio
Write-Host "Regenerando base de datos GESCO..." -ForegroundColor Green

Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

$dbPath = "backend/src/Gesco.Desktop.UI/data/gesco_local.db"
$dataDir = "backend/src/Gesco.Desktop.UI/data"

Write-Host "Eliminando base de datos anterior..." -ForegroundColor Yellow
@($dbPath, "$dbPath-wal", "$dbPath-shm") | ForEach-Object {
    if (Test-Path $_) {
        Remove-Item $_ -Force -ErrorAction SilentlyContinue
    }
}

if (-not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
}

Write-Host "Regenerando base de datos..." -ForegroundColor Yellow
Set-Location "backend"

try {
    $migrationsPath = "src/Gesco.Desktop.Data/Migrations"
    if (Test-Path $migrationsPath) {
        Remove-Item $migrationsPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    dotnet ef migrations add "InitialCreate" -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Error creando migración" }

    dotnet ef database update -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Error aplicando migración" }

    Write-Host "Base de datos regenerada exitosamente" -ForegroundColor Green

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Set-Location ".."

if (Test-Path $dbPath) {
    Write-Host "Base de datos creada: $dbPath" -ForegroundColor Green
    Write-Host "Usuario: admin | Contraseña: admin123" -ForegroundColor Yellow
} else {
    Write-Host "Error: La base de datos no fue creada" -ForegroundColor Red
}