/**
 * Servicio de almacenamiento persistente entre versiones
 * Usa una ubicación fija independiente de la versión de la app
 */

interface StorageData {
  token: string | null;
  user: any | null;
  lastLogin: string | null;
  appVersion: string;
}

class StorageService {
  private readonly STORAGE_KEY = 'gesco-desktop-auth';
  private readonly APP_NAME = 'GESCODesktop'; // Sin espacios, sin versión
  
  /**
   * Obtiene la ruta de almacenamiento consistente
   * Esta ruta NO cambia entre versiones
   */
  private getStoragePath(): string {
    // Verificar si estamos en Electron
    if (typeof window !== 'undefined' && (window as any).electronAPI) {
      return 'electron-userdata'; // Señal para usar electron-store
    }
    
    // Fallback a localStorage en web
    return 'localStorage';
  }

  /**
   * Guarda datos de autenticación de forma persistente
   */
  async saveAuthData(token: string, user: any): Promise<void> {
    const data: StorageData = {
      token,
      user,
      lastLogin: new Date().toISOString(),
      appVersion: this.getAppVersion()
    };

    try {
      const path = this.getStoragePath();
      
      if (path === 'electron-userdata') {
        // Usar API de Electron si está disponible
        await this.saveToElectron(data);
      } else {
        // Fallback a localStorage
        this.saveToLocalStorage(data);
      }
      
      console.log('✅ Sesión guardada en almacenamiento persistente');
    } catch (error) {
      console.error('❌ Error guardando sesión:', error);
      // Fallback siempre a localStorage
      this.saveToLocalStorage(data);
    }
  }

  /**
   * Recupera datos de autenticación
   */
  async loadAuthData(): Promise<StorageData | null> {
    try {
      const path = this.getStoragePath();
      
      let data: StorageData | null = null;
      
      if (path === 'electron-userdata') {
        data = await this.loadFromElectron();
      } else {
        data = this.loadFromLocalStorage();
      }

      if (data) {
        console.log(`✅ Sesión restaurada (guardada en v${data.appVersion})`);
        return data;
      }
      
      console.log('📝 No hay sesión guardada');
      return null;
    } catch (error) {
      console.error('❌ Error cargando sesión:', error);
      // Intentar cargar de localStorage como fallback
      return this.loadFromLocalStorage();
    }
  }

  /**
   * Elimina datos de autenticación
   */
  async clearAuthData(): Promise<void> {
    try {
      const path = this.getStoragePath();
      
      if (path === 'electron-userdata') {
        await this.clearFromElectron();
      } else {
        this.clearFromLocalStorage();
      }
      
      console.log('✅ Sesión eliminada del almacenamiento persistente');
    } catch (error) {
      console.error('❌ Error eliminando sesión:', error);
      this.clearFromLocalStorage();
    }
  }

  // ============================================
  // MÉTODOS PRIVADOS - ELECTRON
  // ============================================

  private async saveToElectron(data: StorageData): Promise<void> {
    // Usar la API de Electron si está disponible
    if ((window as any).electronAPI?.storage?.set) {
      await (window as any).electronAPI.storage.set(this.STORAGE_KEY, data);
    } else {
      // Fallback a localStorage
      this.saveToLocalStorage(data);
    }
  }

  private async loadFromElectron(): Promise<StorageData | null> {
    if ((window as any).electronAPI?.storage?.get) {
      return await (window as any).electronAPI.storage.get(this.STORAGE_KEY);
    }
    return this.loadFromLocalStorage();
  }

  private async clearFromElectron(): Promise<void> {
    if ((window as any).electronAPI?.storage?.delete) {
      await (window as any).electronAPI.storage.delete(this.STORAGE_KEY);
    }
    this.clearFromLocalStorage();
  }

  // ============================================
  // MÉTODOS PRIVADOS - LOCALSTORAGE
  // ============================================

  private saveToLocalStorage(data: StorageData): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(data));
      // También guardar en las claves originales para compatibilidad
      localStorage.setItem('token', data.token || '');
      localStorage.setItem('user', JSON.stringify(data.user));
    } catch (error) {
      console.error('Error guardando en localStorage:', error);
    }
  }

  private loadFromLocalStorage(): StorageData | null {
    try {
      // Intentar cargar del nuevo formato unificado
      const unified = localStorage.getItem(this.STORAGE_KEY);
      if (unified) {
        return JSON.parse(unified);
      }

      // Fallback: intentar cargar del formato antiguo
      const token = localStorage.getItem('token');
      const userStr = localStorage.getItem('user');
      
      if (token && userStr) {
        const user = JSON.parse(userStr);
        return {
          token,
          user,
          lastLogin: null,
          appVersion: this.getAppVersion()
        };
      }

      return null;
    } catch (error) {
      console.error('Error cargando de localStorage:', error);
      return null;
    }
  }

  private clearFromLocalStorage(): void {
    try {
      localStorage.removeItem(this.STORAGE_KEY);
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    } catch (error) {
      console.error('Error limpiando localStorage:', error);
    }
  }

  // ============================================
  // UTILIDADES
  // ============================================

  private getAppVersion(): string {
    // Intentar obtener la versión de package.json o del proceso
    if (typeof window !== 'undefined' && (window as any).electronAPI?.getVersion) {
      return (window as any).electronAPI.getVersion();
    }
    return '1.0.0';
  }

  /**
   * Migra datos del formato antiguo al nuevo
   * Útil para actualizar de versiones anteriores
   */
  async migrateFromOldFormat(): Promise<boolean> {
    try {
      const oldToken = localStorage.getItem('token');
      const oldUserStr = localStorage.getItem('user');

      if (oldToken && oldUserStr && !localStorage.getItem(this.STORAGE_KEY)) {
        const user = JSON.parse(oldUserStr);
        await this.saveAuthData(oldToken, user);
        console.log('✅ Datos migrados del formato antiguo');
        return true;
      }

      return false;
    } catch (error) {
      console.error('❌ Error migrando datos:', error);
      return false;
    }
  }
}

// Exportar instancia única (singleton)
export const storageService = new StorageService();