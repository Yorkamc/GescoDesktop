const { contextBridge } = require('electron');
const path = require('path');
const fs = require('fs');
const os = require('os');

console.log('🔌 Preload script cargado');

// Obtener ruta de userData (consistente entre versiones)
const getUserDataPath = () => {
  try {
    // Nombre fijo sin versión
    const appName = 'GESCODesktop';
    
    // Determinar la ruta base según el sistema operativo
    let basePath;
    if (process.platform === 'win32') {
      basePath = process.env.APPDATA || path.join(os.homedir(), 'AppData', 'Roaming');
    } else if (process.platform === 'darwin') {
      basePath = path.join(os.homedir(), 'Library', 'Application Support');
    } else {
      basePath = path.join(os.homedir(), '.config');
    }
    
    return path.join(basePath, appName);
  } catch (error) {
    console.error('Error obteniendo userData path:', error);
    return null;
  }
};

// Crear directorio si no existe
const ensureDir = (dirPath) => {
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
  }
};

// Ruta fija para el archivo de almacenamiento
const STORAGE_FILE = 'auth-data.json';

// API de almacenamiento persistente
const storageAPI = {
  async get(key) {
    try {
      const userDataPath = getUserDataPath();
      if (!userDataPath) {
        console.warn('No se pudo obtener userData path');
        return null;
      }

      ensureDir(userDataPath);
      const filePath = path.join(userDataPath, STORAGE_FILE);

      if (!fs.existsSync(filePath)) {
        return null;
      }

      const data = fs.readFileSync(filePath, 'utf8');
      const parsed = JSON.parse(data);
      
      console.log(`✅ Datos leídos de: ${filePath}`);
      return parsed[key] || null;
    } catch (error) {
      console.error('Error leyendo storage:', error);
      return null;
    }
  },

  async set(key, value) {
    try {
      const userDataPath = getUserDataPath();
      if (!userDataPath) {
        console.warn('No se pudo obtener userData path');
        return false;
      }

      ensureDir(userDataPath);
      const filePath = path.join(userDataPath, STORAGE_FILE);

      let currentData = {};
      if (fs.existsSync(filePath)) {
        try {
          const existing = fs.readFileSync(filePath, 'utf8');
          currentData = JSON.parse(existing);
        } catch (e) {
          console.warn('Archivo de storage corrupto, creando nuevo');
        }
      }

      currentData[key] = value;
      fs.writeFileSync(filePath, JSON.stringify(currentData, null, 2), 'utf8');
      
      console.log(`✅ Datos guardados en: ${filePath}`);
      return true;
    } catch (error) {
      console.error('Error escribiendo storage:', error);
      return false;
    }
  },

  async delete(key) {
    try {
      const userDataPath = getUserDataPath();
      if (!userDataPath) return false;

      const filePath = path.join(userDataPath, STORAGE_FILE);

      if (!fs.existsSync(filePath)) {
        return true;
      }

      const data = fs.readFileSync(filePath, 'utf8');
      const parsed = JSON.parse(data);
      delete parsed[key];

      fs.writeFileSync(filePath, JSON.stringify(parsed, null, 2), 'utf8');
      console.log(`✅ Clave "${key}" eliminada de storage`);
      return true;
    } catch (error) {
      console.error('Error eliminando de storage:', error);
      return false;
    }
  },

  async clear() {
    try {
      const userDataPath = getUserDataPath();
      if (!userDataPath) return false;

      const filePath = path.join(userDataPath, STORAGE_FILE);

      if (fs.existsSync(filePath)) {
        fs.unlinkSync(filePath);
        console.log('✅ Storage limpiado completamente');
      }
      return true;
    } catch (error) {
      console.error('Error limpiando storage:', error);
      return false;
    }
  },

  getStoragePath() {
    const userDataPath = getUserDataPath();
    return userDataPath ? path.join(userDataPath, STORAGE_FILE) : null;
  }
};

// Exponer API al renderer de manera segura
contextBridge.exposeInMainWorld('electronAPI', {
  storage: storageAPI,
  
  getVersion: () => {
    return process.env.npm_package_version || '1.0.0';
  },
  
  getAppPath: () => {
    return getUserDataPath();
  },
  
  platform: process.platform,
  
  // Para debugging
  debug: {
    getStoragePath: () => storageAPI.getStoragePath(),
    getUserDataPath: getUserDataPath
  }
});

console.log('✅ API de Electron expuesta al renderer');
console.log('📂 Ruta de almacenamiento:', getUserDataPath());