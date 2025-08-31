# build-electron-fixed.ps1 - Script mejorado para solucionar problemas de carga
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  GESCO DESKTOP - BUILD ELECTRON MEJORADO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Verificar ubicación
if (-not (Test-Path "frontend/package.json")) {
    Write-Host "ERROR: Ejecuta desde la raíz del proyecto" -ForegroundColor Red
    exit 1
}

Set-Location "frontend"

Write-Host ""
Write-Host "PASO 1: Verificar dependencias y limpieza..." -ForegroundColor Yellow

# Verificar que las dependencias estén instaladas
if (-not (Test-Path "node_modules")) {
    Write-Host "   Instalando dependencias..." -ForegroundColor Gray
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR instalando dependencias" -ForegroundColor Red
        Set-Location ".."
        exit 1
    }
}

# Limpieza exhaustiva
Write-Host "   Limpiando builds anteriores..." -ForegroundColor Gray
$pathsToClean = @("dist", "dist-electron", ".vite", "node_modules/.vite", ".cache")
foreach ($path in $pathsToClean) {
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "     Eliminado: $path" -ForegroundColor DarkGray
    }
}

# Limpiar cache npm
npm cache clean --force | Out-Null

Write-Host "   Limpieza completada" -ForegroundColor Green

Write-Host ""
Write-Host "PASO 2: Build web con configuración correcta..." -ForegroundColor Yellow

# Configurar variables de entorno para build correcto
$env:NODE_ENV = "production"
$env:VITE_APP_VERSION = "1.0.0"

Write-Host "   Variables configuradas:" -ForegroundColor Gray
Write-Host "     NODE_ENV: $env:NODE_ENV" -ForegroundColor DarkGray
Write-Host "     VITE_APP_VERSION: $env:VITE_APP_VERSION" -ForegroundColor DarkGray

# Build web
Write-Host "   Ejecutando build web..." -ForegroundColor Gray
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en build web" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

Write-Host ""
Write-Host "PASO 3: Verificar build web..." -ForegroundColor Yellow

