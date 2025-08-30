# dev-utils.ps1 - Utilidades para desarrollo diario

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("start", "stop", "restart", "clean", "build", "migrate", "backup", "status", "help")]
    [string]$Action = "help"
)

function Show-Help {
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host "  GESCO DESKTOP - UTILIDADES DE DESARROLLO" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Uso: .\dev-utils.ps1 <accion>" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Acciones disponibles:" -ForegroundColor Green
    Write-Host "  start    - Iniciar entorno completo (backend + frontend + electron)" -ForegroundColor White
    Write-Host "  stop     - Detener todos los procesos" -ForegroundColor White
    Write-Host "  restart  - Reiniciar todo" -ForegroundColor White
    Write-Host "  clean    - Limpiar cache y dependencias" -ForegroundColor White
    Write-Host "  build    - Compilar para produccion" -ForegroundColor White
    Write-Host "  migrate  - Crear y aplicar migraciones de DB" -ForegroundColor White
    Write-Host "  backup   - Hacer backup de la base de datos" -ForegroundColor White
    Write-Host "  status   - Ver estado de servicios" -ForegroundColor White
    Write-Host "  help     - Mostrar esta ayuda" -ForegroundColor White
    Write-Host ""
    Write-Host "Ejemplos:" -ForegroundColor Yellow
    Write-Host "  .\dev-utils.ps1 start    # Iniciar todo" -ForegroundColor Gray
    Write-Host "  .\dev-utils.ps1 status   # Ver estado" -ForegroundColor Gray
    Write-Host "  .\dev-utils.ps1 clean    # Limpiar cache" -ForegroundColor Gray
}

function Start-DevEnvironment {
    Write-Host "Iniciando entorno de desarrollo..." -ForegroundColor Green
    
    # Verificar estructura
    if (-not (Test-Path "backend/src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj")) {
        Write-Host "Error: Ejecuta desde la raiz del proyecto" -ForegroundColor Red
        return
    }
    
    Write-Host "Abriendo terminales para cada servicio..." -ForegroundColor Yellow
    
    # Terminal para backend
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\backend'; Write-Host 'BACKEND - GESCO DESKTOP' -ForegroundColor Green; dotnet run --project src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj"
    
    Start-Sleep -Seconds 3
    
    # Terminal para frontend  
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\frontend'; Write-Host 'FRONTEND - GESCO DESKTOP' -ForegroundColor Blue; npm run dev"
    
    Start-Sleep -Seconds 8
    
    # Terminal para Electron
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\frontend'; Write-Host 'ELECTRON - GESCO DESKTOP' -ForegroundColor Magenta; `$env:NODE_ENV='development'; `$env:ELECTRON_IS_DEV='1'; npx electron ."
    
    Write-Host "Servicios iniciados en terminales separadas!" -ForegroundColor Green
    Write-Host "URLs importantes:" -ForegroundColor Cyan
    Write-Host "  Backend:  http://localhost:5100/swagger" -ForegroundColor White
    Write-Host "  Frontend: http://localhost:5173" -ForegroundColor White
    Write-Host "  Login:    admin / admin123" -ForegroundColor White
}

function Stop-DevEnvironment {
    Write-Host "Deteniendo procesos..." -ForegroundColor Yellow
    
    # Matar procesos relacionados
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -eq "dotnet" } | Stop-Process -Force -ErrorAction SilentlyContinue
    Get-Process -Name "node" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue  
    Get-Process -Name "electron" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    
    Write-Host "Procesos detenidos" -ForegroundColor Green
}

function Restart-DevEnvironment {
    Write-Host "Reiniciando entorno..." -ForegroundColor Yellow
    Stop-DevEnvironment
    Start-Sleep -Seconds 3
    Start-DevEnvironment
}

