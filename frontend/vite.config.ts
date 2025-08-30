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
    assetsDir: 'assets',
    // Configuración adicional para Electron
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'index.html')
      }
    }
  },
  
  server: {
    port: 5173,
    host: '127.0.0.1',
    strictPort: true,
    // CORS para desarrollo
    cors: true,
    // Configuración adicional para desarrollo
    hmr: {
      port: 5174
    }
  },
  
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src/renderer'),
    },
  },
  
  // Variables de entorno
  define: {
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'development'),
  },
  
  // Optimización para Electron
  clearScreen: false,
  
  // Configuración CSS
  css: {
    postcss: './postcss.config.cjs'
  }
})