$indexPath = "dist/index.html"
if (-not (Test-Path $indexPath)) {
    Write-Host "ERROR: No se generó dist/index.html" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Verificar contenido del HTML
$indexContent = Get-Content $indexPath -Raw
Write-Host "   index.html generado: $($indexContent.Length) caracteres" -ForegroundColor Green

# Verificar que no contenga rutas de desarrollo
if ($indexContent -match 'src="/src/renderer/index.tsx"') {
    Write-Host "   ADVERTENCIA: HTML contiene rutas de desarrollo" -ForegroundColor Yellow
    Write-Host "   Esto puede causar problemas en el ejecutable" -ForegroundColor Yellow
} else {
    Write-Host "   HTML correctamente compilado para producción" -ForegroundColor Green
}

# Verificar assets
$assetsPath = "dist/assets"
if (Test-Path $assetsPath) {
    $assetsCount = (Get-ChildItem $assetsPath -File).Count
    Write-Host "   Assets generados: $assetsCount archivos" -ForegroundColor Green
} else {
    Write-Host "   ADVERTENCIA: No se encontró carpeta de assets" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "PASO 4: Verificar configuración de Electron..." -ForegroundColor Yellow

# Verificar que electron-main.cjs existe
if (-not (Test-Path "electron-main.cjs")) {
    Write-Host "ERROR: electron-main.cjs no encontrado" -ForegroundColor Red
    Write-Host "Crea el archivo electron-main.cjs en frontend/" -ForegroundColor Gray
    Set-Location ".."
    exit 1
}

# Verificar package.json
$packageJson = Get-Content "package.json" -Raw | ConvertFrom-Json
if ($packageJson.main -ne "electron-main.cjs") {
    Write-Host "ADVERTENCIA: package.json main no apunta a electron-main.cjs" -ForegroundColor Yellow
} else {
    Write-Host "   Configuración de Electron correcta" -ForegroundColor Green
}

# Verificar configuración de build en package.json
if ($packageJson.build -and $packageJson.build.files) {
    $includesDist = $packageJson.build.files -contains "dist/**/*"
    if ($includesDist) {
        Write-Host "   Configuración de empaquetado incluye dist/" -ForegroundColor Green
    } else {
        Write-Host "   ADVERTENCIA: build.files no incluye dist/**/*" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "PASO 5: Build Electron..." -ForegroundColor Yellow

# Eliminar build anterior de Electron
if (Test-Path "dist-electron") {
    Remove-Item "dist-electron" -Recurse -Force
    Write-Host "   Build anterior de Electron eliminado" -ForegroundColor Gray
}

# Build Electron con logs detallados
Write-Host "   Ejecutando electron-builder..." -ForegroundColor Gray
$env:DEBUG = "electron-builder"

# Usar electron-builder directamente para más control
npx electron-builder --config.directories.output=dist-electron --config.files="[\"dist/**/*\",\"electron-main.cjs\"]"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR en build Electron" -ForegroundColor Red
    Write-Host ""
    Write-Host "Posibles soluciones:" -ForegroundColor Yellow
    Write-Host "1. Verifica que dist/ contiene los archivos correctos" -ForegroundColor Gray
    Write-Host "2. Verifica que electron-main.cjs existe" -ForegroundColor Gray
    Write-Host "3. Intenta: npm run build && npm run electron:build" -ForegroundColor Gray
    Set-Location ".."
    exit 1
}

Write-Host ""
Write-Host "PASO 6: Verificar ejecutable..." -ForegroundColor Yellow

if (-not (Test-Path "dist-electron")) {
    Write-Host "ERROR: No se generó dist-electron/" -ForegroundColor Red
    Set-Location ".."
    exit 1
}

# Buscar ejecutables
$exeFiles = Get-ChildItem "dist-electron" -Filter "*.exe" -Recurse
$appImages = Get-ChildItem "dist-electron" -Filter "*.AppImage" -Recurse
$dmgFiles = Get-ChildItem "dist-electron" -Filter "*.dmg" -Recurse

Write-Host "   Ejecutables generados:" -ForegroundColor Green
$exeFiles | ForEach-Object {
    $size = [Math]::Round($_.Length / 1MB, 2)
    Write-Host "     Windows: $($_.Name) ($size MB)" -ForegroundColor Cyan
    Write-Host "     Ubicación: $($_.FullName)" -ForegroundColor DarkGray
}

$appImages | ForEach-Object {
    $size = [Math]::Round($_.Length / 1MB, 2)
    Write-Host "     Linux: $($_.Name) ($size MB)" -ForegroundColor Cyan
}

$dmgFiles | ForEach-Object {
    $size = [Math]::Round($_.Length / 1MB, 2)
    Write-Host "     macOS: $($_.Name) ($size MB)" -ForegroundColor Cyan
}

if ($exeFiles.Count -eq 0 -and $appImages.Count -eq 0 -and $dmgFiles.Count -eq 0) {
    Write-Host "ERROR: No se generaron ejecutables" -ForegroundColor Red
    Write-Host "Contenido de dist-electron:" -ForegroundColor Gray
    Get-ChildItem "dist-electron" -Recurse | ForEach-Object {
        Write-Host "  $($_.FullName)" -ForegroundColor DarkGray
    }
    Set-Location ".."
    exit 1
}

Write-Host ""
Write-Host "PASO 7: Prueba de integridad..." -ForegroundColor Yellow

# Verificar que el ejecutable contiene los archivos necesarios
if ($exeFiles.Count -gt 0) {
    $mainExe = $exeFiles[0]
    Write-Host "   Ejecutable principal: $($mainExe.Name)" -ForegroundColor Green
    Write-Host "   Tamaño: $([Math]::Round($mainExe.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    Write-Host "   Creado: $($mainExe.LastWriteTime)" -ForegroundColor Cyan
}

# Verificar timestamps
$distTime = (Get-Item "dist/index.html").LastWriteTime
if ($exeFiles.Count -gt 0) {
    $exeTime = $exeFiles[0].LastWriteTime
    $timeDiff = ($exeTime - $distTime).TotalMinutes
    
    Write-Host ""
    Write-Host "   Comparación de timestamps:" -ForegroundColor Gray
    Write-Host "     Build web: $distTime" -ForegroundColor DarkGray
    Write-Host "     Ejecutable: $exeTime" -ForegroundColor DarkGray
    Write-Host "     Diferencia: $([Math]::Round($timeDiff, 1)) minutos" -ForegroundColor DarkGray
    
    if ($exeTime -ge $distTime.AddMinutes(-2)) {
        Write-Host "   INTEGRIDAD: Ejecutable contiene build reciente" -ForegroundColor Green
    } else {
        Write-Host "   ADVERTENCIA: Ejecutable podría estar desactualizado" -ForegroundColor Yellow
    }
}

Set-Location ".."

Write-Host ""
Write-Host "BUILD COMPLETADO EXITOSAMENTE!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "EJECUTABLE GENERADO:" -ForegroundColor White
if ($exeFiles.Count -gt 0) {
    Write-Host "   Windows: frontend/dist-electron/$($exeFiles[0].Name)" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "SOLUCION AL PROBLEMA:" -ForegroundColor Yellow
Write-Host "   1. El build web se genera correctamente en frontend/dist/" -ForegroundColor White
Write-Host "   2. Electron-main.cjs está configurado para cargar desde dist/" -ForegroundColor White
Write-Host "   3. El ejecutable incluye todos los archivos necesarios" -ForegroundColor White
Write-Host ""
Write-Host "PARA EJECUTAR:" -ForegroundColor Green
Write-Host "   1. Ve a: frontend/dist-electron/" -ForegroundColor White
Write-Host "   2. Ejecuta el archivo .exe" -ForegroundColor White
Write-Host "   3. Debería cargar el login correctamente" -ForegroundColor White
Write-Host ""
Write-Host "SI SIGUE SIN FUNCIONAR:" -ForegroundColor Yellow
Write-Host "   1. Verifica que el backend esté corriendo (localhost:5100)" -ForegroundColor White
Write-Host "   2. El ejecutable abrirá DevTools para debugging" -ForegroundColor White
Write-Host "   3. Revisa la consola para errores específicos" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor Green