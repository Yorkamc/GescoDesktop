# clean-backend-simple.ps1
# Version simple del script de limpieza

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Limpiando Backend de GESCO" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Cambiar a la carpeta del script
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Limpiar con dotnet
Write-Host "[1/4] Ejecutando dotnet clean..." -ForegroundColor Yellow
dotnet clean 2>&1 | Out-Null
Write-Host "      Completado" -ForegroundColor Green

# Eliminar bin
Write-Host "[2/4] Eliminando carpetas bin/..." -ForegroundColor Yellow
Get-ChildItem -Path . -Directory -Recurse -Filter "bin" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "      Completado" -ForegroundColor Green

# Eliminar obj
Write-Host "[3/4] Eliminando carpetas obj/..." -ForegroundColor Yellow
Get-ChildItem -Path . -Directory -Recurse -Filter "obj" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "      Completado" -ForegroundColor Green

# Eliminar publish
Write-Host "[4/4] Eliminando carpetas publish/..." -ForegroundColor Yellow
Get-ChildItem -Path . -Directory -Recurse -Filter "publish" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "      Completado" -ForegroundColor Green

Write-Host ""
Write-Host "==================================" -ForegroundColor Green
Write-Host "  Limpieza Completada!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""
Write-Host "Para recompilar ejecuta:" -ForegroundColor Yellow
Write-Host "  dotnet restore" -ForegroundColor White
Write-Host "  dotnet build" -ForegroundColor White
Write-Host ""