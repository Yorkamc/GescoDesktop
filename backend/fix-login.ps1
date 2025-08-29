# Script para arreglar el problema de login
Write-Host "🔧 REPARANDO PROBLEMA DE LOGIN" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Verificar que estamos en el directorio correcto
if (-not (Test-Path "src\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj")) {
    Write-Host "❌ Error: Ejecuta este script desde el directorio 'backend'" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Pasos a seguir:" -ForegroundColor Yellow
Write-Host "1. Verificar hash de contraseña actual" -ForegroundColor White
Write-Host "2. Generar nuevo hash correcto" -ForegroundColor White
Write-Host "3. Actualizar base de datos" -ForegroundColor White
Write-Host ""

# Paso 1: Crear herramienta temporal para generar hash
Write-Host "🔨 Creando herramienta de hash..." -ForegroundColor Yellow

# Crear directorio temporal
if (-not (Test-Path "temp")) {
    New-Item -ItemType Directory -Path "temp" | Out-Null
}

# Crear archivo C# para generar hash
$toolContent = @'
using System;
using BCrypt.Net;

namespace HashTool 
{
    class Program
    {
        static void Main(string[] args)
        {
            string password = "admin123";
            
            // Generar hash correcto
            string correctHash = BCrypt.HashPassword(password, BCrypt.GenerateSalt(12));
            
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Correct Hash: {correctHash}");
            
            // Verificar que funciona
            bool isValid = BCrypt.Verify(password, correctHash);
            Console.WriteLine($"Hash Verification: {isValid}");
            
            // Probar hash actual (problemático)
            string currentHash = "$2a$11$7qOV8ZE8hVjlNBHjw5GdAeO5LZvWHOPz8t2OYH.uZT8lGJeUpBJ5O";
            try {
                bool currentValid = BCrypt.Verify(password, currentHash);
                Console.WriteLine($"Current Hash Valid: {currentValid}");
            } catch (Exception ex) {
                Console.WriteLine($"Current Hash Error: {ex.Message}");
            }
        }
    }
}
'@

$toolContent | Out-File -FilePath "temp\HashTool.cs" -Encoding UTF8

# Crear proyecto temporal
$projectContent = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  </ItemGroup>
</Project>
'@

$projectContent | Out-File -FilePath "temp\HashTool.csproj" -Encoding UTF8

# Ejecutar herramienta
Write-Host "🔐 Generando hash correcto..." -ForegroundColor Yellow
Set-Location temp

try {
    dotnet run --verbosity quiet 2>$null | Tee-Object -Variable output
    
    # Extraer el hash correcto del output
    $correctHashLine = $output | Where-Object { $_ -match "Correct Hash: " }
    if ($correctHashLine) {
        $correctHash = ($correctHashLine -split "Correct Hash: ")[1]
        Write-Host "✅ Hash generado: $correctHash" -ForegroundColor Green
        
        Set-Location ..
        
        # Paso 2: Actualizar LocalDbContext.cs con el hash correcto
        Write-Host "📝 Actualizando LocalDbContext.cs..." -ForegroundColor Yellow
        
        $contextPath = "src\Gesco.Desktop.Data\Context\LocalDbContext.cs"
        $contextContent = Get-Content $contextPath -Raw
        
        # Reemplazar el hash problemático
        $oldHash = '$2a$11$7qOV8ZE8hVjlNBHjw5GdAeO5LZvWHOPz8t2OYH.uZT8lGJeUpBJ5O'
        $updatedContent = $contextContent -replace [regex]::Escape($oldHash), $correctHash
        
        $updatedContent | Out-File -FilePath $contextPath -Encoding UTF8
        
        Write-Host "✅ LocalDbContext.cs actualizado" -ForegroundColor Green
        
        # Paso 3: Eliminar base de datos existente para forzar recreación
        Write-Host "🗃️ Eliminando base de datos existente..." -ForegroundColor Yellow
        
        $dbPath = "src\Gesco.Desktop.UI\data\gesco_local.db"
        if (Test-Path $dbPath) {
            Remove-Item $dbPath -Force
            Write-Host "✅ Base de datos eliminada" -ForegroundColor Green
        } else {
            Write-Host "ℹ️ Base de datos no encontrada (OK)" -ForegroundColor Blue
        }
        
        # Paso 4: Recompilar proyecto
        Write-Host "🔨 Recompilando proyecto..." -ForegroundColor Yellow
        dotnet build --configuration Debug --verbosity quiet
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Proyecto recompilado exitosamente" -ForegroundColor Green
            
            Write-Host ""
            Write-Host "🎉 REPARACIÓN COMPLETADA" -ForegroundColor Green
            Write-Host "=========================" -ForegroundColor Green
            Write-Host "✅ Nuevo hash de contraseña generado" -ForegroundColor White
            Write-Host "✅ LocalDbContext.cs actualizado" -ForegroundColor White
            Write-Host "✅ Base de datos eliminada (se recreará)" -ForegroundColor White
            Write-Host "✅ Proyecto recompilado" -ForegroundColor White
            Write-Host ""
            Write-Host "🚀 Ahora puedes ejecutar:" -ForegroundColor Yellow
            Write-Host "   .\start-dev.ps1" -ForegroundColor White
            Write-Host ""
            Write-Host "🔑 Credenciales:" -ForegroundColor Yellow
            Write-Host "   Usuario: admin" -ForegroundColor White
            Write-Host "   Password: admin123" -ForegroundColor White
        } else {
            Write-Host "❌ Error al recompilar proyecto" -ForegroundColor Red
        }
        
    } else {
        Write-Host "❌ Error: No se pudo generar el hash" -ForegroundColor Red
        Set-Location ..
    }
} catch {
    Write-Host "❌ Error ejecutando herramienta de hash: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location ..
}

# Limpiar archivos temporales
if (Test-Path "temp") {
    Remove-Item -Path "temp" -Recurse -Force
}

Write-Host ""
Write-Host "🧹 Archivos temporales eliminados" -ForegroundColor Gray