# Script para iniciar el entorno de desarrollo completo

Write-Host "Starting GESCO Desktop Development Environment..." -ForegroundColor Cyan

# Start API in background
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src\Gesco.Desktop.API; dotnet watch run"

# Wait for API to start
Start-Sleep -Seconds 5

# Start Electron + React
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src\Gesco.Desktop.UI; npm run electron-dev"

Write-Host "Development environment started!" -ForegroundColor Green
Write-Host "API: http://localhost:5000" -ForegroundColor Yellow
Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "React: http://localhost:3000" -ForegroundColor Yellow
