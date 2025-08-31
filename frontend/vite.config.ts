import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  
  // CRÍTICO: Base DEBE ser './' para Electron
  base: './',
  
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    assetsDir: 'assets',
    // Configuración mejorada para Electron
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'index.html')
      },
      output: {
        // Evitar nombres de archivos con hash muy largos
        assetFileNames: 'assets/[name].[ext]',
        chunkFileNames: 'assets/[name].js',
        entryFileNames: 'assets/[name].js',
      }
    },
    // Configuración adicional para compatibilidad
    target: 'es2015', // Compatibilidad con Electron
    minify: 'esbuild',
    sourcemap: false, // Desactivar sourcemaps en producción para reducir tamaño
    
    // Configuración para manejar assets correctamente
    assetsInlineLimit: 4096, // Inline assets < 4kb
  },
  
  server: {
    port: 5173,
    host: '127.0.0.1',
    strictPort: true,
    cors: true,
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
    'process.env.VITE_APP_VERSION': JSON.stringify(process.env.npm_package_version || '1.0.0'),
  },
  
  // Optimización para Electron
  clearScreen: false,
  
  // Configuración CSS
  css: {
    postcss: './postcss.config.cjs'
  },

  // Configuración de optimización de dependencias
  optimizeDeps: {
    include: [
      'react',
      'react-dom',
      'react-router-dom',
      'axios',
      '@tanstack/react-query'
    ]
  },

  // Configuración para preview (testing del build)
  preview: {
    port: 4173,
    host: '127.0.0.1'
  }
})