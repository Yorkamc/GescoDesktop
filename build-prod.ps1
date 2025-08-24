# Script para compilar versión de producción

Write-Host "Building GESCO Desktop for Production..." -ForegroundColor Cyan

# Build .NET API
Write-Host "Building API..." -ForegroundColor Yellow
dotnet publish src\Gesco.Desktop.API\Gesco.Desktop.API.csproj -c Release -o dist\api

# Build Electron app
Write-Host "Building Electron app..." -ForegroundColor Yellow
Set-Location src\Gesco.Desktop.UI
npm run dist
Set-Location ..\..

Write-Host "Production build complete!" -ForegroundColor Green
Write-Host "API: dist\api\" -ForegroundColor Yellow
Write-Host "Electron: src\Gesco.Desktop.UI\dist\" -ForegroundColor Yellow
