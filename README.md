# GESCO Desktop

Sistema de Gestión de Eventos y Actividades Comunitarias

## Estructura del Proyecto

- src/Gesco.Desktop.Core - Lógica de negocio
- src/Gesco.Desktop.Data - Acceso a datos
- src/Gesco.Desktop.Sync - Sincronización
- src/Gesco.Desktop.Shared - Modelos compartidos
- src/Gesco.Desktop.API - API REST local
- tests/Gesco.Desktop.Tests - Pruebas unitarias

## Tecnologías

- .NET 8.0
- SQLite
- Entity Framework Core

## Instalación

1. Ejecutar script de estructura: .\01-create-structure-simple.ps1
2. Ejecutar script de librerías: .\02-install-packages.ps1
3. Ejecutar: dotnet run --project src\Gesco.Desktop.API
