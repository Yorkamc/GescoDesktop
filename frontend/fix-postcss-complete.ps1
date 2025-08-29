# SOLUCIÓN COMPLETA PARA POSTCSS
Write-Host "🔧 REPARANDO CONFIGURACIÓN POSTCSS COMPLETA" -ForegroundColor Red
Write-Host "============================================" -ForegroundColor Red

# 1. ELIMINAR TODOS LOS CONFIGS POSIBLES
Write-Host "`n🗑️ Eliminando TODOS los configs PostCSS..." -ForegroundColor Yellow

$configsToDelete = @(
    "postcss.config.js",
    "postcss.config.cjs", 
    "postcss.config.mjs",
    "postcss.config.ts",
    "postcss.config.json",
    ".postcssrc",
    ".postcssrc.js",
    ".postcssrc.json",
    ".postcssrc.yml",
    ".postcssrc.yaml",
    "package.json.postcss"
)

foreach ($config in $configsToDelete) {
    if (Test-Path $config) {
        Write-Host "   ❌ Eliminando: $config" -ForegroundColor Gray
        Remove-Item $config -Force -ErrorAction SilentlyContinue
    }
}

# 2. VERIFICAR SI HAY CONFIGURACIÓN EN PACKAGE.JSON
Write-Host "`n📋 Verificando package.json..." -ForegroundColor Yellow

if (Test-Path "package.json") {
    $packageJson = Get-Content "package.json" -Raw | ConvertFrom-Json
    
    # Eliminar configuración postcss del package.json si existe
    if ($packageJson.postcss) {
        Write-Host "   ⚠️ Eliminando configuración PostCSS de package.json" -ForegroundColor Yellow
        $packageJson.PSObject.Properties.Remove('postcss')
        $packageJson | ConvertTo-Json -Depth 100 | Out-File "package.json" -Encoding UTF8
    } else {
        Write-Host "   ✅ package.json no tiene config PostCSS" -ForegroundColor Green
    }
} else {
    Write-Host "   ❌ package.json no encontrado" -ForegroundColor Red
}

# 3. LIMPIAR CACHE COMPLETO
Write-Host "`n🧹 Limpiando caché completo..." -ForegroundColor Yellow

$cacheItems = @(
    "node_modules/.vite",
    "node_modules/.cache",
    "dist",
    ".vite",
    ".next",
    ".turbo"
)

foreach ($item in $cacheItems) {
    if (Test-Path $item) {
        Write-Host "   🗑️ Eliminando: $item" -ForegroundColor Gray
        Remove-Item $item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# 4. CREAR POSTCSS CONFIG CORRECTO (ES Module)
Write-Host "`n✨ Creando postcss.config.js CORRECTO..." -ForegroundColor Green

$postcssConfig = @'
export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
'@

$postcssConfig | Out-File -FilePath "postcss.config.js" -Encoding UTF8 -NoNewline
Write-Host "   ✅ postcss.config.js creado" -ForegroundColor Green

# 5. VERIFICAR TAILWIND CONFIG
Write-Host "`n🎨 Verificando tailwind.config.js..." -ForegroundColor Yellow

if (-not (Test-Path "tailwind.config.js")) {
    Write-Host "   📝 Creando tailwind.config.js..." -ForegroundColor Gray
    
    $tailwindConfig = @'
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
'@
    
    $tailwindConfig | Out-File -FilePath "tailwind.config.js" -Encoding UTF8 -NoNewline
    Write-Host "   ✅ tailwind.config.js creado" -ForegroundColor Green
} else {
    Write-Host "   ✅ tailwind.config.js existe" -ForegroundColor Green
}

# 6. VERIFICAR VITE CONFIG
Write-Host "`n⚡ Verificando vite.config.ts..." -ForegroundColor Yellow

if (-not (Test-Path "vite.config.ts")) {
    Write-Host "   📝 Creando vite.config.ts..." -ForegroundColor Gray
    
    $viteConfig = @'
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5100',
        changeOrigin: true
      }
    }
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src/renderer'),
      '@main': path.resolve(__dirname, './src/main'),
      '@shared': path.resolve(__dirname, './src/shared')
    }
  }
});
'@
    
    $viteConfig | Out-File -FilePath "vite.config.ts" -Encoding UTF8 -NoNewline
    Write-Host "   ✅ vite.config.ts creado" -ForegroundColor Green
} else {
    Write-Host "   ✅ vite.config.ts existe" -ForegroundColor Green
}

# 7. VERIFICAR CSS PRINCIPAL
Write-Host "`n🎨 Verificando globals.css..." -ForegroundColor Yellow

$cssPath = "src/renderer/styles/globals.css"
if (-not (Test-Path $cssPath)) {
    Write-Host "   📝 Creando globals.css..." -ForegroundColor Gray
    
    # Crear directorio si no existe
    $cssDir = Split-Path $cssPath -Parent
    if (-not (Test-Path $cssDir)) {
        New-Item -Path $cssDir -ItemType Directory -Force | Out-Null
    }
    
    $globalsCss = @'
@tailwind base;
@tailwind components;
@tailwind utilities;

body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

code {
  font-family: source-code-pro, Menlo, Monaco, Consolas, 'Courier New',
    monospace;
}
'@
    
    $globalsCss | Out-File -FilePath $cssPath -Encoding UTF8 -NoNewline
    Write-Host "   ✅ globals.css creado" -ForegroundColor Green
} else {
    Write-Host "   ✅ globals.css existe" -ForegroundColor Green
}

# 8. MOSTRAR ARCHIVOS CREADOS
Write-Host "`n📁 Archivos de configuración:" -ForegroundColor Cyan
Get-ChildItem -Filter "*.config.*" | ForEach-Object {
    Write-Host "   ✅ $($_.Name)" -ForegroundColor White
}

# 9. VERIFICAR DEPENDENCIAS
Write-Host "`n📦 Verificando dependencias..." -ForegroundColor Yellow

$packageJsonContent = Get-Content "package.json" -Raw | ConvertFrom-Json

$requiredDeps = @("postcss", "tailwindcss", "autoprefixer")
$missingDeps = @()

foreach ($dep in $requiredDeps) {
    if (-not ($packageJsonContent.dependencies.$dep -or $packageJsonContent.devDependencies.$dep)) {
        $missingDeps += $dep
    }
}

if ($missingDeps.Count -gt 0) {
    Write-Host "   ⚠️ Dependencias faltantes: $($missingDeps -join ', ')" -ForegroundColor Yellow
    Write-Host "   💡 Instalar con: npm install -D $($missingDeps -join ' ')" -ForegroundColor Cyan
} else {
    Write-Host "   ✅ Todas las dependencias están presentes" -ForegroundColor Green
}

# 10. RESUMEN FINAL
Write-Host "`n🎉 REPARACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host "✅ PostCSS config recreado (ES Module)" -ForegroundColor White
Write-Host "✅ TailwindCSS config verificado" -ForegroundColor White  
Write-Host "✅ Vite config verificado" -ForegroundColor White
Write-Host "✅ CSS principal verificado" -ForegroundColor White
Write-Host "✅ Caché completo limpiado" -ForegroundColor White

Write-Host "`n🚀 AHORA EJECUTA:" -ForegroundColor Cyan
Write-Host "   npm run dev" -ForegroundColor White

Write-Host "`n💡 Si aún hay error:" -ForegroundColor Yellow
Write-Host "   1. Ctrl+C para detener Vite" -ForegroundColor White
Write-Host "   2. npm install (reinstalar dependencias)" -ForegroundColor White
Write-Host "   3. npm run dev" -ForegroundColor White