import { safeStorage, app } from 'electron';
import * as crypto from 'crypto';
import * as fs from 'fs/promises';
import * as path from 'path';

interface SecretMetadata {
  id: string;
  level: 'critical' | 'important' | 'generated';
  method: 'safeStorage' | 'encrypted' | 'memory';
  createdAt: Date;
  lastAccessed?: Date;
  expiresAt?: Date;
  persistent: boolean;
}

interface StoredSecret {
  data: string;
  metadata: SecretMetadata;
}

export class GescoSecretsManager {
  private static instance: GescoSecretsManager;
  private readonly serviceName = 'GESCO-Desktop';
  private readonly secretsDir: string;
  private readonly metadataFile: string;
  private readonly masterKey: Buffer;
  private metadata: Map<string, SecretMetadata> = new Map();
  private memorySecrets: Map<string, string> = new Map();
  private isInitialized = false;

  private constructor() {
    this.secretsDir = path.join(app.getPath('userData'), 'secure');
    this.metadataFile = path.join(this.secretsDir, 'metadata.enc');
    this.masterKey = this.deriveMasterKey();
  }

  public static getInstance(): GescoSecretsManager {
    if (!GescoSecretsManager.instance) {
      GescoSecretsManager.instance = new GescoSecretsManager();
    }
    return GescoSecretsManager.instance;
  }

  // ============================================
  // INICIALIZACIÓN Y CONFIGURACIÓN
  // ============================================

  public async initialize(): Promise<void> {
    if (this.isInitialized) return;

    try {
      await fs.mkdir(this.secretsDir, { recursive: true });
      await this.loadMetadata();
      await this.performMaintenanceTasks();
      this.isInitialized = true;
      console.log('✅ GescoSecretsManager inicializado correctamente');
    } catch (error) {
      console.error('❌ Error inicializando GescoSecretsManager:', error);
      throw error;
    }
  }

  private deriveMasterKey(): Buffer {
    // Generar clave maestra basada en características del sistema
    const machineId = app.getName() + process.platform + app.getVersion();
    const hash = crypto.createHash('sha256');
    hash.update(machineId);
    hash.update(app.getPath('exe')); // Ruta del ejecutable
    hash.update(process.env.USERNAME || 'defaultuser');
    return hash.digest();
  }

  // ============================================
  // GESTIÓN DE SECRETOS CRÍTICOS (SAFESTORAGE)
  // ============================================

  public async setCriticalSecret(key: string, value: string, expiresIn?: number): Promise<void> {
    if (!safeStorage.isEncryptionAvailable()) {
      throw new Error('Electron safeStorage no está disponible para secretos críticos');
    }

    try {
      const encrypted = safeStorage.encryptString(value);
      const encodedValue = encrypted.toString('base64');
      
      // Almacenar en archivo con prefijo crítico
      const secretPath = path.join(this.secretsDir, `critical_${key}.enc`);
      await fs.writeFile(secretPath, encodedValue, 'utf8');
      
      const metadata: SecretMetadata = {
        id: key,
        level: 'critical',
        method: 'safeStorage',
        persistent: true,
        createdAt: new Date(),
        expiresAt: expiresIn ? new Date(Date.now() + expiresIn) : undefined
      };
      
      this.metadata.set(key, metadata);
      await this.saveMetadata();
      
      console.log(`🔐 Secret crítico '${key}' almacenado con safeStorage`);
    } catch (error) {
      console.error(`❌ Error almacenando secret crítico '${key}':`, error);
      throw error;
    }
  }

  public async getCriticalSecret(key: string): Promise<string | null> {
    try {
      const metadata = this.metadata.get(key);
      if (!metadata || metadata.method !== 'safeStorage' || metadata.level !== 'critical') {
        return null;
      }

      // Verificar expiración
      if (metadata.expiresAt && metadata.expiresAt < new Date()) {
        await this.deleteCriticalSecret(key);
        return null;
      }

      const secretPath = path.join(this.secretsDir, `critical_${key}.enc`);
      const encodedValue = await fs.readFile(secretPath, 'utf8');
      const encrypted = Buffer.from(encodedValue, 'base64');
      
      const decrypted = safeStorage.decryptString(encrypted);
      
      // Actualizar último acceso
      metadata.lastAccessed = new Date();
      await this.saveMetadata();
      
      return decrypted;
    } catch (error) {
      console.error(`❌ Error obteniendo secret crítico '${key}':`, error);
      return null;
    }
  }

