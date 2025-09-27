# =============================================
# SCRIPT REGENERACION DE BASE DE DATOS GESCO
# =============================================

param(
    [switch]$SkipMigration,
    [switch]$OnlyClean,
    [switch]$Verbose
)

Write-Host "=============================================" -ForegroundColor Green
Write-Host "  REGENERANDO BASE DE DATOS GESCO CON SEED DATA" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Green

# Verificar que estamos en el directorio correcto
$projectRoot = Get-Location
$dataProject = Join-Path $projectRoot "backend\src\Gesco.Desktop.Data"
$uiProject = Join-Path $projectRoot "backend\src\Gesco.Desktop.UI"

if (-not (Test-Path $dataProject) -or -not (Test-Path $uiProject)) {
    Write-Host "Error: No se encontraron los proyectos. Asegurate de estar en el directorio raiz del proyecto." -ForegroundColor Red
    Write-Host "Directorio actual: $projectRoot" -ForegroundColor Yellow
    Write-Host "Buscando: $dataProject" -ForegroundColor Yellow
    Write-Host "Buscando: $uiProject" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Configuracion detectada:" -ForegroundColor Cyan
Write-Host "- Proyecto raiz: $projectRoot" -ForegroundColor White
Write-Host "- Proyecto Data: $dataProject" -ForegroundColor White
Write-Host "- Proyecto UI: $uiProject" -ForegroundColor White

Write-Host ""
Write-Host "Deteniendo procesos relacionados..." -ForegroundColor Yellow

# Detener procesos que puedan estar usando la base de datos
try {
    $gescoProcesses = Get-Process -Name "Gesco.Desktop.UI" -ErrorAction SilentlyContinue
    if ($gescoProcesses) {
        $gescoProcesses | Stop-Process -Force
        Write-Host "- Procesos Gesco detenidos: $($gescoProcesses.Count)" -ForegroundColor Green
    }
    
    $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { 
        $_.MainWindowTitle -like "*Gesco*" -or $_.ProcessName -like "*Gesco*" 
    }
    if ($dotnetProcesses) {
        $dotnetProcesses | Stop-Process -Force
        Write-Host "- Procesos dotnet relacionados detenidos: $($dotnetProcesses.Count)" -ForegroundColor Green
    }
    
    Start-Sleep -Seconds 3
    Write-Host "- Procesos detenidos correctamente" -ForegroundColor Green
} catch {
    Write-Host "- No hay procesos activos que detener" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Limpiando archivos anteriores..." -ForegroundColor Yellow

# Limpiar archivos de base de datos SQLite en multiples ubicaciones
$possibleDataDirs = @(
    (Join-Path $uiProject "data"),
    (Join-Path $projectRoot "backend\data"),
    (Join-Path $projectRoot "data"),
    (Join-Path $uiProject "bin\Debug\net8.0\data")
)

$filesRemoved = 0
foreach ($dataDir in $possibleDataDirs) {
    if (Test-Path $dataDir) {
        try {
            $dbFiles = Get-ChildItem -Path $dataDir -Filter "*.db*" -ErrorAction SilentlyContinue
            if ($dbFiles) {
                $dbFiles | Remove-Item -Force -ErrorAction SilentlyContinue
                $filesRemoved += $dbFiles.Count
                Write-Host "- Eliminados $($dbFiles.Count) archivos de: $dataDir" -ForegroundColor Green
            }
        } catch {
            Write-Host "- Advertencia: No se pudieron eliminar archivos de: $dataDir" -ForegroundColor Yellow
        }
    }
}

if ($filesRemoved -eq 0) {
    Write-Host "- No se encontraron archivos de base de datos para eliminar" -ForegroundColor Gray
} else {
    Write-Host "- Total de archivos DB eliminados: $filesRemoved" -ForegroundColor Green
}

# Limpiar migraciones anteriores
$migrationsDir = Join-Path $dataProject "Migrations"
if (Test-Path $migrationsDir) {
    try {
        Remove-Item -Path $migrationsDir -Recurse -Force
        Write-Host "- Carpeta Migrations eliminada" -ForegroundColor Green
    } catch {
        Write-Host "- Error eliminando carpeta Migrations: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "- No hay migraciones anteriores que limpiar" -ForegroundColor Gray
}

# Limpiar bin y obj para asegurar rebuild limpio
$binObjDirs = @(
    (Join-Path $dataProject "bin"),
    (Join-Path $dataProject "obj"),
    (Join-Path $uiProject "bin"),
    (Join-Path $uiProject "obj")
)

foreach ($dir in $binObjDirs) {
    if (Test-Path $dir) {
        try {
            Remove-Item -Path $dir -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "- Limpiado: $dir" -ForegroundColor Green
        } catch {
            Write-Host "- Advertencia: No se pudo limpiar: $dir" -ForegroundColor Yellow
        }
    }
}

if ($OnlyClean) {
    Write-Host ""
    Write-Host "Limpieza completada. Saliendo por parametro -OnlyClean" -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Construyendo proyectos..." -ForegroundColor Yellow

# Restore y build de los proyectos para asegurar que esten actualizados
try {
    Write-Host "- Restaurando dependencias..." -ForegroundColor Cyan
    $restoreResult = dotnet restore $dataProject 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "- Advertencias en restore (continuando...)" -ForegroundColor Yellow
        if ($Verbose) { Write-Host $restoreResult -ForegroundColor Gray }
    }
    
    Write-Host "- Construyendo proyecto Data..." -ForegroundColor Cyan
    $buildResult = dotnet build $dataProject --no-restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "- Error en build del proyecto Data" -ForegroundColor Red
        if ($Verbose) { Write-Host $buildResult -ForegroundColor Gray }
        throw "Build failed"
    }
    
    Write-Host "- Construccion completada" -ForegroundColor Green
} catch {
    Write-Host "- Error en construccion, continuando con migracion..." -ForegroundColor Yellow
}

if (-not $SkipMigration) {
    Write-Host ""
    Write-Host "Regenerando base de datos..." -ForegroundColor Yellow

    # Cambiar al directorio del proyecto de datos
    Set-Location $dataProject

    try {
        Write-Host "- Creando migracion inicial para LocalDbContext..." -ForegroundColor Cyan
        
        # Crear migracion con nombre descriptivo
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $migrationName = "InitialCreateWithSeedData_$timestamp"
        
        $migrationResult = dotnet ef migrations add $migrationName `
            --project $dataProject `
            --startup-project $uiProject `
            --context LocalDbContext `
            --verbose 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "- Error creando migracion" -ForegroundColor Red
            if ($Verbose) { Write-Host "Output: $migrationResult" -ForegroundColor Red }
            throw "Migration creation failed"
        }
        
        Write-Host "- Migracion creada: $migrationName" -ForegroundColor Green
        
        Write-Host "- Aplicando migracion a la base de datos..." -ForegroundColor Cyan
        
        # Aplicar la migracion
        $updateResult = dotnet ef database update `
            --project $dataProject `
            --startup-project $uiProject `
            --context LocalDbContext `
            --verbose 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "- Error aplicando migracion" -ForegroundColor Red
            if ($Verbose) { Write-Host "Output: $updateResult" -ForegroundColor Red }
            throw "Database update failed"
        }
        
        Write-Host "- Base de datos actualizada exitosamente" -ForegroundColor Green
        
    } catch {
        Write-Host "- Error durante regeneracion: $($_.Exception.Message)" -ForegroundColor Red
        Set-Location $projectRoot
        exit 1
    }

    # Regresar al directorio original
    Set-Location $projectRoot
}

