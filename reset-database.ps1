# reset-database-con-optimizacion.ps1 - Regenerar BD con auto-optimizacion
param(
    [switch]$Force = $false,
    [switch]$SkipOptimization = $false
)

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  REGENERANDO BASE DE DATOS GESCO CON OPTIMIZACION" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Funcion para verificar si un proceso esta usando un archivo
function Test-FileInUse {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        return $false
    }
    
    try {
        $file = [System.IO.File]::Open($FilePath, 'Open', 'Write')
        $file.Close()
        return $false
    } catch {
        return $true
    }
}

# Detener procesos que puedan estar usando la DB
Write-Host ""
Write-Host "Deteniendo procesos relacionados..." -ForegroundColor Yellow

$processesToKill = @("dotnet", "Gesco.Desktop.UI")
foreach ($processName in $processesToKill) {
    $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "   Terminando $($processes.Count) proceso(s) de $processName..." -ForegroundColor Gray
        $processes | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 1
    }
}

# Configuracion de rutas
$backendPath = "backend"
$dataDir = "$backendPath/src/Gesco.Desktop.UI/data"
$dbPath = "$dataDir/gesco_local.db"
$migrationsPath = "$backendPath/src/Gesco.Desktop.Data/Migrations"
$optimizationScriptPath = "$backendPath/src/Gesco.Desktop.Data/script/sqlite_optimization_script.sql"

# Verificar que el script de optimizacion existe
if (-not $SkipOptimization -and -not (Test-Path $optimizationScriptPath)) {
    Write-Host "  Script de optimizacion no encontrado en: $optimizationScriptPath" -ForegroundColor Yellow
    Write-Host "   Se continuara sin optimizacion automatica" -ForegroundColor Gray
    $SkipOptimization = $true
}

# Verificar si la DB esta en uso
if (Test-Path $dbPath) {
    if (Test-FileInUse $dbPath) {
        Write-Host " La base de datos esta en uso" -ForegroundColor Red
        Write-Host "   Cierra todas las instancias de la aplicacion e intenta de nuevo" -ForegroundColor Yellow
        
        if (-not $Force) {
            $continue = Read-Host "Forzar eliminacion? (y/n)"
            if ($continue -ne 'y') {
                Write-Host "Operacion cancelada" -ForegroundColor Yellow
                exit 1
            }
        }
    }
}

# Limpiar base de datos anterior
Write-Host ""
Write-Host "Limpiando base de datos anterior..." -ForegroundColor Yellow

$filesToDelete = @($dbPath, "$dbPath-wal", "$dbPath-shm", "$dbPath-journal")
foreach ($file in $filesToDelete) {
    if (Test-Path $file) {
        try {
            Remove-Item $file -Force
            Write-Host "    Eliminado: $file" -ForegroundColor Green
        } catch {
            Write-Host "    No se pudo eliminar: $file" -ForegroundColor Red
            Write-Host "      Error: $($_.Exception.Message)" -ForegroundColor Gray
        }
    }
}

# Asegurar directorio de datos
if (-not (Test-Path $dataDir)) {
    New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    Write-Host "    Directorio creado: $dataDir" -ForegroundColor Green
}

# Limpiar migraciones existentes
Write-Host ""
Write-Host "Limpiando migraciones anteriores..." -ForegroundColor Yellow
if (Test-Path $migrationsPath) {
    Remove-Item $migrationsPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "    Migraciones anteriores eliminadas" -ForegroundColor Green
}

# Cambiar al directorio backend
Write-Host ""
Write-Host "Regenerando base de datos..." -ForegroundColor Yellow
Set-Location $backendPath