  public async deleteCriticalSecret(key: string): Promise<boolean> {
    try {
      const secretPath = path.join(this.secretsDir, `critical_${key}.enc`);
      await fs.unlink(secretPath);
      this.metadata.delete(key);
      await this.saveMetadata();
      return true;
    } catch (error) {
      console.error(`❌ Error eliminando secret crítico '${key}':`, error);
      return false;
    }
  }

  // ============================================
  // GESTIÓN DE SECRETOS IMPORTANTES (SAFESTORAGE)
  // ============================================

  public async setImportantSecret(key: string, value: string, expiresIn?: number): Promise<void> {
    if (!safeStorage.isEncryptionAvailable()) {
      throw new Error('Electron safeStorage no está disponible');
    }

    try {
      const encrypted = safeStorage.encryptString(value);
      const encodedValue = encrypted.toString('base64');
      
      const secretPath = path.join(this.secretsDir, `${key}.enc`);
      await fs.writeFile(secretPath, encodedValue, 'utf8');
      
      const metadata: SecretMetadata = {
        id: key,
        level: 'important',
        method: 'safeStorage',
        persistent: true,
        createdAt: new Date(),
        expiresAt: expiresIn ? new Date(Date.now() + expiresIn) : undefined
      };
      
      this.metadata.set(key, metadata);
      await this.saveMetadata();
      
      console.log(`🔒 Secret importante '${key}' almacenado con safeStorage`);
    } catch (error) {
      console.error(`❌ Error almacenando secret importante '${key}':`, error);
      throw error;
    }
  }

  public async getImportantSecret(key: string): Promise<string | null> {
    try {
      const metadata = this.metadata.get(key);
      if (!metadata || metadata.method !== 'safeStorage' || metadata.level !== 'important') {
        return null;
      }

      // Verificar expiración
      if (metadata.expiresAt && metadata.expiresAt < new Date()) {
        await this.deleteImportantSecret(key);
        return null;
      }

      const secretPath = path.join(this.secretsDir, `${key}.enc`);
      const encodedValue = await fs.readFile(secretPath, 'utf8');
      const encrypted = Buffer.from(encodedValue, 'base64');
      
      const decrypted = safeStorage.decryptString(encrypted);
      
      // Actualizar último acceso
      metadata.lastAccessed = new Date();
      await this.saveMetadata();
      
      return decrypted;
    } catch (error) {
      console.error(`❌ Error obteniendo secret importante '${key}':`, error);
      return null;
    }
  }

  public async deleteImportantSecret(key: string): Promise<boolean> {
    try {
      const secretPath = path.join(this.secretsDir, `${key}.enc`);
      await fs.unlink(secretPath);
      this.metadata.delete(key);
      await this.saveMetadata();
      return true;
    } catch (error) {
      console.error(`❌ Error eliminando secret importante '${key}':`, error);
      return false;
    }
  }

  // ============================================
  // GESTIÓN DE SECRETOS TEMPORALES (MEMORIA)
  // ============================================

  public setTemporarySecret(key: string, value: string, expiresIn: number = 3600000): void {
    // Almacenar en memoria con expiración automática
    this.memorySecrets.set(key, value);
    
    const metadata: SecretMetadata = {
      id: key,
      level: 'generated',
      method: 'memory',
      persistent: false,
      createdAt: new Date(),
      expiresAt: new Date(Date.now() + expiresIn)
    };
    
    this.metadata.set(key, metadata);
    
    // Programar eliminación automática
    setTimeout(() => {
      this.deleteTemporarySecret(key);
    }, expiresIn);
    
    console.log(`🔑 Secret temporal '${key}' almacenado en memoria`);
  }

  public getTemporarySecret(key: string): string | null {
    const metadata = this.metadata.get(key);
    if (!metadata || metadata.method !== 'memory') {
      return null;
    }

    // Verificar expiración
    if (metadata.expiresAt && metadata.expiresAt < new Date()) {
      this.deleteTemporarySecret(key);
      return null;
    }

    const value = this.memorySecrets.get(key);
    if (value) {
      metadata.lastAccessed = new Date();
    }

    return value || null;
  }

