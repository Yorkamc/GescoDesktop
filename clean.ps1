# clean.ps1 - Limpiar proyecto completamente (SIN EMOJIS)
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  LIMPIEZA COMPLETA DEL PROYECTO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

Write-Host ""
Write-Host "Limpiando backend..." -ForegroundColor Yellow
if (Test-Path "backend") {
    Set-Location "backend"
    dotnet clean --verbosity quiet | Out-Null
    Set-Location ".."
    Write-Host "   Backend limpiado" -ForegroundColor Green
}

Write-Host ""
Write-Host "Limpiando frontend..." -ForegroundColor Yellow
if (Test-Path "frontend") {
    Set-Location "frontend"
    
    # Limpiar node_modules
    if (Test-Path "node_modules") {
        Write-Host "   Eliminando node_modules..." -ForegroundColor Gray
        Remove-Item "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Limpiar builds
    $buildPaths = @("dist", "dist-electron", ".vite")
    foreach ($buildPath in $buildPaths) {
        if (Test-Path $buildPath) {
            Write-Host "   Eliminando $buildPath..." -ForegroundColor Gray
            Remove-Item $buildPath -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    
    Set-Location ".."
    Write-Host "   Frontend limpiado" -ForegroundColor Green
}

Write-Host ""
Write-Host "Limpiando carpeta dist raiz..." -ForegroundColor Yellow
if (Test-Path "dist") {
    Remove-Item "dist" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   Carpeta dist eliminada" -ForegroundColor Green
}

Write-Host ""
Write-Host "Reinstalando dependencias..." -ForegroundColor Yellow
Set-Location "frontend"
npm install
if ($LASTEXITCODE -eq 0) {
    Write-Host "   Dependencias reinstaladas" -ForegroundColor Green
} else {
    Write-Host "   ERROR reinstalando dependencias" -ForegroundColor Red
}
Set-Location ".."

Write-Host ""
Write-Host "LIMPIEZA COMPLETA FINALIZADA" -ForegroundColor Green
Write-Host ""
Write-Host "Archivos eliminados:" -ForegroundColor Yellow
Write-Host "   - backend/bin, backend/obj" -ForegroundColor Gray
Write-Host "   - frontend/node_modules" -ForegroundColor Gray  
Write-Host "   - frontend/dist" -ForegroundColor Gray
Write-Host "   - frontend/dist-electron" -ForegroundColor Gray
Write-Host "   - dist/ (raiz)" -ForegroundColor Gray
Write-Host ""
Write-Host "Para continuar desarrollo:" -ForegroundColor Cyan
Write-Host "   .\dev.ps1" -ForegroundColor White