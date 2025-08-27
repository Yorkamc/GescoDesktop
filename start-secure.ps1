# Script de inicio seguro
Write-Host "🔐 Verificando configuración de seguridad..." -ForegroundColor Cyan

# Verificar que existe .env
if (-not (Test-Path "backend\.env")) {
    Write-Host "❌ Error: Archivo .env no encontrado" -ForegroundColor Red
    Write-Host "   Ejecuta primero: .\fix-security-issues.ps1" -ForegroundColor Yellow
    exit 1
}

# Verificar variables críticas
$envContent = Get-Content "backend\.env" -Raw
if ($envContent -notmatch "JWT_SECRET_KEY=") {
    Write-Host "❌ Error: JWT_SECRET_KEY no configurado" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Configuración de seguridad verificada" -ForegroundColor Green
Write-Host "🚀 Iniciando aplicación..." -ForegroundColor Cyan

# Continuar con el inicio normal
& ".\start-dev.ps1"