  public deleteTemporarySecret(key: string): boolean {
    const deleted = this.memorySecrets.delete(key);
    this.metadata.delete(key);
    return deleted;
  }

  // ============================================
  // GESTIÓN DE SECRETOS GENERADOS (ENCRIPTACIÓN PERSONALIZADA)
  // ============================================

  public async setGeneratedSecret(key: string, value: string, expiresIn?: number): Promise<void> {
    try {
      const encrypted = this.encryptWithMasterKey(value);
      const secretPath = path.join(this.secretsDir, `gen_${key}.enc`);
      await fs.writeFile(secretPath, encrypted, 'utf8');
      
      const metadata: SecretMetadata = {
        id: key,
        level: 'generated',
        method: 'encrypted',
        persistent: true,
        createdAt: new Date(),
        expiresAt: expiresIn ? new Date(Date.now() + expiresIn) : undefined
      };
      
      this.metadata.set(key, metadata);
      await this.saveMetadata();
      
      console.log(`🔑 Secret generado '${key}' almacenado con encriptación personalizada`);
    } catch (error) {
      console.error(`❌ Error almacenando secret generado '${key}':`, error);
      throw error;
    }
  }

  public async getGeneratedSecret(key: string): Promise<string | null> {
    try {
      const metadata = this.metadata.get(key);
      if (!metadata || metadata.method !== 'encrypted') {
        return null;
      }

      // Verificar expiración
      if (metadata.expiresAt && metadata.expiresAt < new Date()) {
        await this.deleteGeneratedSecret(key);
        return null;
      }

      const secretPath = path.join(this.secretsDir, `gen_${key}.enc`);
      const encrypted = await fs.readFile(secretPath, 'utf8');
      const decrypted = this.decryptWithMasterKey(encrypted);
      
      // Actualizar último acceso
      metadata.lastAccessed = new Date();
      await this.saveMetadata();
      
      return decrypted;
    } catch (error) {
      console.error(`❌ Error obteniendo secret generado '${key}':`, error);
      return null;
    }
  }

  public async deleteGeneratedSecret(key: string): Promise<boolean> {
    try {
      const secretPath = path.join(this.secretsDir, `gen_${key}.enc`);
      await fs.unlink(secretPath);
      this.metadata.delete(key);
      await this.saveMetadata();
      return true;
    } catch (error) {
      console.error(`❌ Error eliminando secret generado '${key}':`, error);
      return false;
    }
  }

  // ============================================
  // INTERFAZ UNIFICADA MEJORADA
  // ============================================

  public async setSecret(
    key: string, 
    value: string, 
    options: {
      level?: 'critical' | 'important' | 'generated' | 'temporary';
      expiresIn?: number;
      persistent?: boolean;
    } = {}
  ): Promise<void> {
    const { level = 'important', expiresIn, persistent = true } = options;

    // Si no es persistente, usar memoria
    if (!persistent) {
      this.setTemporarySecret(key, value, expiresIn || 3600000);
      return;
    }

    switch (level) {
      case 'critical':
        return this.setCriticalSecret(key, value, expiresIn);
      case 'important':
        return this.setImportantSecret(key, value, expiresIn);
      case 'generated':
        return this.setGeneratedSecret(key, value, expiresIn);
      case 'temporary':
        this.setTemporarySecret(key, value, expiresIn || 3600000);
        return;
      default:
        throw new Error(`Nivel de seguridad inválido: ${level}`);
    }
  }

  public async getSecret(key: string): Promise<string | null> {
    const metadata = this.metadata.get(key);
    if (!metadata) {
      return null;
    }

    switch (metadata.method) {
      case 'safeStorage':
        if (metadata.level === 'critical') {
          return this.getCriticalSecret(key);
        } else {
          return this.getImportantSecret(key);
        }
      case 'encrypted':
        return this.getGeneratedSecret(key);
      case 'memory':
        return this.getTemporarySecret(key);
      default:
        return null;
    }
  }

  public async deleteSecret(key: string): Promise<boolean> {
    const metadata = this.metadata.get(key);
    if (!metadata) {
      return false;
    }

    switch (metadata.method) {
      case 'safeStorage':
        if (metadata.level === 'critical') {
          return this.deleteCriticalSecret(key);
        } else {
          return this.deleteImportantSecret(key);
        }
      case 'encrypted':
        return this.deleteGeneratedSecret(key);
      case 'memory':
        return this.deleteTemporarySecret(key);
      default:
        return false;
    }
  }

