Write-Host "========================================" -ForegroundColor Cyan
Write-Host " LIMPIEZA EXHAUSTIVA DE GESCO DESKTOP" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Función para eliminar archivo si existe
function Remove-IfExists {
    param($Path, $Description)
    
    if (Test-Path $Path) {
        Remove-Item $Path -Force -Recurse -ErrorAction SilentlyContinue
        Write-Host "[OK] Eliminado: $Description" -ForegroundColor Green
    } else {
        Write-Host "[--] No encontrado: $Description" -ForegroundColor Yellow
    }
}

# 1. Desanclar de la barra de tareas (si existe)
Write-Host "`n1. Desanclando de Barra de Tareas..." -ForegroundColor White
try {
    $shell = New-Object -ComObject Shell.Application
    $taskbarPath = [Environment]::GetFolderPath('ApplicationData') + '\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar'
    
    if (Test-Path $taskbarPath) {
        Get-ChildItem -Path $taskbarPath -Filter "*GESCO*.lnk" | ForEach-Object {
            Remove-Item $_.FullName -Force
            Write-Host "[OK] Eliminado de taskbar: $($_.Name)" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "[!!] No se pudo desanclar automáticamente" -ForegroundColor Yellow
}

# 2. Eliminar acceso directo del escritorio
Write-Host "`n2. Limpiando Escritorio..." -ForegroundColor White
Remove-IfExists "$env:USERPROFILE\Desktop\GESCO Desktop.lnk" "Escritorio (usuario actual)"
Remove-IfExists "$env:USERPROFILE\Desktop\GESCODesktop.lnk" "Escritorio (sin espacios)"
Remove-IfExists "$env:PUBLIC\Desktop\GESCO Desktop.lnk" "Escritorio (todos los usuarios)"
Remove-IfExists "$env:PUBLIC\Desktop\GESCODesktop.lnk" "Escritorio público (sin espacios)"

# 3. Eliminar del menú inicio
Write-Host "`n3. Limpiando Menu Inicio..." -ForegroundColor White
Remove-IfExists "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\GESCO Desktop.lnk" "Menu Inicio (usuario)"
Remove-IfExists "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\GESCODesktop.lnk" "Menu Inicio (sin espacios)"
Remove-IfExists "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\GESCO Desktop.lnk" "Menu Inicio (todos)"
Remove-IfExists "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\GESCODesktop.lnk" "Menu Inicio público"

# 4. Limpiar carpetas del menú inicio
Write-Host "`n4. Limpiando carpetas del Menu Inicio..." -ForegroundColor White
$startMenuPaths = @(
    "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\GESCO Desktop",
    "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\GESCODesktop",
    "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\GESCO Desktop",
    "$env:ProgramData\Microsoft\Windows\Start Menu\Programs\GESCODesktop"
)

foreach ($path in $startMenuPaths) {
    Remove-IfExists $path "Carpeta: $path"
}

# 5. Limpiar instalaciones anteriores
Write-Host "`n5. Limpiando instalaciones anteriores..." -ForegroundColor White
$localAppData = [Environment]::GetFolderPath('LocalApplicationData')
$appData = [Environment]::GetFolderPath('ApplicationData')

Remove-IfExists "$localAppData\gesco-desktop" "AppData Local (gesco-desktop)"
Remove-IfExists "$localAppData\GESCODesktop" "AppData Local (GESCODesktop)"
Remove-IfExists "$localAppData\GESCO Desktop" "AppData Local (GESCO Desktop)"

# NO borrar datos de usuario
# Remove-IfExists "$appData\gesco-desktop" "AppData Roaming (gesco-desktop)"
# Remove-IfExists "$appData\GESCODesktop" "AppData Roaming (GESCODesktop)"

Write-Host "[INFO] Datos de usuario preservados en: $appData\GESCODesktop" -ForegroundColor Cyan

# 6. Limpiar Quick Launch
Write-Host "`n6. Limpiando Quick Launch..." -ForegroundColor White
$quickLaunchPath = "$env:APPDATA\Microsoft\Internet Explorer\Quick Launch"
Remove-IfExists "$quickLaunchPath\GESCO Desktop.lnk" "Quick Launch"
Remove-IfExists "$quickLaunchPath\GESCODesktop.lnk" "Quick Launch (sin espacios)"

# 7. LIMPIEZA EXHAUSTIVA DE CACHÉ DE ICONOS
Write-Host "`n7. Limpiando cache de iconos (EXHAUSTIVO)..." -ForegroundColor White

# Detener Explorer y procesos relacionados
Write-Host "   Deteniendo Explorer..." -ForegroundColor Gray
Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Limpiar todos los archivos de caché de iconos
Write-Host "   Limpiando archivos de cache..." -ForegroundColor Gray
Remove-Item -Path "$env:LOCALAPPDATA\IconCache.db" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\Windows\Explorer\iconcache*.db" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$env:LOCALAPPDATA\Microsoft\Windows\Explorer\thumbcache*.db" -Force -ErrorAction SilentlyContinue

# Limpiar caché adicional de Windows
Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\Windows\Explorer" -ErrorAction SilentlyContinue | 
    Where-Object { $_.Name -like "*cache*" } | 
    Remove-Item -Force -ErrorAction SilentlyContinue

# Ejecutar comando de limpieza de iconos de Windows
Write-Host "   Ejecutando ie4uinit..." -ForegroundColor Gray
ie4uinit.exe -show | Out-Null
Start-Sleep -Seconds 1
ie4uinit.exe -ClearIconCache | Out-Null

Write-Host "[OK] Cache de iconos limpiado" -ForegroundColor Green

# 8. Limpiar registro de Windows (opcional - requiere admin)
Write-Host "`n8. Limpiando entradas del registro..." -ForegroundColor White
try {
    # Limpiar MRU (Most Recently Used)
    Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.gesco" -Force -Recurse -ErrorAction SilentlyContinue
    Remove-Item -Path "HKCU:\Software\Classes\gesco-desktop" -Force -Recurse -ErrorAction SilentlyContinue
    Write-Host "[OK] Registro limpiado" -ForegroundColor Green
} catch {
    Write-Host "[!!] No se pudo limpiar el registro (puede requerir permisos admin)" -ForegroundColor Yellow
}

# 9. Reiniciar Explorer
Write-Host "`n9. Reiniciando Explorer..." -ForegroundColor White
Start-Sleep -Seconds 2
Start-Process explorer.exe
Start-Sleep -Seconds 3
Write-Host "[OK] Explorer reiniciado" -ForegroundColor Green

# Resumen
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host " LIMPIEZA COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "PASOS SIGUIENTES (IMPORTANTE):" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. REINICIA WINDOWS (obligatorio para que tome efecto)" -ForegroundColor Red
Write-Host ""
Write-Host "2. Despues de reiniciar, compila el ejecutable:" -ForegroundColor White
Write-Host "   npm run build:complete" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Instala y crea shortcuts manualmente:" -ForegroundColor White
Write-Host "   - Ejecuta el instalador .exe" -ForegroundColor White
Write-Host "   - NO uses el portable hasta verificar que el instalador funcione" -ForegroundColor White
Write-Host ""
Write-Host "4. Si el icono sigue mal en la taskbar:" -ForegroundColor White
Write-Host "   - Desancla de la barra de tareas (clic derecho)" -ForegroundColor White
Write-Host "   - Cierra la app completamente" -ForegroundColor White
Write-Host "   - Vuelve a abrir desde el menu inicio" -ForegroundColor White
Write-Host "   - Vuelve a anclar" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Enter para cerrar..." -ForegroundColor Gray
Read-Host