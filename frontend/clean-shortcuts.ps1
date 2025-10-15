# Script para limpiar completamente los accesos directos de GESCO Desktop
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " LIMPIEZA DE ACCESOS DIRECTOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para eliminar archivo si existe
function Remove-IfExists {
    param($Path, $Description)
    
    if (Test-Path $Path) {
        Remove-Item $Path -Force -ErrorAction SilentlyContinue
        Write-Host "[OK] Eliminado: $Description" -ForegroundColor Green
    } else {
        Write-Host "[--] No encontrado: $Description" -ForegroundColor Yellow
    }
}

# 1. Eliminar acceso directo del escritorio
Write-Host "`n1. Limpiando Escritorio..." -ForegroundColor White
Remove-IfExists "$env:USERPROFILE\Desktop\GESCO Desktop.lnk" "Escritorio (usuario actual)"
Remove-IfExists "$env:PUBLIC\Desktop\GESCO Desktop.lnk" "Escritorio (todos los usuarios)"

# 2. Eliminar del menú inicio
Write-Host "`n2. Limpiando Menu Inicio..." -ForegroundColor White
Remove-IfExists "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\GESCO Desktop.lnk" "Menu Inicio (usuario)"
Remove-IfExists "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\GESCO Desktop.lnk" "Menu Inicio (todos)"

# 3. Limpiar carpeta de programas del menú inicio
Write-Host "`n3. Limpiando carpetas del Menu Inicio..." -ForegroundColor White
$startMenuPaths = @(
    "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\GESCO Desktop",
    "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\GESCO Desktop"
)

foreach ($path in $startMenuPaths) {
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "[OK] Eliminada carpeta: $path" -ForegroundColor Green
    }
}

# 4. Limpiar caché de iconos
Write-Host "`n4. Limpiando cache de iconos..." -ForegroundColor White
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

ie4uinit.exe -ClearIconCache | Out-Null
Remove-Item -Path "$env:LOCALAPPDATA\IconCache.db" -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\Windows\Explorer" -Filter "iconcache*" -ErrorAction SilentlyContinue | Remove-Item -Force
Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\Windows\Explorer" -Filter "thumbcache*" -ErrorAction SilentlyContinue | Remove-Item -Force

Write-Host "[OK] Cache de iconos limpiado" -ForegroundColor Green

# 5. Reiniciar Explorer
Write-Host "`n5. Reiniciando Explorer..." -ForegroundColor White
Start-Sleep -Seconds 2
Start-Process explorer.exe
Write-Host "[OK] Explorer reiniciado" -ForegroundColor Green

# Resumen
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " LIMPIEZA COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pasos siguientes:" -ForegroundColor Yellow
Write-Host "1. Si la app esta anclada en la barra de tareas:" -ForegroundColor White
Write-Host "   - Clic derecho > Desanclar de la barra de tareas" -ForegroundColor White
Write-Host "2. REINICIA WINDOWS (importante)" -ForegroundColor White
Write-Host "3. Recompila e instala:" -ForegroundColor White
Write-Host "   npm run build:complete" -ForegroundColor Cyan
Write-Host ""