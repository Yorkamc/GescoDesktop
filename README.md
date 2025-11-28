GESCO Desktop
Sistema de Gestión de Eventos y Actividades Comunitarias - Versión Desktop
Descripción
GESCO Desktop es una aplicación híbrida que combina un backend robusto en .NET con una interfaz moderna en React, empaquetada como aplicación de escritorio con Electron. Diseñada para funcionar completamente offline con capacidades de sincronización cuando hay conexión disponible.
Características Principales
Arquitectura Híbrida

Backend: ASP.NET Core 8 + Entity Framework Core + SQLite
Frontend: React 18 + TypeScript + Electron
UI: Tailwind CSS para diseño moderno y responsivo

Modo Offline First

Base de datos local SQLite embebida
Funcionalidad completa sin conexión a internet
Sincronización automática con servidor Laravel cuando hay conectividad

Seguridad

Autenticación con BCrypt
Sistema de activación de licencias
Gestión de sesiones y tokens

Gestión Completa

Actividades y eventos
Categorías y productos
Combos y promociones
Cajas registradoras
Punto de venta (POS)
Inventario y movimientos de stock
Reportes y dashboards

Estructura del Proyecto
GescoDesktop/
├── backend/
│   ├── src/
│   │   ├── Gesco.Desktop.Core/      # Lógica de negocio y servicios
│   │   ├── Gesco.Desktop.Data/      # Acceso a datos, migraciones, contexto
│   │   ├── Gesco.Desktop.Shared/    # DTOs, modelos compartidos
│   │   ├── Gesco.Desktop.Sync/      # Sincronización con servidor
│   │   └── Gesco.Desktop.UI/        # API REST con ASP.NET Core
│   └── tests/                       # Pruebas unitarias e integración
│
├── frontend/
│   ├── public/                      # Recursos estáticos
│   ├── src/
│   │   ├── components/              # Componentes React reutilizables
│   │   ├── pages/                   # Vistas principales
│   │   ├── hooks/                   # Custom React hooks
│   │   ├── services/                # APIs y servicios
│   │   ├── types/                   # Definiciones TypeScript
│   │   └── utils/                   # Utilidades y helpers
│   ├── electron-main.cjs            # Proceso principal Electron
│   └── package.json
│
├── .env.example                     # Plantilla de variables de entorno
├── package.json                     # Scripts NPM principales
├── GescoDesktop.sln                 # Solución .NET
└── README.md

Requisitos del Sistema
Desarrollo:

.NET 8.0 SDK o superior
Node.js 18+ y npm
Git

Producción:

Windows 10/11, macOS 10.15+, o Linux (Ubuntu 20.04+)
4 GB RAM mínimo (8 GB recomendado)
500 MB espacio en disco

Instalación
1. Clonar el Repositorio
2. Configurar Variables de Entorno
3. Instalar Dependencias

Licencia
Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.

Contacto
Desarrollador: Yorkael Mc Carthey Sánchez
Email: hernaldomccarthy14@gmail.com
Proyecto: GESCO Desktop en GitHub

Agradecimientos

ASP.NET Core Team
React Team
Electron Team
Tailwind CSS Team
Entity Framework Core Team
Comunidad Open Source