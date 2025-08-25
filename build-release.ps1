# Script para compilar versión de producción
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  COMPILANDO VERSIÓN DE PRODUCCIÓN" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Limpiar compilaciones anteriores
Write-Host " Limpiando compilaciones anteriores..." -ForegroundColor Yellow
if (Test-Path "publish") {
    Remove-Item -Path "publish" -Recurse -Force
}

# Compilar en modo Release
Write-Host "`n Compilando en modo Release..." -ForegroundColor Yellow
dotnet publish src\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj -c Release -o publish

if ($LASTEXITCODE -ne 0) {
    Write-Host " Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "`n Compilación completada" -ForegroundColor Green
Write-Host "   Archivos en: publish\" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ejecutar la aplicación:" -ForegroundColor Yellow
Write-Host "   cd publish" -ForegroundColor White
Write-Host "   .\Gesco.Desktop.UI.exe" -ForegroundColor White
