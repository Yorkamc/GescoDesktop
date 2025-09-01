# Build script optimizado para GESCO Desktop
param(
    [string]$Configuration = "Release",
    [switch]$SkipTests = $false,
    [switch]$BuildElectron = $false,
    [switch]$Clean = $false
)

Write-Host "Iniciando build de GESCO Desktop..." -ForegroundColor Green

$BackendProject = "backend/src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj"
$FrontendPath = "frontend"
$OutputPath = "dist"

# Limpiar si se solicita
if ($Clean) {
    Write-Host "Limpiando builds anteriores..." -ForegroundColor Yellow
    @("dist", "$FrontendPath/dist", "$FrontendPath/dist-electron") | ForEach-Object {
        if (Test-Path $_) { Remove-Item $_ -Recurse -Force }
    }
}

# Crear directorios
@($OutputPath, "$OutputPath/backend", "$OutputPath/frontend") | ForEach-Object {
    New-Item -ItemType Directory -Path $_ -Force | Out-Null
}

# Verificar dependencias
try {
    $dotnetVersion = dotnet --version
    Write-Host "Usando .NET SDK: $dotnetVersion" -ForegroundColor Cyan
} catch {
    Write-Host "Error: .NET SDK no encontrado" -ForegroundColor Red
    exit 1
}

# Build Backend
Write-Host "Building Backend (.NET)..." -ForegroundColor Green
Set-Location "backend"

dotnet restore --verbosity quiet | Out-Null
if ($LASTEXITCODE -ne 0) { 
    Write-Host "Error restaurando paquetes" -ForegroundColor Red
    exit 1 
}

if (-not $SkipTests) {
    dotnet test --verbosity quiet --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        $continue = Read-Host "Tests fallaron. Continuar? (y/n)"
        if ($continue -ne 'y') { exit 1 }
    }
}

dotnet publish $BackendProject -c $Configuration -o "../$OutputPath/backend" --verbosity quiet --no-restore | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error compilando backend" -ForegroundColor Red
    exit 1
}

Set-Location ".."
Write-Host "Backend compilado exitosamente" -ForegroundColor Green

# Build Frontend
Write-Host "Building Frontend (React)..." -ForegroundColor Green
Set-Location $FrontendPath

if (-not (Test-Path "node_modules")) {
    npm install | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error instalando dependencias" -ForegroundColor Red
        exit 1
    }
}

$env:NODE_ENV = "production"
npm run build | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error compilando frontend" -ForegroundColor Red
    exit 1
}

if (Test-Path "dist") {
    Copy-Item -Path "dist/*" -Destination "../$OutputPath/frontend" -Recurse -Force
    Write-Host "Frontend compilado exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error: directorio dist no encontrado" -ForegroundColor Red
    exit 1
}

# Build Electron (opcional)
if ($BuildElectron) {
    Write-Host "Building aplicación Electron..." -ForegroundColor Green
    npm run electron:build | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Electron compilado: $FrontendPath/dist-electron/" -ForegroundColor Cyan
    }
}

Set-Location ".."

# Resumen
Write-Host "Build completado exitosamente" -ForegroundColor Green
Write-Host "Output: $OutputPath/" -ForegroundColor Cyan

$buildInfo = @{
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    configuration = $Configuration
    dotnetVersion = $dotnetVersion
    electronBuild = $BuildElectron
} | ConvertTo-Json

$buildInfo | Out-File -FilePath "$OutputPath/build-info.json" -Encoding UTF8

Write-Host "Para desplegar: subir contenido de '$OutputPath/' al servidor" -ForegroundColor Yellow