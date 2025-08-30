# 🚀 GESCO Desktop

**Sistema de Gestión de Eventos y Actividades Comunitarias - Versión Desktop**

---

## ✨ Características

- 🔌 **Arquitectura híbrida**
  - Backend en **ASP.NET Core + Entity Framework Core + SQLite**
  - Frontend en **React + TypeScript + Electron**

- 📦 **Modo Offline First**
  - Base de datos local SQLite
  - Funciona sin conexión a internet

- 🔄 **Sincronización con Laravel**
  - Cuando hay conexión, los datos se sincronizan automáticamente con el servidor

- 🔐 **Seguridad**
  - Autenticación con **BCrypt**
  - Sistema de **activación de licencias**

- 🎨 **Interfaz moderna**
  - React + Tailwind CSS
  - UI rápida y responsiva

---

## 📂 Estructura del Proyecto

GescoDesktop/
│
├── backend/ # Backend en .NET Core (C#)
│ ├── src/
│ │ ├── Gesco.Desktop.Core/ # Lógica de negocio
│ │ ├── Gesco.Desktop.Data/ # Acceso a datos (SQLite + EF Core)
│ │ ├── Gesco.Desktop.Shared/ # Modelos y DTOs compartidos
│ │ ├── Gesco.Desktop.Sync/ # Sincronización con Laravel
│ │ └── Gesco.Desktop.UI/ # API mínima con ASP.NET Core
│ ├── utils/ # Utilidades
│ └── tests/ # Pruebas unitarias
│
├── frontend/ # Frontend React + TS + Electron
│ ├── public/ # Archivos estáticos
│ ├── src/
│ │ ├── main/ # Proceso principal de Electron
│ │ ├── renderer/ # React UI (pantallas)
│ │ └── shared/ # Código compartido entre procesos
│ ├── electron-main.cjs # Entry point de Electron
│ ├── index.html # Página base
│ ├── package.json
│ └── tailwind.config.js
│
├── .env / .env.example # Variables de entorno
├── .prettierrc # Configuración de estilos
├── GescoDesktop.sln # Solución .NET
├── start-dev.ps1 # Script arranque dev
├── build-prod.ps1 # Script build producción
├── build-release.ps1 # Script release
├── create-migration.ps1 # Script para EF migrations
└── LICENSE # Licencia MIT

yaml
Copiar código

---

## 🛠️ Tecnologías

**Backend:**  
- .NET 8.0  
- Entity Framework Core  
- SQLite  
- BCrypt.Net  

**Frontend:**  
- React  
- TypeScript  
- Electron  
- TailwindCSS  

**Otros:**  
- Laravel API (para sincronización)

---

## ⚙️ Requisitos

- .NET 8.0 SDK o superior  
- Node.js 18+ y npm  
- Visual Studio 2022 o VS Code  
- Git  

---

## 🚀 Instalación y Uso

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/tu-usuario/gesco-desktop.git
   cd gesco-desktop
Configurar variables de entorno

bash
Copiar código
cp .env.example .env
# Editar .env con tus configuraciones
Backend: Restaurar paquetes

bash
Copiar código
dotnet restore
Frontend: Instalar dependencias

bash
Copiar código
cd frontend
npm install
Iniciar en modo desarrollo

powershell
Copiar código
.\start-dev.ps1
Compilar para producción

powershell
Copiar código
.\build-release.ps1
🧪 Migraciones de Base de Datos
Crear una nueva migración:

powershell
Copiar código
.\create-migration.ps1 -MigrationName "NombreMigracion"
Actualizar la base de datos:

bash
Copiar código
cd backend/src/Gesco.Desktop.Data
dotnet ef database update -s ../Gesco.Desktop.UI/Gesco.Desktop.UI.csproj
🌐 Acceso a la Aplicación
Login: http://localhost:5100/login

Dashboard: http://localhost:5100/dashboard

Activación: http://localhost:5100/activation

API Swagger: http://localhost:5100/swagger

🔄 Sincronización con Laravel
En tu archivo .env configurar:

env
Copiar código
LARAVEL_API_URL=https://tu-servidor.com/api
LARAVEL_API_KEY=tu-api-key
🐞 Solución de Problemas
La base de datos no se crea

bash
Copiar código
dotnet ef database update
Error de puerto en uso
Cambiar el puerto en .env:

env
Copiar código
LOCAL_API_PORT=5101
🤝 Contribuir
Haz un fork del proyecto

Crea una rama:

bash
Copiar código
git checkout -b feature/NuevaFeature
Commit tus cambios:

bash
Copiar código
git commit -m 'Agrega nueva feature'
Push a tu rama:

bash
Copiar código
git push origin feature/NuevaFeature
Abre un Pull Request 🚀

📄 Licencia
Este proyecto está bajo licencia MIT.
Consulta el archivo LICENSE para más detalles.

📬 Contacto
👤 Yorkael Mc Carthey Sánchez
📧 Email: hernaldomccarthy14@gmail.com
🔗 Proyecto: GESCO Desktop en GitHub