  // ============================================
  // UTILITIES Y MANTENIMIENTO
  // ============================================

  public async listSecrets(): Promise<SecretMetadata[]> {
    return Array.from(this.metadata.values());
  }

  public async generateSecureKey(length: number = 32): Promise<string> {
    return crypto.randomBytes(length).toString('hex');
  }

  public async generateJWTSecret(): Promise<string> {
    const secret = await this.generateSecureKey(64);
    await this.setSecret('jwt_secret', secret, { level: 'critical' });
    return secret;
  }

  public async generateEncryptionKey(): Promise<string> {
    const key = await this.generateSecureKey(32);
    await this.setSecret('encryption_key', key, { level: 'critical' });
    return key;
  }

  public async rotateMasterKey(): Promise<void> {
    console.log('🔄 Iniciando rotación de clave maestra...');
    
    // Re-encriptar todos los secretos generados con nueva clave maestra
    const generatedSecrets = Array.from(this.metadata.entries())
      .filter(([_, metadata]) => metadata.method === 'encrypted');

    for (const [key, _] of generatedSecrets) {
      const value = await this.getGeneratedSecret(key);
      if (value) {
        await this.setGeneratedSecret(key, value, undefined);
      }
    }
    
    console.log('✅ Rotación de clave maestra completada');
  }

  private async performMaintenanceTasks(): Promise<void> {
    // Limpiar secretos expirados
    const now = new Date();
    const expiredKeys: string[] = [];

    for (const [key, metadata] of this.metadata.entries()) {
      if (metadata.expiresAt && metadata.expiresAt < now) {
        expiredKeys.push(key);
      }
    }

    for (const key of expiredKeys) {
      await this.deleteSecret(key);
      console.log(`🧹 Secret expirado '${key}' eliminado`);
    }

    // Limpiar archivos huérfanos
    await this.cleanupOrphanedFiles();
  }

  private async cleanupOrphanedFiles(): Promise<void> {
    try {
      const files = await fs.readdir(this.secretsDir);
      const knownFiles = new Set([
        'metadata.enc',
        ...Array.from(this.metadata.entries()).map(([key, metadata]) => {
          switch (metadata.method) {
            case 'safeStorage':
              return metadata.level === 'critical' ? `critical_${key}.enc` : `${key}.enc`;
            case 'encrypted':
              return `gen_${key}.enc`;
            default:
              return null;
          }
        }).filter(Boolean)
      ]);

      for (const file of files) {
        if (!knownFiles.has(file) && file.endsWith('.enc')) {
          const filePath = path.join(this.secretsDir, file);
          await fs.unlink(filePath);
          console.log(`🧹 Archivo huérfano eliminado: ${file}`);
        }
      }
    } catch (error) {
      console.warn('⚠️ Error limpiando archivos huérfanos:', error);
    }
  }

  // ============================================
  // ENCRIPTACIÓN PERSONALIZADA
  // ============================================

  private encryptWithMasterKey(text: string): string {
    const iv = crypto.randomBytes(16);
    const cipher = crypto.createCipher('aes-256-cbc', this.masterKey);
    
    let encrypted = cipher.update(text, 'utf8', 'hex');
    encrypted += cipher.final('hex');
    
    return iv.toString('hex') + ':' + encrypted;
  }

  private decryptWithMasterKey(encryptedText: string): string {
    const parts = encryptedText.split(':');
    const iv = Buffer.from(parts[0], 'hex');
    const encrypted = parts[1];
    
    const decipher = crypto.createDecipher('aes-256-cbc', this.masterKey);
    
    let decrypted = decipher.update(encrypted, 'hex', 'utf8');
    decrypted += decipher.final('utf8');
    
    return decrypted;
  }

  // ============================================
  // GESTIÓN DE METADATA
  // ============================================

