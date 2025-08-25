# Script para iniciar entorno de desarrollo
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  INICIANDO GESCO DESKTOP" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que existe el proyecto
if (-not (Test-Path "src\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj")) {
    Write-Host " Error: No se encuentra el proyecto. Ejecuta primero los scripts de reestructuración." -ForegroundColor Red
    exit 1
}

# Restaurar paquetes
Write-Host " Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore

# Compilar proyecto
Write-Host "`n🔨 Compilando proyecto..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host " Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}

# Iniciar API local
Write-Host "`n Iniciando API local..." -ForegroundColor Yellow
Write-Host "   URL: http://localhost:5100" -ForegroundColor Cyan
Write-Host "   Swagger: http://localhost:5100/swagger" -ForegroundColor Cyan
Write-Host ""

Set-Location src\Gesco.Desktop.UI
dotnet run