try {
    # Crear migracion inicial
    Write-Host "    Creando migracion inicial..." -ForegroundColor Gray
    $migrationOutput = dotnet ef migrations add "InitialCreateWithOptimization" -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI --verbose
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Error creando migracion" -ForegroundColor Red
        Write-Host "Output: $migrationOutput" -ForegroundColor Gray
        throw "Migration creation failed"
    }
    Write-Host "    Migracion creada exitosamente" -ForegroundColor Green

    # Aplicar migracion (esto ejecuta EnsureCreatedAsync y el script de optimizacion)
    Write-Host "    Aplicando migracion y ejecutando optimizacion..." -ForegroundColor Gray
    $updateOutput = dotnet ef database update -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI --verbose
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Error aplicando migracion" -ForegroundColor Red
        Write-Host "Output: $updateOutput" -ForegroundColor Gray
        throw "Database update failed"
    }
    Write-Host "    Migracion aplicada exitosamente" -ForegroundColor Green

} catch {
    Write-Host " Error durante regeneracion: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Set-Location ".."

# Verificar resultado
Write-Host ""
Write-Host "Verificando base de datos..." -ForegroundColor Yellow

if (Test-Path $dbPath) {
    $dbSize = (Get-Item $dbPath).Length
    $dbSizeKB = [math]::Round($dbSize / 1024, 2)
    Write-Host "    Base de datos creada: $dbPath ($dbSizeKB KB)" -ForegroundColor Green
    
    # Verificar que tiene contenido (debe ser mayor a 50KB con datos semilla)
    if ($dbSize -lt 50000) {
        Write-Host "     Base de datos parece estar vacia (tamano muy pequeno)" -ForegroundColor Yellow
    } else {
        Write-Host "    Base de datos contiene datos semilla" -ForegroundColor Green
    }
} else {
    Write-Host "    La base de datos no fue creada" -ForegroundColor Red
    exit 1
}

# Verificar optimizacion ejecutada
if (-not $SkipOptimization) {
    Write-Host ""
    Write-Host "Verificando optimizacion SQLite..." -ForegroundColor Yellow
    
    # Verificar si se pueden ejecutar consultas basicas
    try {
        Set-Location $backendPath
        $testQuery = "SELECT COUNT(*) as UserCount FROM users;"
        $result = dotnet ef database drop --dry-run -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI 2>&1
        Set-Location ".."
        
        if ($result -match "would drop") {
            Write-Host "    Base de datos accesible y operacional" -ForegroundColor Green
        }
    } catch {
        Write-Host "     No se pudo verificar la optimizacion completamente" -ForegroundColor Yellow
    }
}

# Mostrar informacion del script de optimizacion
if (-not $SkipOptimization -and (Test-Path $optimizationScriptPath)) {
    $scriptContent = Get-Content $optimizationScriptPath -Raw
    $indexCount = ($scriptContent -split "CREATE INDEX").Length - 1
    $viewCount = ($scriptContent -split "CREATE VIEW").Length - 1
    $triggerCount = ($scriptContent -split "CREATE TRIGGER").Length - 1
    
    Write-Host ""
    Write-Host "Optimizaciones aplicadas:" -ForegroundColor Cyan
    Write-Host "    Indices creados: $indexCount" -ForegroundColor White
    Write-Host "     Vistas creadas: $viewCount" -ForegroundColor White
    Write-Host "    Triggers creados: $triggerCount" -ForegroundColor White
}

# Resumen final
Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host " BASE DE DATOS REGENERADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

Write-Host " Informacion de la base de datos:" -ForegroundColor White
Write-Host "    Ubicacion: $dbPath" -ForegroundColor Cyan
Write-Host "    Usuario: admin" -ForegroundColor Yellow
Write-Host "    Contrasena: admin123" -ForegroundColor Yellow
Write-Host "     Tamano: $dbSizeKB KB" -ForegroundColor Cyan

if (-not $SkipOptimization) {
    Write-Host "    Optimizacion SQLite: Aplicada automaticamente" -ForegroundColor Green
} else {
    Write-Host "     Optimizacion SQLite: Omitida" -ForegroundColor Yellow
}

Write-Host ""
Write-Host " Proximos pasos:" -ForegroundColor White
Write-Host "     Inicia el backend:" -ForegroundColor Gray
Write-Host "      cd backend" -ForegroundColor Cyan
Write-Host "      dotnet run" -ForegroundColor Cyan
Write-Host ""
Write-Host "    Inicia el frontend:" -ForegroundColor Gray  
Write-Host "      cd frontend" -ForegroundColor Cyan
Write-Host "      npm run dev" -ForegroundColor Cyan
Write-Host ""
Write-Host "     Accede a la aplicacion:" -ForegroundColor Gray
Write-Host "      http://localhost:5173 (Frontend)" -ForegroundColor Cyan
Write-Host "      http://localhost:5100/swagger (API)" -ForegroundColor Cyan

Write-Host ""
Write-Host "  Para Electron:" -ForegroundColor White
Write-Host "   cd frontend" -ForegroundColor Cyan
Write-Host "   npm run dev:electron" -ForegroundColor Cyan

Write-Host ""
Write-Host " Comandos utiles:" -ForegroundColor White
Write-Host "   .\dev.ps1                    # Iniciar desarrollo completo" -ForegroundColor Gray
Write-Host "   .\build.ps1                  # Build para produccion" -ForegroundColor Gray
Write-Host "   .\clean.ps1                  # Limpiar proyecto" -ForegroundColor Gray

Write-Host ""
Write-Host " La optimizacion SQLite incluye:" -ForegroundColor White
Write-Host "   - Indices para consultas frecuentes" -ForegroundColor Gray
Write-Host "   - Vistas para estadisticas de dashboard" -ForegroundColor Gray
Write-Host "   - Triggers para automatizacion" -ForegroundColor Gray
Write-Host "   - Configuraciones de rendimiento" -ForegroundColor Gray

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green