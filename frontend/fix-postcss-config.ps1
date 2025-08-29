# Script para arreglar configuración PostCSS corrupta
Write-Host "🔧 REPARANDO CONFIGURACIÓN POSTCSS" -ForegroundColor Red
Write-Host "===================================" -ForegroundColor Red

# Verificar directorio
if (-not (Test-Path "package.json")) {
    Write-Host "Error: Ejecutar desde el directorio frontend/" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Problema detectado:" -ForegroundColor Yellow
Write-Host "   - PostCSS config tiene JSON inválido" -ForegroundColor White
Write-Host "   - Vite no puede cargar la configuración" -ForegroundColor White

# 1. ELIMINAR CONFIG CORRUPTO
Write-Host "`n🗑️ Eliminando configs corruptos..." -ForegroundColor Yellow
$configsToDelete = @(
    "postcss.config.js",
    "postcss.config.cjs", 
    "postcss.config.json",
    ".postcssrc",
    ".postcssrc.js",
    ".postcssrc.json"
)

foreach ($config in $configsToDelete) {
    if (Test-Path $config) {
        Write-Host "   Eliminando: $config" -ForegroundColor Gray
        Remove-Item $config -Force -ErrorAction SilentlyContinue
    }
}

# 2. CREAR POSTCSS CONFIG CORRECTO
Write-Host "`n✨ Creando postcss.config.js correcto..." -ForegroundColor Green

$postcssConfig = @"
export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
"@

$postcssConfig | Out-File -FilePath "postcss.config.js" -Encoding UTF8
Write-Host "   ✅ postcss.config.js creado" -ForegroundColor Green

# 3. VERIFICAR TAILWIND CONFIG
Write-Host "`n🎨 Verificando tailwind.config.js..." -ForegroundColor Yellow

if (-not (Test-Path "tailwind.config.js")) {
    Write-Host "   Creando tailwind.config.js..." -ForegroundColor Gray
    
    $tailwindConfig = @"
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
"@
    
    $tailwindConfig | Out-File -FilePath "tailwind.config.js" -Encoding UTF8
    Write-Host "   ✅ tailwind.config.js creado" -ForegroundColor Green
} else {
    Write-Host "   ✅ tailwind.config.js existe" -ForegroundColor Green
}

# 4. VERIFICAR VITE CONFIG
Write-Host "`n⚡ Verificando vite.config.ts..." -ForegroundColor Yellow

if (Test-Path "vite.config.ts") {
    Write-Host "   ✅ vite.config.ts existe" -ForegroundColor Green
} else {
    Write-Host "   Creando vite.config.ts..." -ForegroundColor Gray
    
    $viteConfig = @"
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
"@
    
    $viteConfig | Out-File -FilePath "vite.config.ts" -Encoding UTF8
    Write-Host "   ✅ vite.config.ts creado" -ForegroundColor Green
}

# 5. LIMPIAR CACHE
Write-Host "`n🧹 Limpiando caché..." -ForegroundColor Yellow

$cacheItems = @(
    "node_modules/.vite",
    "dist",
    ".vite"
)

foreach ($item in $cacheItems) {
    if (Test-Path $item) {
        Write-Host "   Limpiando: $item" -ForegroundColor Gray
        Remove-Item $item -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "   ✅ Caché limpiado" -ForegroundColor Green

# 6. MOSTRAR RESUMEN
Write-Host "`n✅ REPARACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host "✅ PostCSS config recreado" -ForegroundColor White
Write-Host "✅ TailwindCSS config verificado" -ForegroundColor White  
Write-Host "✅ Vite config verificado" -ForegroundColor White
Write-Host "✅ Caché limpiado" -ForegroundColor White

Write-Host "`n🚀 Ahora puedes ejecutar:" -ForegroundColor Cyan
Write-Host "   npm run dev" -ForegroundColor White

Write-Host "`n💡 Si persiste el error:" -ForegroundColor Yellow
Write-Host "   1. Ctrl+C para detener Vite" -ForegroundColor White
Write-Host "   2. npm run dev de nuevo" -ForegroundColor White
Write-Host "   3. O reinicia la terminal" -ForegroundColor White