Write-Host ""
Write-Host "Verificando estructura de la base de datos..." -ForegroundColor Yellow

# Buscar la base de datos en multiples ubicaciones posibles
$possibleDbPaths = @(
    (Join-Path $uiProject "data\gesco_local.db"),
    (Join-Path $uiProject "data\gesco_encrypted.db"),
    (Join-Path $projectRoot "backend\data\gesco_local.db"),
    (Join-Path $projectRoot "backend\data\gesco_encrypted.db"),
    (Join-Path $projectRoot "data\gesco_local.db"),
    (Join-Path $uiProject "bin\Debug\net8.0\data\gesco_local.db")
)

$dbFound = $false
foreach ($dbPath in $possibleDbPaths) {
    if (Test-Path $dbPath) {
        $dbSize = (Get-Item $dbPath).Length
        Write-Host "- Base de datos encontrada: $dbPath" -ForegroundColor Green
        Write-Host "- Tamaño: $([math]::Round($dbSize/1KB, 2)) KB" -ForegroundColor Green
        $dbFound = $true
        break
    }
}

if (-not $dbFound) {
    Write-Host "- No se encontro la base de datos en las ubicaciones esperadas" -ForegroundColor Yellow
    Write-Host "- La base de datos se creara automaticamente al iniciar la aplicacion" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Verificando script de optimizacion..." -ForegroundColor Yellow

# Verificar si existe el script de optimizacion
$optimizationScript = Join-Path $dataProject "script\sqlite_optimization_script.sql"
if (Test-Path $optimizationScript) {
    $scriptSize = (Get-Item $optimizationScript).Length
    Write-Host "- Script de optimizacion encontrado ($([math]::Round($scriptSize/1KB, 2)) KB)" -ForegroundColor Green
} else {
    Write-Host "- No se encontro el script de optimizacion" -ForegroundColor Yellow
    Write-Host "- Ubicacion esperada: $optimizationScript" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  REGENERACION COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

Write-Host ""
Write-Host "PROXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "1. cd backend\src\Gesco.Desktop.UI" -ForegroundColor White
Write-Host "2. dotnet run" -ForegroundColor White
Write-Host "3. Abrir: http://localhost:5100/swagger" -ForegroundColor White

Write-Host ""
Write-Host "CREDENCIALES DEFAULT:" -ForegroundColor Cyan
Write-Host "Username: admin" -ForegroundColor White
Write-Host "Password: admin123" -ForegroundColor White
Write-Host "Cedula (ID): 118640123" -ForegroundColor White

Write-Host ""
Write-Host "ENDPOINTS PARA TESTING:" -ForegroundColor Cyan
Write-Host "Login:  POST /api/Auth/login" -ForegroundColor White
Write-Host "Health: GET  /ping" -ForegroundColor White
Write-Host "Stats:  GET  /api/stats" -ForegroundColor White

Write-Host ""
Write-Host "La base de datos incluye:" -ForegroundColor Cyan
Write-Host "- Usuario administrador con hash correcto" -ForegroundColor White
Write-Host "- Datos de ejemplo (organizaciones, roles, estados)" -ForegroundColor White
Write-Host "- Optimizaciones SQLite aplicadas automaticamente" -ForegroundColor White
Write-Host "- Indices para mejorar rendimiento" -ForegroundColor White
Write-Host "- Seed data creada automaticamente" -ForegroundColor White

Write-Host ""
Write-Host "NOTAS IMPORTANTES:" -ForegroundColor Yellow
Write-Host "- La seed data se crea automaticamente al iniciar la aplicacion" -ForegroundColor White
Write-Host "- El hash de contraseña se verifica y corrige automaticamente" -ForegroundColor White
Write-Host "- Si hay warnings de nullable references, son informativos" -ForegroundColor White
Write-Host "- PostgreSQL y Laravel API son opcionales para funcionamiento basico" -ForegroundColor White
Write-Host "- El sistema funciona completamente offline con SQLite" -ForegroundColor White

Write-Host ""
Write-Host "PARA DEBUGGING:" -ForegroundColor Magenta
Write-Host "- Debug password: GET /api/PasswordDebug/debug" -ForegroundColor White
Write-Host "- Fix password:   POST /api/PasswordDebug/fix-admin-password" -ForegroundColor White
Write-Host "- Test encryption: POST /api/test/encryption" -ForegroundColor White

Write-Host ""
Write-Host "Regeneracion de base de datos completada exitosamente!" -ForegroundColor Green
Write-Host ""

# Opcional: Ofrecer iniciar la aplicacion automaticamente
$startApp = Read-Host "Deseas iniciar la aplicacion ahora? (y/N)"
if ($startApp -eq "y" -or $startApp -eq "Y") {
    Write-Host ""
    Write-Host "Iniciando aplicacion..." -ForegroundColor Cyan
    Set-Location $uiProject
    dotnet run
}