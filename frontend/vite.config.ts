import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  
  // CRÍTICO para Electron
  base: './',
  
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    
    // Configuración ultra-simplificada
    rollupOptions: {
      output: {
        // Sin hash en los nombres para debugging
        entryFileNames: 'assets/main.js',
        chunkFileNames: 'assets/[name].js',
        assetFileNames: 'assets/[name].[ext]'
      }
    },
    
    // Sin optimizaciones para debugging
    target: 'es2015',
    minify: false,
    sourcemap: true,
    assetsInlineLimit: 0
  },
  
  server: {
    port: 5173,
    host: '127.0.0.1'
  }
})