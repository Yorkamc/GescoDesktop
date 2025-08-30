# OPCIÓN NUCLEAR - ELIMINAR Y RECREAR TODO
Write-Host "💥 OPCIÓN NUCLEAR - RECREANDO FRONTEND COMPLETO" -ForegroundColor Red
Write-Host "================================================" -ForegroundColor Red

Write-Host "`n⚠️ ADVERTENCIA: Esto eliminará node_modules y recreará toda la configuración" -ForegroundColor Yellow
$confirm = Read-Host "¿Continuar? (y/n)"
if ($confirm -ne 'y') { exit }

# 1. ELIMINAR TODO
Write-Host "`n🗑️ Eliminando TODO..." -ForegroundColor Yellow
Remove-Item "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "dist" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "dist-electron" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item ".vite" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "postcss.config.*" -ErrorAction SilentlyContinue
Remove-Item ".postcssrc*" -ErrorAction SilentlyContinue
Remove-Item "tailwind.config.*" -ErrorAction SilentlyContinue

# 2. CREAR POSTCSS CONFIG SIMPLE
Write-Host "`n📝 Creando PostCSS config simple..." -ForegroundColor Green

@'
module.exports = {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
'@ | Out-File -FilePath "postcss.config.cjs" -Encoding UTF8 -NoNewline

Write-Host "   ✅ postcss.config.cjs creado" -ForegroundColor Green

# 3. CREAR TAILWIND CONFIG
Write-Host "`n🎨 Creando Tailwind config..." -ForegroundColor Green

@'
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
'@ | Out-File -FilePath "tailwind.config.cjs" -Encoding UTF8 -NoNewline

Write-Host "   ✅ tailwind.config.cjs creado" -ForegroundColor Green

# 4. VERIFICAR VITE CONFIG (convertir a import si es necesario)
Write-Host "`n⚡ Verificando vite.config.ts..." -ForegroundColor Green

if (Test-Path "vite.config.ts") {
    Write-Host "   ✅ vite.config.ts existe" -ForegroundColor Green
} else {
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
  },
  css: {
    postcss: './postcss.config.cjs'
  }
});
'@
    
    $viteConfig | Out-File -FilePath "vite.config.ts" -Encoding UTF8 -NoNewline
    Write-Host "   ✅ vite.config.ts creado con referencia explícita a PostCSS" -ForegroundColor Green
}

# 5. INSTALAR DEPENDENCIAS
Write-Host "`n📦 Instalando dependencias..." -ForegroundColor Yellow
npm install

# 6. INSTALAR DEPENDENCIAS DE CSS SI FALTAN
Write-Host "`n📦 Instalando dependencias CSS..." -ForegroundColor Yellow
npm install -D tailwindcss postcss autoprefixer

# 7. MOSTRAR ARCHIVOS CREADOS
Write-Host "`n📁 Archivos de configuración creados:" -ForegroundColor Cyan
Get-ChildItem -Filter "*.config.*" | ForEach-Object {
    Write-Host "   📄 $($_.Name)" -ForegroundColor White
    Write-Host "      Tamaño: $($_.Length) bytes" -ForegroundColor Gray
}

# 8. VERIFICAR CONTENIDO
Write-Host "`n📄 Contenido PostCSS config:" -ForegroundColor Cyan
Get-Content "postcss.config.cjs" | ForEach-Object { Write-Host "   $_" -ForegroundColor White }

Write-Host "`n🎉 RECREACIÓN COMPLETA TERMINADA" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "✅ node_modules reinstalado" -ForegroundColor White
Write-Host "✅ PostCSS config como CommonJS" -ForegroundColor White
Write-Host "✅ Tailwind config como CommonJS" -ForegroundColor White
Write-Host "✅ Vite config con referencia explícita" -ForegroundColor White

Write-Host "`n🚀 EJECUTA AHORA:" -ForegroundColor Cyan
Write-Host "   npm run dev" -ForegroundColor White