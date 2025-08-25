# GESCO Desktop

Sistema de Gestión de Eventos y Actividades Comunitarias - Versión Desktop

##  Características

-  **Modo Offline First**: Funciona sin conexión a internet
- ✅ **Base de datos local SQLite**: Almacenamiento local de datos
-  **Sincronización con servidor Laravel**: Cuando hay conexión disponible
-  **Autenticación con BCrypt**: Compatible con Laravel
-  **Activación de licencias**: Sistema de licencias para desktop
- ✅ **UI simple y eficiente**: HTML + Alpine.js

##  Estructura del Proyecto

```
GescoDesktop/
 src/
    Gesco.Desktop.Core/        # Lógica de negocio
    Gesco.Desktop.Data/        # Acceso a datos (SQLite)
   ─ Gesco.Desktop.Sync/        # Sincronización con Laravel
│   ├── Gesco.Desktop.Shared/      # DTOs y modelos compartidos
    Gesco.Desktop.UI/          # API mínima y UI web
 logs/                           # Archivos de log
 data/                           # Base de datos SQLite
 electron/                       # Aplicación Electron (futuro)
```

##  Tecnologías

- **.NET 8.0**
- **Entity Framework Core** con SQLite
- **BCrypt.Net** para encriptación
- **HTML + Alpine.js** para UI
- **Tailwind CSS** para estilos

##  Requisitos

- .NET 8.0 SDK o superior
- Visual Studio 2022 o VS Code
- Git

##  Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/tu-usuario/gesco-desktop.git
   cd gesco-desktop
   ```

2. **Configurar variables de entorno**
   ```bash
   cp .env.example .env
   # Editar .env con tus configuraciones
   ```

3. **Restaurar paquetes**
   ```bash
   dotnet restore
   ```

4. **Ejecutar la aplicación**
   ```bash
   .\start-dev.ps1
   ```

##  Uso

### Iniciar en modo desarrollo
```powershell
.\start-dev.ps1
```

### Compilar para producción
```powershell
.\build-release.ps1
```

### Crear una migración
```powershell
.\create-migration.ps1 -MigrationName "NombreMigracion"
```

##  Acceso a la Aplicación

- **Login**: http://localhost:5100/login.html
- **Dashboard**: http://localhost:5100/dashboard.html
- **Activación**: http://localhost:5100/activation.html
- **API Swagger**: http://localhost:5100/swagger

##  Credenciales por Defecto

- **Usuario**: admin
- **Contraseña**: admin123

##  Sincronización con Laravel

La aplicación funciona de manera offline y sincroniza con el servidor Laravel cuando hay conexión disponible.

### Configurar servidor Laravel

En tu archivo `.env`:

```env
LARAVEL_API_URL=https://tu-servidor.com/api
LARAVEL_API_KEY=tu-api-key
```

##  Licencias

El sistema requiere activación de licencia para funcionar. Las licencias se validan contra el servidor Laravel.

## � Solución de Problemas

### La base de datos no se crea
```powershell
# Crear manualmente la migración inicial
cd src\Gesco.Desktop.Data
dotnet ef database update -s ..\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj
```

### Error de puerto en uso
Cambiar el puerto en `.env`:
```env
LOCAL_API_PORT=5101
```

##  Documentación Adicional

- [Documentación de Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Alpine.js Documentation](https://alpinejs.dev/)
- [Tailwind CSS](https://tailwindcss.com/)

## 🤝 Contribuir

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo licencia MIT. Ver el archivo `LICENSE` para más detalles.

##  Contacto

Tu Nombre - [@tutwitter](https://twitter.com/tutwitter) - email@example.com

Link del Proyecto: [https://github.com/tu-usuario/gesco-desktop](https://github.com/tu-usuario/gesco-desktop)
