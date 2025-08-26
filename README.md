GESCO Desktop

Sistema de Gestión de Eventos y Actividades Comunitarias - Versión Desktop

✨ Características

🔌 Arquitectura híbrida:

Backend en ASP.NET Core con Entity Framework Core + SQLite.

Frontend en React + TypeScript + Electron.

📦 Modo Offline First:

Base de datos local SQLite.

Funciona sin conexión a internet.

🔄 Sincronización con Laravel:

Cuando hay conexión, los datos se sincronizan automáticamente con el servidor Laravel.

🔐 Seguridad:

Autenticación con BCrypt.

Sistema de activación de licencias.

🎨 Interfaz moderna:

React + Tailwind CSS.

UI rápida y responsiva.

📂 Estructura del Proyecto
GescoDesktop/
│
├── backend/                     # Backend en .NET Core (C#)
│   |                 
│   ├── src/
│   │   ├── Gesco.Desktop.Core/   # Lógica de negocio
│   │   ├── Gesco.Desktop.Data/   # Acceso a datos (SQLite + EF Core)
│   │   ├── Gesco.Desktop.Shared/ # Modelos y DTOs compartidos
│   │   ├── Gesco.Desktop.Sync/   # Sincronización con Laravel
│   │   └── Gesco.Desktop.UI/     # API mínima con ASP.NET Core
│   ├── utils/                    # Utilidades
│   └── tests/                    # Pruebas unitarias
│
├── frontend/                     # Frontend React + TS + Electron
│   ├── public/                   # Archivos estáticos
│   ├── src/
│   │   ├── main/                 # Proceso principal de Electron
│   │   ├── renderer/             # React UI (pantallas)
│   │   └── shared/               # Código compartido entre procesos
│   ├── electron-main.cjs         # Entry point de Electron
│   ├── index.html                # Página base
│   ├── package.json
│   ├── package-lock.json
│   ├── postcss.config.js
│   └── tailwind.config.js
│
├── .env                          # Variables de entorno
├── .env.example
├── .prettierrc                   # Configuración de estilos
├── build-prod.ps1                 # Script build producción
├── build-release.ps1              # Script release
├── create-migration.ps1           # Script para EF migrations
├── start-dev.ps1                  # Script arranque dev
├── GescoDesktop.sln               # Solución .NET
└── LICENSE                        # Licencia MIT

🛠️ Tecnologías

Backend: .NET 8.0, Entity Framework Core, SQLite, BCrypt.Net

Frontend: React, TypeScript, Electron, TailwindCSS

Otros:  Laravel API (para sincronización)

⚙️ Requisitos

.NET 8.0 SDK o superior

Node.js 18+ y npm

Visual Studio 2022 o VS Code

Git

🚀 Instalación y Uso

Clonar el repositorio

git clone https://github.com/tu-usuario/gesco-desktop.git
cd gesco-desktop


Configurar variables de entorno

cp .env.example .env
# Editar .env con tus configuraciones


Backend: Restaurar paquetes

dotnet restore


Frontend: Instalar dependencias

cd frontend
npm install


Iniciar en modo desarrollo

.\start-dev.ps1


Compilar para producción

.\build-release.ps1

🧪 Migraciones de Base de Datos

Crear una nueva migración:

.\create-migration.ps1 -MigrationName "NombreMigracion"


Actualizar la base de datos:

cd backend/src/Gesco.Desktop.Data
dotnet ef database update -s ../Gesco.Desktop.UI/Gesco.Desktop.UI.csproj

🌐 Acceso a la Aplicación

Login: http://localhost:5100/login

Dashboard: http://localhost:5100/dashboard

Activación: http://localhost:5100/activation

API Swagger: http://localhost:5100/swagger


🔄 Sincronización con Laravel

En tu archivo .env configurar:

LARAVEL_API_URL=https://tu-servidor.com/api
LARAVEL_API_KEY=tu-api-key

🐞 Solución de Problemas

La base de datos no se crea

dotnet ef database update


Error de puerto en uso
Cambiar el puerto en .env:

LOCAL_API_PORT=5101

🤝 Contribuir

Haz un fork del proyecto

Crea una rama (git checkout -b feature/NuevaFeature)

Commit tus cambios (git commit -m 'Agrega nueva feature')

Push a tu rama (git push origin feature/NuevaFeature)

Abre un Pull Request

📄 Licencia

Este proyecto está bajo licencia MIT. Ver el archivo LICENSE
.

📬 Contacto

Yorkael Mc Carthey Sánchez

Email: hernaldomccarthy14@gmail.com

Link del Proyecto: https://github.com/Yorkamc/GescoDesktop.git