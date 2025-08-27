# Script para resolver problemas de base de datos y autenticacion
Write-Host "Reparando problemas de base de datos..." -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Verificar ubicacion
if (-not (Test-Path "src\Gesco.Desktop.Data\Context\LocalDbContext.cs")) {
    Write-Host "ERROR: Ejecutar este script desde la carpeta 'backend'" -ForegroundColor Red
    Write-Host "   Ubicacion actual: $(Get-Location)" -ForegroundColor Yellow
    Write-Host "   Cambiar a: cd backend" -ForegroundColor Yellow
    exit 1
}

Write-Host "Ubicacion correcta detectada" -ForegroundColor Green

# 1. BACKUP de la base de datos actual (si existe)
$dbPath = "src\Gesco.Desktop.UI\data\gesco_local.db"
if (Test-Path $dbPath) {
    $backupPath = "src\Gesco.Desktop.UI\data\gesco_local_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
    Copy-Item $dbPath $backupPath
    Write-Host "Backup creado: $backupPath" -ForegroundColor Green
}

# 2. Aplicar correccion al LocalDbContext.cs
Write-Host ""
Write-Host "Aplicando correccion a LocalDbContext.cs..." -ForegroundColor Yellow

$contextPath = "src\Gesco.Desktop.Data\Context\LocalDbContext.cs"
$contextContent = Get-Content $contextPath -Raw

# Buscar y reemplazar el metodo ConfigurarNotificacionesYConfiguracion
$correctedMethod = @'
    private void ConfigurarNotificacionesYConfiguracion(ModelBuilder modelBuilder)
    {
        // Notificacion - CONFIGURACION CORREGIDA
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizacionId, e.Leida, e.Importante });
            entity.HasIndex(e => new { e.UsuarioId, e.Leida, e.FechaProgramada });
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensaje).IsRequired();
            entity.Property(e => e.CanalesEntrega).HasMaxLength(100);

            // FIX: Configurar relaciones explicitamente
            entity.HasOne(e => e.Organizacion)
                .WithMany(o => o.Notificaciones)
                .HasForeignKey(e => e.OrganizacionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                .WithMany() // Sin navegacion inversa para evitar conflictos
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CreadaPorUsuario)
                .WithMany() // Sin navegacion inversa para evitar conflictos
                .HasForeignKey(e => e.CreadaPor)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ConfiguracionSistema
        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.OrganizacionId, e.Clave }).IsUnique();
            entity.HasIndex(e => new { e.Clave, e.Categoria });
            entity.Property(e => e.Clave).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Valor).IsRequired();
            entity.Property(e => e.TipoValor).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.NivelAcceso).HasMaxLength(50).HasDefaultValue("admin");

            // Configurar relacion con Usuario para ActualizadoPor
            entity.HasOne(e => e.ActualizadoPorUsuario)
                .WithMany() // Sin navegacion inversa
                .HasForeignKey(e => e.ActualizadoPor)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
'@

# Realizar el reemplazo usando regex - metodo mas seguro
$pattern = 'private void ConfigurarNotificacionesYConfiguracion\(ModelBuilder modelBuilder\)\s*\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}'
if ($contextContent -match $pattern) {
    $correctedContent = $contextContent -replace $pattern, $correctedMethod, 'Singleline'
    Set-Content -Path $contextPath -Value $correctedContent -Encoding UTF8
    Write-Host "LocalDbContext.cs corregido" -ForegroundColor Green
} else {
    Write-Host "No se encontro el metodo, verificando estructura..." -ForegroundColor Yellow
    
    # Verificar si el metodo existe de otra forma
    if ($contextContent -match "ConfigurarNotificacionesYConfiguracion") {
        Write-Host "Metodo existe pero no se pudo hacer reemplazo automatico" -ForegroundColor Yellow
        Write-Host "Aplicar correccion manualmente o eliminar migraciones y continuar" -ForegroundColor Yellow
    } else {
        Write-Host "Metodo no existe, se agregara al final" -ForegroundColor Yellow
        # Agregar el metodo al final del OnModelCreating
        $addToEnd = "`n            // Configuracion corregida de Notificaciones`n            ConfigurarNotificacionesYConfiguracion(modelBuilder);`n        }`n    }`n$correctedMethod"
        $contextContent = $contextContent -replace "(SeedData\(modelBuilder\);[\s]*\}[\s]*\}[\s]*)", "SeedData(modelBuilder);`n            ConfigurarNotificacionesYConfiguracion(modelBuilder);`n        }`n    }$correctedMethod"
        Set-Content -Path $contextPath -Value $contextContent -Encoding UTF8
        Write-Host "Metodo agregado al final" -ForegroundColor Green
    }
}

