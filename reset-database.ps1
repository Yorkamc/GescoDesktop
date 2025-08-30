# reset-database-fixed.ps1 - Regenerar base de datos sin problemas de codificacion
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  REGENERANDO BASE DE DATOS GESCO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Detener cualquier proceso que pueda estar usando la DB
Write-Host ""
Write-Host "Deteniendo procesos que puedan usar la DB..." -ForegroundColor Yellow

# Buscar y matar procesos de dotnet que puedan estar usando la DB
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "   Terminando proceso dotnet: $($_.Id)" -ForegroundColor Gray
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

Start-Sleep -Seconds 2

# Ubicacion de la base de datos
$dbPath = "backend/src/Gesco.Desktop.UI/data/gesco_local.db"
$dataDir = "backend/src/Gesco.Desktop.UI/data"

# Eliminar base de datos existente
Write-Host ""
Write-Host "Eliminando base de datos anterior..." -ForegroundColor Yellow

if (Test-Path $dbPath) {
    try {
        Remove-Item $dbPath -Force
        Write-Host "   Base de datos eliminada: $dbPath" -ForegroundColor Green
    } catch {
        Write-Host "   No se pudo eliminar la DB (puede estar en uso): $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "   Intenta cerrar todas las ventanas de la aplicacion y reintenta" -ForegroundColor Gray
        exit 1
    }
} else {
    Write-Host "   No se encontro base de datos anterior" -ForegroundColor Gray
}

# Eliminar archivos relacionados
$relatedFiles = @("$dbPath-wal", "$dbPath-shm")
foreach ($file in $relatedFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Force -ErrorAction SilentlyContinue
        Write-Host "   Eliminado: $file" -ForegroundColor Gray
    }
}

# Asegurar que el directorio existe
Write-Host ""
Write-Host "Preparando directorio de datos..." -ForegroundColor Yellow
if (-not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Write-Host "   Directorio creado: $dataDir" -ForegroundColor Green
} else {
    Write-Host "   Directorio ya existe: $dataDir" -ForegroundColor Green
}

# Regenerar base de datos
Write-Host ""
Write-Host "Regenerando base de datos..." -ForegroundColor Yellow

Set-Location "backend"

try {
    # Eliminar migraciones existentes (opcional)
    $migrationsPath = "src/Gesco.Desktop.Data/Migrations"
    if (Test-Path $migrationsPath) {
        Write-Host "   Eliminando migraciones anteriores..." -ForegroundColor Gray
        Remove-Item $migrationsPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    # Crear nueva migracion
    Write-Host "   Creando nueva migracion..." -ForegroundColor Gray
    $migrationResult = dotnet ef migrations add "InitialCreate" -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error creando migracion" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }

    # Aplicar migracion (esto creara la DB con datos semilla)
    Write-Host "   Aplicando migracion y datos semilla..." -ForegroundColor Gray
    $updateResult = dotnet ef database update -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error aplicando migracion" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }

    Write-Host "   Base de datos regenerada exitosamente" -ForegroundColor Green

} catch {
    Write-Host "Error durante la regeneracion: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Set-Location ".."

# Verificar que la base de datos fue creada
Write-Host ""
Write-Host "Verificando base de datos..." -ForegroundColor Yellow

if (Test-Path $dbPath) {
    $dbSize = (Get-Item $dbPath).Length
    Write-Host "   Base de datos creada: $dbPath ($dbSize bytes)" -ForegroundColor Green
} else {
    Write-Host "   La base de datos no fue creada" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "BASE DE DATOS REGENERADA EXITOSAMENTE!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Informacion de la base de datos:" -ForegroundColor White
Write-Host "   Ubicacion: $dbPath" -ForegroundColor Cyan
Write-Host "   Usuario: admin" -ForegroundColor Yellow
Write-Host "   Contraseña: admin123" -ForegroundColor Yellow
Write-Host ""
Write-Host "Proximos pasos:" -ForegroundColor White
Write-Host "   1. Inicia el backend: cd backend && dotnet run" -ForegroundColor Gray
Write-Host "   2. Inicia el frontend: cd frontend && npm run dev" -ForegroundColor Gray
Write-Host "   3. Prueba el login en: http://localhost:5173" -ForegroundColor Gray
Write-Host ""
Write-Host "Para Electron:" -ForegroundColor White
Write-Host "   cd frontend && npm run dev:electron" -ForegroundColor Gray
Write-Host "=============================================" -ForegroundColor Green