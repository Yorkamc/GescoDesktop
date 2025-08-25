# Script para iniciar GESCO Desktop en desarrollo

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  INICIANDO GESCO DESKTOP" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar .env
if (-not (Test-Path ".env")) {
    Write-Host "  Creando .env desde .env.example..." -ForegroundColor Yellow
    Copy-Item ".env.example" ".env" -ErrorAction SilentlyContinue
}

# Mostrar configuración
Write-Host " Configuración:" -ForegroundColor Yellow
Write-Host "   Puerto API: 5100" -ForegroundColor Gray
Write-Host "   Base de datos: SQLite local" -ForegroundColor Gray
Write-Host "   UI: HTML + Alpine.js" -ForegroundColor Gray
Write-Host ""

# Compilar y ejecutar
Write-Host " Iniciando aplicación..." -ForegroundColor Green
try {
    # Restaurar paquetes si es necesario
    if (-not (Test-Path "src/Gesco.Desktop.UI/bin")) {
        Write-Host " Restaurando paquetes NuGet..." -ForegroundColor Yellow
        dotnet restore --verbosity quiet
    }
    
    # Iniciar la aplicación
    Write-Host " Abriendo aplicación en http://localhost:5100" -ForegroundColor Green
    Write-Host ""
    Write-Host " Credenciales por defecto:" -ForegroundColor Cyan
    Write-Host "   Usuario: admin" -ForegroundColor White
    Write-Host "   Contraseña: admin123" -ForegroundColor White
    Write-Host ""
    Write-Host "Para detener: Ctrl+C" -ForegroundColor Yellow
    Write-Host ""
    
    # Abrir navegador después de un momento
    Start-Sleep -Seconds 2
    Start-Process "http://localhost:5100"
    
    # Ejecutar la aplicación
    dotnet run --project src/Gesco.Desktop.UI
}
catch {
    Write-Host " Error al iniciar la aplicación:" -ForegroundColor Red
    Write-Host .Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host " Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "   1. Verificar que .NET 8 esté instalado: dotnet --version" -ForegroundColor White
    Write-Host "   2. Restaurar paquetes: dotnet restore" -ForegroundColor White
    Write-Host "   3. Limpiar y recompilar: dotnet clean && dotnet build" -ForegroundColor White
    exit 1
}