  private async loadMetadata(): Promise<void> {
    try {
      if (await fs.access(this.metadataFile).then(() => true).catch(() => false)) {
        const encryptedMetadata = await fs.readFile(this.metadataFile, 'utf8');
        const decryptedMetadata = this.decryptWithMasterKey(encryptedMetadata);
        const metadataArray = JSON.parse(decryptedMetadata);
        
        this.metadata.clear();
        for (const item of metadataArray) {
          this.metadata.set(item.id, {
            ...item,
            createdAt: new Date(item.createdAt),
            lastAccessed: item.lastAccessed ? new Date(item.lastAccessed) : undefined,
            expiresAt: item.expiresAt ? new Date(item.expiresAt) : undefined
          });
        }
      }
    } catch (error) {
      console.warn('⚠️ No se pudo cargar metadata, iniciando limpio:', error);
      this.metadata.clear();
    }
  }

  private async saveMetadata(): Promise<void> {
    try {
      const metadataArray = Array.from(this.metadata.values());
      const jsonMetadata = JSON.stringify(metadataArray);
      const encryptedMetadata = this.encryptWithMasterKey(jsonMetadata);
      await fs.writeFile(this.metadataFile, encryptedMetadata, 'utf8');
    } catch (error) {
      console.error('❌ Error guardando metadata:', error);
    }
  }

  // ============================================
  // BACKUP Y RESTORE
  // ============================================

  public async createBackup(): Promise<string> {
    const backupData = {
      metadata: Array.from(this.metadata.entries()),
      timestamp: new Date().toISOString(),
      version: '1.0.0'
    };

    const backupJson = JSON.stringify(backupData);
    const encrypted = this.encryptWithMasterKey(backupJson);
    
    const backupPath = path.join(this.secretsDir, `backup_${Date.now()}.enc`);
    await fs.writeFile(backupPath, encrypted, 'utf8');
    
    console.log(`💾 Backup creado: ${backupPath}`);
    return backupPath;
  }

  public async restoreFromBackup(backupPath: string): Promise<void> {
    try {
      const encrypted = await fs.readFile(backupPath, 'utf8');
      const decrypted = this.decryptWithMasterKey(encrypted);
      const backupData = JSON.parse(decrypted);
      
      // Validar backup
      if (!backupData.metadata || !backupData.timestamp) {
        throw new Error('Formato de backup inválido');
      }
      
      // Restaurar metadata
      this.metadata.clear();
      for (const [key, metadata] of backupData.metadata) {
        this.metadata.set(key, {
          ...metadata,
          createdAt: new Date(metadata.createdAt),
          lastAccessed: metadata.lastAccessed ? new Date(metadata.lastAccessed) : undefined,
          expiresAt: metadata.expiresAt ? new Date(metadata.expiresAt) : undefined
        });
      }
      
      await this.saveMetadata();
      console.log(`✅ Backup restaurado desde: ${backupPath}`);
      
    } catch (error) {
      console.error('❌ Error restaurando backup:', error);
      throw error;
    }
  }

  // ============================================
  // CLEANUP AL CERRAR LA APLICACIÓN
  // ============================================

  public async cleanup(): Promise<void> {
    try {
      await this.saveMetadata();
      
      // Limpiar secretos en memoria
      this.memorySecrets.clear();
      
      // Crear backup automático
      await this.createBackup();
      
      console.log('✅ GescoSecretsManager cleanup completado');
    } catch (error) {
      console.error('❌ Error durante cleanup:', error);
    }
  }

  // ============================================
  // MÉTODOS DE UTILIDAD PARA LA APLICACIÓN
  // ============================================

  public async setupApplicationSecrets(): Promise<void> {
    console.log('🔧 Configurando secretos de aplicación...');

    // Generar o recuperar JWT secret
    let jwtSecret = await this.getSecret('jwt_secret');
    if (!jwtSecret) {
      jwtSecret = await this.generateJWTSecret();
      console.log('🔑 JWT secret generado');
    }

    // Generar o recuperar encryption key
    let encryptionKey = await this.getSecret('encryption_key');
    if (!encryptionKey) {
      encryptionKey = await this.generateEncryptionKey();
      console.log('🔐 Encryption key generada');
    }

    console.log('✅ Secretos de aplicación configurados');
  }

  public async getJWTSecret(): Promise<string> {
    const secret = await this.getSecret('jwt_secret');
    if (!secret) {
      return await this.generateJWTSecret();
    }
    return secret;
  }

  public async getEncryptionKey(): Promise<string> {
    const key = await this.getSecret('encryption_key');
    if (!key) {
      return await this.generateEncryptionKey();
    }
    return key;
  }
}