function Clean-DevEnvironment {
    Write-Host "Limpiando cache y dependencias..." -ForegroundColor Yellow
    
    # Limpiar frontend
    if (Test-Path "frontend/node_modules") {
        Write-Host "  Limpiando node_modules..." -ForegroundColor Gray
        Remove-Item "frontend/node_modules" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    if (Test-Path "frontend/dist") {
        Write-Host "  Limpiando dist frontend..." -ForegroundColor Gray
        Remove-Item "frontend/dist" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Limpiar backend
    Write-Host "  Limpiando backend..." -ForegroundColor Gray
    Set-Location "backend"
    dotnet clean --verbosity quiet | Out-Null
    Set-Location ".."
    
    # Reinstalar dependencias
    Write-Host "  Reinstalando dependencias frontend..." -ForegroundColor Gray
    Set-Location "frontend"
    npm install --silent
    Set-Location ".."
    
    Write-Host "  Restaurando paquetes backend..." -ForegroundColor Gray
    Set-Location "backend"  
    dotnet restore --verbosity quiet | Out-Null
    Set-Location ".."
    
    Write-Host "Limpieza completada!" -ForegroundColor Green
}

function Build-Production {
    Write-Host "Compilando para produccion..." -ForegroundColor Green
    
    # Build backend
    Write-Host "  Compilando backend..." -ForegroundColor Gray
    Set-Location "backend"
    dotnet publish src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj -c Release -o dist --verbosity quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "    Backend compilado en backend/dist/" -ForegroundColor Green
    } else {
        Write-Host "    Error compilando backend" -ForegroundColor Red
    }
    Set-Location ".."
    
    # Build frontend
    Write-Host "  Compilando frontend..." -ForegroundColor Gray
    Set-Location "frontend"
    npm run build --silent
    if ($LASTEXITCODE -eq 0) {
        Write-Host "    Frontend compilado en frontend/dist/" -ForegroundColor Green
    } else {
        Write-Host "    Error compilando frontend" -ForegroundColor Red
    }
    Set-Location ".."
    
    Write-Host "Build completado!" -ForegroundColor Green
}

function Create-Migration {
    Write-Host "Gestion de migraciones..." -ForegroundColor Green
    
    $migrationName = Read-Host "Nombre de la migracion (o ENTER para solo aplicar)"
    
    Set-Location "backend"
    
    if ($migrationName) {
        Write-Host "  Creando migracion: $migrationName" -ForegroundColor Gray
        dotnet ef migrations add $migrationName -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    Migracion creada exitosamente" -ForegroundColor Green
        } else {
            Write-Host "    Error creando migracion" -ForegroundColor Red
            Set-Location ".."
            return
        }
    }
    
    Write-Host "  Aplicando migraciones..." -ForegroundColor Gray
    dotnet ef database update -p src/Gesco.Desktop.Data -s src/Gesco.Desktop.UI
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "    Migraciones aplicadas exitosamente" -ForegroundColor Green
    } else {
        Write-Host "    Error aplicando migraciones" -ForegroundColor Red
    }
    
    Set-Location ".."
}

function Backup-Database {
    Write-Host "Creando backup de base de datos..." -ForegroundColor Green
    
    $dbPath = "backend/src/Gesco.Desktop.UI/data/gesco_local.db"
    
    if (Test-Path $dbPath) {
        $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
        $backupName = "backup_gesco_$timestamp.db"
        
        Copy-Item $dbPath $backupName
        Write-Host "  Backup creado: $backupName" -ForegroundColor Green
        
        $size = (Get-Item $backupName).Length
        Write-Host "  Tamaño: $([math]::Round($size/1KB, 2)) KB" -ForegroundColor Cyan
    } else {
        Write-Host "  No se encontro base de datos" -ForegroundColor Red
    }
}

function Show-Status {
    Write-Host "Estado de servicios GESCO Desktop:" -ForegroundColor Green
    Write-Host ""
    
    # Backend
    try {
        $healthCheck = Invoke-RestMethod -Uri "http://localhost:5100/api/system/health" -TimeoutSec 3
        Write-Host "  Backend:  CORRIENDO ($($healthCheck.status))" -ForegroundColor Green
        Write-Host "    URL: http://localhost:5100/swagger" -ForegroundColor Gray
    } catch {
        Write-Host "  Backend:  NO DISPONIBLE" -ForegroundColor Red
    }
    
    # Frontend
    try {
        $frontendResponse = Invoke-WebRequest -Uri "http://localhost:5173" -TimeoutSec 3
        Write-Host "  Frontend: CORRIENDO (Status: $($frontendResponse.StatusCode))" -ForegroundColor Green
        Write-Host "    URL: http://localhost:5173" -ForegroundColor Gray
    } catch {
        Write-Host "  Frontend: NO DISPONIBLE" -ForegroundColor Red
    }
    
    # Base de datos
    $dbPath = "backend/src/Gesco.Desktop.UI/data/gesco_local.db"
    if (Test-Path $dbPath) {
        $dbSize = (Get-Item $dbPath).Length
        Write-Host "  Base de datos: EXISTE ($([math]::Round($dbSize/1KB, 2)) KB)" -ForegroundColor Green
    } else {
        Write-Host "  Base de datos: NO EXISTE" -ForegroundColor Red
    }
    
    # Procesos
    $dotnetProcesses = @(Get-Process -Name "dotnet" -ErrorAction SilentlyContinue)
    $nodeProcesses = @(Get-Process -Name "node" -ErrorAction SilentlyContinue)
    $electronProcesses = @(Get-Process -Name "electron" -ErrorAction SilentlyContinue)
    
    Write-Host ""
    Write-Host "Procesos activos:" -ForegroundColor Cyan
    Write-Host "  dotnet: $($dotnetProcesses.Count)" -ForegroundColor White
    Write-Host "  node: $($nodeProcesses.Count)" -ForegroundColor White  
    Write-Host "  electron: $($electronProcesses.Count)" -ForegroundColor White
}

# Ejecutar accion
switch ($Action.ToLower()) {
    "start" { Start-DevEnvironment }
    "stop" { Stop-DevEnvironment }
    "restart" { Restart-DevEnvironment }
    "clean" { Clean-DevEnvironment }
    "build" { Build-Production }
    "migrate" { Create-Migration }
    "backup" { Backup-Database }
    "status" { Show-Status }
    "help" { Show-Help }
    default { Show-Help }
}