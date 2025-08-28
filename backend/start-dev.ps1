# Script mejorado para iniciar entorno de desarrollo
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  🚀 INICIANDO GESCO DESKTOP API" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Configurar entorno de desarrollo
$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host "🌐 Entorno configurado: Development" -ForegroundColor Green

# Verificar que existe el proyecto
if (-not (Test-Path "src\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj")) {
    Write-Host "❌ Error: No se encuentra el proyecto." -ForegroundColor Red
    Write-Host "   Asegúrate de estar en el directorio 'backend'" -ForegroundColor Yellow
    exit 1
}

# Actualizar paquetes vulnerables si es necesario
Write-Host "🔐 Verificando paquetes de seguridad..." -ForegroundColor Yellow
$coreProjectPath = "src\Gesco.Desktop.Core\Gesco.Desktop.Core.csproj"
$projectContent = Get-Content $coreProjectPath -Raw

if ($projectContent -match 'System\.IdentityModel\.Tokens\.Jwt.*Version="7\.0\.3"') {
    Write-Host "⚠️ Paquete JWT vulnerable detectado. Actualizando..." -ForegroundColor Yellow
    
    # Actualizar paquetes de seguridad
    dotnet add $coreProjectPath package System.IdentityModel.Tokens.Jwt --version 8.0.2 | Out-Null
    dotnet add $coreProjectPath package Microsoft.IdentityModel.Tokens --version 8.0.2 | Out-Null
    
    Write-Host "✅ Paquetes de seguridad actualizados" -ForegroundColor Green
}

# Restaurar paquetes
Write-Host "📦 Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore --verbosity quiet

# Compilar proyecto
Write-Host "🔨 Compilando proyecto..." -ForegroundColor Yellow
dotnet build --configuration Debug --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al compilar el proyecto" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Compilación exitosa" -ForegroundColor Green

# Mostrar información de inicio
Write-Host ""
Write-Host "🌟 GESCO DESKTOP API - DESARROLLO" -ForegroundColor Cyan
Write-Host "=================================="
Write-Host "🔗 URLs principales:" -ForegroundColor Yellow
Write-Host "   API Base:    http://localhost:5100/api" -ForegroundColor White
Write-Host "   Swagger:     http://localhost:5100/swagger" -ForegroundColor White
Write-Host "   Health:      http://localhost:5100/api/system/health" -ForegroundColor White
Write-Host "   Ping:        http://localhost:5100/ping" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Endpoints de prueba:" -ForegroundColor Yellow
Write-Host "   Login:       POST /api/auth/login" -ForegroundColor White
Write-Host "   Stats:       GET  /api/system/stats" -ForegroundColor White
Write-Host "   License:     GET  /api/license/status" -ForegroundColor White
Write-Host ""
Write-Host "🔑 Credenciales por defecto:" -ForegroundColor Yellow
Write-Host "   Usuario: admin" -ForegroundColor White
Write-Host "   Password: admin123" -ForegroundColor White
Write-Host ""

# Iniciar aplicación
Write-Host "🚀 Iniciando servidor de desarrollo..." -ForegroundColor Green
Write-Host "   (Presiona Ctrl+C para detener)" -ForegroundColor Gray
Write-Host ""

Set-Location src\Gesco.Desktop.UI

# Ejecutar con variables de entorno específicas
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5100"

try {
    dotnet run
}
finally {
    Set-Location ..\..
    Write-Host ""
    Write-Host "👋 Servidor detenido" -ForegroundColor Yellow
}