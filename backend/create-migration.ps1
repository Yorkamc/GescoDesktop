# Script para crear migraciones de Entity Framework
param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "Creando migración: $MigrationName" -ForegroundColor Yellow

Set-Location src\Gesco.Desktop.Data
dotnet ef migrations add $MigrationName -s ..\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj
Set-Location ..\..

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Migración creada exitosamente" -ForegroundColor Green
} else {
    Write-Host " Error al crear la migración" -ForegroundColor Red
}
