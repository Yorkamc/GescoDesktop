# 🔍 Herramienta de validación de seguridad
Write-Host "🔍 Ejecutando validación de seguridad..." -ForegroundColor Cyan

$issues = @()

# Verificar .env
if (-not (Test-Path "backend\.env")) {
    $issues += "❌ Archivo .env no encontrado"
} else {
    $envContent = Get-Content "backend\.env" -Raw
    if ($envContent -match "admin123|default|example") {
        $issues += "⚠️ Credenciales por defecto detectadas en .env"
    }
    if ($envContent -notmatch "JWT_SECRET_KEY=.{32,}") {
        $issues += " JWT_SECRET_KEY muy corto o no configurado"
    }
}

# Verificar que no hay credenciales hardcodeadas
$files = Get-ChildItem -Recurse -Include *.cs,*.tsx,*.ts,*.js | Where-Object { $_.FullName -notlike "*node_modules*" }
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "admin123|password.*=.*['""].*['""]|secret.*=.*['""].*['""]") {
        $issues += " Posibles credenciales hardcodeadas en: $($file.FullName)"
    }
}

# Verificar configuración HTTPS
if (-not (Get-Content "backend/src/Gesco.Desktop.UI/appsettings.json" | Select-String "https")) {
    $issues += " HTTPS no configurado por defecto"
}

# Mostrar resultados
if ($issues.Count -eq 0) {
    Write-Host " Validación de seguridad exitosa - No se encontraron problemas" -ForegroundColor Green
} else {
    Write-Host " Se encontraron los siguientes problemas de seguridad:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  $issue" -ForegroundColor Yellow
    }
}

Write-Host "
 Reporte de seguridad generado en security-report.txt" -ForegroundColor Cyan
"# Reporte de Seguridad - 08/26/2025 18:44:07
## Problemas encontrados:


## Recomendaciones:
1. Mantener .env fuera del control de versiones
2. Usar certificados SSL en producción
3. Revisar logs de auditoría regularmente
4. Mantener backups actualizados
5. Monitorear intentos de acceso fallidos
" | Out-File -FilePath "security-report.txt" -Encoding UTF8
