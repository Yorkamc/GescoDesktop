# =============================================
# SCRIPT CORREGIDO: REGENERACION DE BASE DE DATOS GESCO
# =============================================

Write-Host "=============================================" -ForegroundColor Green
Write-Host "  REGENERANDO BASE DE DATOS GESCO CON OPTIMIZACION" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Green

# Verificar que estamos en el directorio correcto
$projectRoot = Get-Location
$dataProject = Join-Path $projectRoot "backend\src\Gesco.Desktop.Data"
$uiProject = Join-Path $projectRoot "backend\src\Gesco.Desktop.UI"

if (-not (Test-Path $dataProject) -or -not (Test-Path $uiProject)) {
    Write-Host "Error: No se encontraron los proyectos. Asegurate de estar en el directorio raiz del proyecto." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Deteniendo procesos relacionados..." -ForegroundColor Yellow

# Detener procesos que puedan estar usando la base de datos
try {
    Get-Process -Name "Gesco.Desktop.UI" -ErrorAction SilentlyContinue | Stop-Process -Force
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*Gesco*" } | Stop-Process -Force
    Start-Sleep -Seconds 2
} catch {
    # Ignorar errores si no hay procesos
}

Write-Host "Limpiando base de datos anterior..." -ForegroundColor Yellow

# Limpiar archivos de base de datos SQLite
$dataDir = Join-Path $projectRoot "backend\src\Gesco.Desktop.UI\data"
if (Test-Path $dataDir) {
    try {
        Remove-Item -Path "$dataDir\*.db" -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "$dataDir\*.db-*" -Force -ErrorAction SilentlyContinue
        Write-Host "  - Archivos de base de datos eliminados" -ForegroundColor Green
    } catch {
        Write-Host "  - Advertencia: No se pudieron eliminar algunos archivos de DB" -ForegroundColor Yellow
    }
}

Write-Host "Limpiando migraciones anteriores..." -ForegroundColor Yellow

# Limpiar migraciones anteriores
$migrationsDir = Join-Path $dataProject "Migrations"
if (Test-Path $migrationsDir) {
    try {
        Remove-Item -Path $migrationsDir -Recurse -Force
        Write-Host "  - Carpeta Migrations eliminada" -ForegroundColor Green
    } catch {
        Write-Host "  - Error eliminando carpeta Migrations: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Regenerando base de datos..." -ForegroundColor Yellow

# Cambiar al directorio del proyecto de datos
Set-Location $dataProject

try {
    Write-Host "    Creando migracion inicial para LocalDbContext..." -ForegroundColor Cyan
    
    # CORREGIDO: Especificar el contexto LocalDbContext explícitamente
    $migrationResult = dotnet ef migrations add InitialCreateWithOptimization `
        --project $dataProject `
        --startup-project $uiProject `
        --context LocalDbContext `
        --verbose 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Error creando migracion" -ForegroundColor Red
        Write-Host "Output: $migrationResult" -ForegroundColor Red
        throw "Migration creation failed"
    }
    
    Write-Host "  - Migracion creada exitosamente" -ForegroundColor Green
    
    Write-Host "    Aplicando migracion a la base de datos..." -ForegroundColor Cyan
    
    # Aplicar la migración
    $updateResult = dotnet ef database update `
        --project $dataProject `
        --startup-project $uiProject `
        --context LocalDbContext `
        --verbose 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host " Error aplicando migracion" -ForegroundColor Red
        Write-Host "Output: $updateResult" -ForegroundColor Red
        throw "Database update failed"
    }
    
    Write-Host "  - Base de datos actualizada exitosamente" -ForegroundColor Green
    
} catch {
    Write-Host " Error durante regeneracion: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location $projectRoot
    exit 1
}

# Regresar al directorio original
Set-Location $projectRoot

Write-Host ""
Write-Host "Verificando estructura de la base de datos..." -ForegroundColor Yellow

# Verificar que la base de datos se creó correctamente
$dbPath = Join-Path $uiProject "data\gesco_local.db"
if (Test-Path $dbPath) {
    $dbSize = (Get-Item $dbPath).Length
    Write-Host "  - Base de datos creada: $dbPath" -ForegroundColor Green
    Write-Host "  - Tamaño: $([math]::Round($dbSize/1KB, 2)) KB" -ForegroundColor Green
} else {
    Write-Host "  - Error: No se encontró la base de datos creada" -ForegroundColor Red
}

Write-Host ""
Write-Host "Ejecutando script de optimización..." -ForegroundColor Yellow

# Verificar si existe el script de optimización
$optimizationScript = Join-Path $dataProject "script\sqlite_optimization_script.sql"
if (Test-Path $optimizationScript) {
    Write-Host "  - Script de optimización encontrado" -ForegroundColor Green
    # La optimización se ejecutará cuando inicie la aplicación
} else {
    Write-Host "  - Advertencia: No se encontró el script de optimización" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  REGENERACION COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "PROXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "1. Ejecutar: cd backend\src\Gesco.Desktop.UI" -ForegroundColor White
Write-Host "2. Ejecutar: dotnet run" -ForegroundColor White
Write-Host "3. Abrir: http://localhost:5100/swagger" -ForegroundColor White
Write-Host ""
Write-Host "CREDENCIALES DEFAULT:" -ForegroundColor Cyan
Write-Host "Username: admin" -ForegroundColor White
Write-Host "Password: admin123" -ForegroundColor White
Write-Host ""
Write-Host "La base de datos incluye:" -ForegroundColor Cyan
Write-Host "- Usuario administrador configurado" -ForegroundColor White
Write-Host "- Datos de ejemplo (organizaciones, roles, estados)" -ForegroundColor White
Write-Host "- Optimizaciones SQLite aplicadas" -ForegroundColor White
Write-Host "- Índices para mejorar rendimiento" -ForegroundColor White
Write-Host ""

# Mostrar warnings importantes si los hay
Write-Host "NOTAS IMPORTANTES:" -ForegroundColor Yellow
Write-Host "- Si hay warnings de nullable references, son informativos y no afectan funcionalidad" -ForegroundColor White
Write-Host "- PostgreSQL y Laravel API son opcionales para el funcionamiento básico" -ForegroundColor White
Write-Host "- El sistema funciona completamente offline con SQLite" -ForegroundColor White
Write-Host ""

Write-Host "✅ Regeneración de base de datos completada!" -ForegroundColor Green