#  GESCO Desktop - Build & Distribution

## 📋 Requisitos

- Node.js 18+
- .NET 8 SDK
- npm or yarn

## 🛠️ Desarrollo

`bash
# Instalar dependencias
npm run install:frontend

# Iniciar desarrollo completo (Backend + Frontend)
npm run dev

# Solo backend
npm run dev:backend

# Solo frontend  
npm run dev:frontend

# Desarrollo con Electron
npm run dev:electron
`

## 📦 Build para Producción

`bash
# Build completo (Backend + Frontend)
npm run build

# Solo backend
npm run build:backend

# Solo frontend
npm run build:frontend
`

## 📱 Build Electron (Aplicación de Escritorio)

`bash
# Build para plataforma actual
cd frontend && npm run electron:build

# Build para Windows
cd frontend && npm run electron:build:win

# Build para macOS
cd frontend && npm run electron:build:mac

# Build para Linux  
cd frontend && npm run electron:build:linux

# Build para todas las plataformas
cd frontend && npm run electron:build:all
`

##  Estructura de Salida

`
dist/
 backend/          # API .NET compilada
└── frontend/
    ├── dist/         # Web build
    └── dist-electron/ # Apps de escritorio
         win-unpacked/
         linux-unpacked/
         mac/
         GESCO-Desktop-Setup.exe
         GESCO-Desktop.AppImage
         GESCO-Desktop.dmg
`

## 🎨 Iconos

Coloca los iconos en rontend/build/:
- icon.ico (Windows)
- icon.icns (macOS)  
- icon.png (Linux, 512x512px)

## 🔄 Migraciones de Base de Datos

`bash
# Crear nueva migración
npm run migrate

# Aplicar migraciones (automático en desarrollo)
`