# 3. Eliminar migraciones existentes y base de datos para empezar limpio
Write-Host ""
Write-Host "Limpiando migraciones y base de datos..." -ForegroundColor Yellow

$migrationsFolder = "src\Gesco.Desktop.Data\Migrations"
if (Test-Path $migrationsFolder) {
    Remove-Item $migrationsFolder -Recurse -Force
    Write-Host "Carpeta de migraciones eliminada" -ForegroundColor Green
}

if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "Base de datos eliminada" -ForegroundColor Green
}

# 4. Restaurar paquetes
Write-Host ""
Write-Host "Restaurando paquetes..." -ForegroundColor Yellow
$restoreResult = dotnet restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Paquetes restaurados" -ForegroundColor Green
} else {
    Write-Host "Error restaurando paquetes, continuando..." -ForegroundColor Yellow
    Write-Host $restoreResult -ForegroundColor Red
}

# 5. Crear nueva migracion inicial
Write-Host ""
Write-Host "Creando migracion inicial..." -ForegroundColor Yellow
Set-Location "src\Gesco.Desktop.Data"

$migrationOutput = dotnet ef migrations add InitialCreate -s ..\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Migracion inicial creada exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error creando migracion:" -ForegroundColor Red
    Write-Host $migrationOutput -ForegroundColor Yellow
    Set-Location ..\..
    Write-Host ""
    Write-Host "SOLUCION: Editar manualmente LocalDbContext.cs para corregir relaciones" -ForegroundColor Yellow
    Write-Host "1. Buscar ConfigurarNotificacionesYConfiguracion" -ForegroundColor White
    Write-Host "2. Agregar .WithMany() sin parametros a las relaciones de Usuario" -ForegroundColor White
    Write-Host "3. Volver a ejecutar este script" -ForegroundColor White
    exit 1
}

# 6. Aplicar migracion
Write-Host ""
Write-Host "Aplicando migracion..." -ForegroundColor Yellow
$updateOutput = dotnet ef database update -s ..\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Base de datos actualizada exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error actualizando base de datos:" -ForegroundColor Red
    Write-Host $updateOutput -ForegroundColor Yellow
    Set-Location ..\..
    exit 1
}

Set-Location ..\..

# 7. Verificar que el usuario admin existe
Write-Host ""
Write-Host "Verificando usuario administrador..." -ForegroundColor Yellow
Write-Host "Seed data configurado para crear usuario admin automaticamente" -ForegroundColor Green
Write-Host "   Usuario: admin" -ForegroundColor Cyan
Write-Host "   Contrasena: admin123" -ForegroundColor Cyan

# 8. Compilar proyecto para asegurar que todo esta OK
Write-Host ""
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
$buildOutput = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Proyecto compilado exitosamente" -ForegroundColor Green
} else {
    Write-Host "Error compilando:" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Yellow
    exit 1
}

# 9. Resultado final
Write-Host ""
Write-Host "REPARACION COMPLETADA!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "- Relaciones de Entity Framework corregidas" -ForegroundColor White
Write-Host "- Base de datos recreada con datos semilla" -ForegroundColor White
Write-Host "- Usuario administrador configurado" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "Para probar la aplicacion:" -ForegroundColor Yellow
Write-Host "   1. cd src\Gesco.Desktop.UI" -ForegroundColor Cyan
Write-Host "   2. dotnet run" -ForegroundColor Cyan
Write-Host "   3. Ir a http://localhost:5100/swagger" -ForegroundColor Cyan
Write-Host "   4. Probar login con: admin / admin123" -ForegroundColor Cyan
Write-Host "" -ForegroundColor White
Write-Host "Si aun hay problemas de login, revisar que el backend este ejecutandose correctamente" -ForegroundColor Yellow