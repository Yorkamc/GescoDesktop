import { safeStorage, app } from 'electron';
import * as keytar from 'keytar';
import * as crypto from 'crypto';
import * as fs from 'fs/promises';
import * as path from 'path';

interface SecretMetadata {
  id: string;
  level: 'critical' | 'important' | 'generated';
  method: 'keytar' | 'safeStorage' | 'encrypted';
  createdAt: Date;
  lastAccessed?: Date;
  expiresAt?: Date;
}

export class GescoSecretsManager {
  private static instance: GescoSecretsManager;
  private readonly serviceName = 'GESCO-Desktop';
  private readonly secretsDir: string;
  private readonly metadataFile: string;
  private readonly masterKey: Buffer;
  private metadata: Map<string, SecretMetadata> = new Map();

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
    try {
      await fs.mkdir(this.secretsDir, { recursive: true });
      await this.loadMetadata();
      await this.performMaintenanceTasks();
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
    return hash.digest();
  }

  // ============================================
  // GESTIÓN DE SECRETOS CRÍTICOS (KEYTAR)
  // ============================================

  public async setCriticalSecret(key: string, value: string, expiresIn?: number): Promise<void> {
    try {
      // Usar keytar para secretos críticos (PostgreSQL, API keys, etc.)
      await keytar.setPassword(this.serviceName, key, value);
      
      const metadata: SecretMetadata = {
        id: key,
        level: 'critical',
        method: 'keytar',
        createdAt: new Date(),
        expiresAt: expiresIn ? new Date(Date.now() + expiresIn) : undefined
      };
      
      this.metadata.set(key, metadata);
      await this.saveMetadata();
      
      console.log(`🔐 Secret crítico '${key}' almacenado en OS keychain`);
    } catch (error) {
      console.error(`❌ Error almacenando secret crítico '${key}':`, error);
      throw error;
    }
  }

  public async getCriticalSecret(key: string): Promise<string | null> {
    try {
      const metadata = this.metadata.get(key);
      if (!metadata || metadata.method !== 'keytar') {
        return null;
      }

      // Verificar expiración
      if (metadata.expiresAt && metadata.expiresAt < new Date()) {
        await this.deleteCriticalSecret(key);
        return null;
      }

      const value = await keytar.getPassword(this.serviceName, key);
      
      // Actualizar último acceso
      metadata.lastAccessed = new Date();
      await this.saveMetadata();
      
      return value;
    } catch (error) {
      console.error(`❌ Error obteniendo secret crítico '${key}':`, error);
      return null;
    }
  }

  public async deleteCriticalSecret(key: string): Promise<boolean> {
    try {
      const result = await keytar.deletePassword(this.serviceName, key);
      this.metadata.delete(key);
      await this.saveMetadata();
      return result;
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
      
      // Almacenar en archivo local encriptado
      const secretPath = path.join(this.secretsDir, `${key}.enc`);
      await fs.writeFile(secretPath, encodedValue, 'utf8');
      
      const metadata: SecretMetadata = {
        id: key,
        level: 'important',
        method: 'safeStorage',
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
      if (!metadata || metadata.method !== 'safeStorage') {
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
  // INTERFAZ UNIFICADA
  // ============================================

  public async setSecret(key: string, value: string, level: 'critical' | 'important' | 'generated' = 'important', expiresIn?: number): Promise<void> {
    switch (level) {
      case 'critical':
        return this.setCriticalSecret(key, value, expiresIn);
      case 'important':
        return this.setImportantSecret(key, value, expiresIn);
      case 'generated':
        return this.setGeneratedSecret(key, value, expiresIn);
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
      case 'keytar':
        return this.getCriticalSecret(key);
      case 'safeStorage':
        return this.getImportantSecret(key);
      case 'encrypted':
        return this.getGeneratedSecret(key);
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
      case 'keytar':
        return this.deleteCriticalSecret(key);
      case 'safeStorage':
        return this.deleteImportantSecret(key);
      case 'encrypted':
        return this.deleteGeneratedSecret(key);
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

  public async rotateMasterKey(): Promise<void> {
    // Re-encriptar todos los secretos generados con nueva clave maestra
    const generatedSecrets = Array.from(this.metadata.entries())
      .filter(([_, metadata]) => metadata.method === 'encrypted');

    for (const [key, _] of generatedSecrets) {
      const value = await this.getGeneratedSecret(key);
      if (value) {
        await this.setGeneratedSecret(key, value, undefined);
      }
    }
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
  // CLEANUP AL CERRAR LA APLICACIÓN
  // ============================================

  public async cleanup(): Promise<void> {
    try {
      await this.saveMetadata();
      console.log('✅ GescoSecretsManager cleanup completado');
    } catch (error) {
      console.error('❌ Error durante cleanup:', error);
    }
  }
}