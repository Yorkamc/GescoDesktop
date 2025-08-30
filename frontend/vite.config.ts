import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  
  // CRÍTICO: Base relativa para Electron
  base: './',
  
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    // Generar archivos con rutas relativas
    assetsDir: 'assets',
  },
  
  server: {
    port: 5173,
    host: '127.0.0.1',
    strictPort: true,
  },
  
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src/renderer'),
    },
  },
  
  // Optimizaciones para Electron
  define: {
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'development'),
  },
  
  // No limpiar la pantalla para ver logs de Electron
  clearScreen: false,
})