using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // AGREGADO: Para ExecuteSqlRaw

namespace Gesco.Desktop.Core.Security
{
    /// <summary>
    /// Servicio mejorado de encriptación para la base de datos
    /// Usa AES-256-CBC con HMAC-SHA256 para autenticación
    /// </summary>
    public class DatabaseEncryption : IDisposable
    {
        private readonly ILogger<DatabaseEncryption> _logger;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _hmacKey;
        private bool _disposed = false;

        public DatabaseEncryption(ILogger<DatabaseEncryption> logger)
        {
            _logger = logger;
            var masterKey = GetOrCreateEncryptionKey();
            
            // Derivar claves separadas para encriptación y HMAC
            (_encryptionKey, _hmacKey) = DeriveKeys(masterKey);
        }

        /// <summary>
        /// Obtiene o crea la clave de encriptación principal
        /// </summary>
        private byte[] GetOrCreateEncryptionKey()
        {
            // 1. Intentar desde variable de entorno
            var envKey = Environment.GetEnvironmentVariable("SQLITE_ENCRYPTION_KEY");
            if (!string.IsNullOrEmpty(envKey))
            {
                try
                {
                    var keyBytes = Convert.FromBase64String(envKey);
                    if (keyBytes.Length == 32) // AES-256 requiere 32 bytes
                    {
                        _logger.LogInformation("🔑 Clave de encriptación cargada desde variable de entorno");
                        return keyBytes;
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Clave de encriptación en variable de entorno tiene tamaño incorrecto");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error procesando clave de encriptación desde variable de entorno");
                }
            }

            // 2. Generar clave determinística basada en el sistema (fallback)
            _logger.LogWarning("⚠️ Generando clave de encriptación basada en características del sistema");
            return GenerateDeterministicKey();
        }

        /// <summary>
        /// Genera una clave determinística basada en características únicas del sistema
        /// </summary>
        private byte[] GenerateDeterministicKey()
        {
            var keyMaterial = new StringBuilder();
            
            // Usar características únicas del sistema
            keyMaterial.Append(Environment.MachineName);
            keyMaterial.Append(Environment.UserName);
            keyMaterial.Append(Environment.OSVersion.ToString());
            keyMaterial.Append(Environment.ProcessorCount);
            
            // Agregar ruta de instalación para mayor uniqueness
            try
            {
                keyMaterial.Append(Environment.CurrentDirectory);
            }
            catch
            {
                keyMaterial.Append("fallback_path");
            }
            
            // Agregar una sal constante específica de GESCO
            keyMaterial.Append("GESCO_DESKTOP_2024_ENCRYPTION_SALT");

            // Derivar clave usando PBKDF2
            using (var rfc2898 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(keyMaterial.ToString()),
                Encoding.UTF8.GetBytes("GESCO_SALT_2024"),
                100000, // 100,000 iteraciones
                HashAlgorithmName.SHA256))
            {
                return rfc2898.GetBytes(32); // 256 bits para AES-256
            }
        }

        /// <summary>
        /// Deriva claves separadas para encriptación y HMAC desde la clave maestra
        /// </summary>
        private (byte[] encryptionKey, byte[] hmacKey) DeriveKeys(byte[] masterKey)
        {
            using (var kdf = new Rfc2898DeriveBytes(masterKey, Encoding.UTF8.GetBytes("GESCO_DERIVE_SALT"), 10000, HashAlgorithmName.SHA256))
            {
                var encKey = kdf.GetBytes(32); // 256 bits para AES
                var hmacKey = kdf.GetBytes(32); // 256 bits para HMAC-SHA256
                return (encKey, hmacKey);
            }
        }

        /// <summary>
        /// Encripta un string usando AES-256-CBC con HMAC-SHA256 para autenticación
        /// </summary>
        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.Mode = CipherMode.CBC; // Usar CBC que es ampliamente compatible
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateIV(); // Generar IV aleatorio para cada encriptación

                    var plainBytes = Encoding.UTF8.GetBytes(plainText);

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        // Escribir IV al inicio
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                        }
                        
                        var encryptedData = ms.ToArray();
                        
                        // Calcular HMAC para autenticación
                        var hmac = ComputeHmac(encryptedData);
                        
                        // Combinar HMAC + datos encriptados
                        var result = new byte[hmac.Length + encryptedData.Length];
                        Buffer.BlockCopy(hmac, 0, result, 0, hmac.Length);
                        Buffer.BlockCopy(encryptedData, 0, result, hmac.Length, encryptedData.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error encriptando string");
                throw new InvalidOperationException("Error en encriptación", ex);
            }
        }

        /// <summary>
        /// Desencripta un string usando AES-256-CBC con verificación HMAC
        /// </summary>
        public string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText);
                
                if (cipherBytes.Length < 48) // HMAC(32) + IV(16) = 48 bytes mínimo
                {
                    // Podría ser formato legacy, intentar desencriptar con método CBC simple
                    return DecryptStringLegacy(cipherText);
                }

                // Extraer HMAC y datos encriptados
                var hmac = new byte[32]; // SHA256 = 32 bytes
                var encryptedData = new byte[cipherBytes.Length - 32];
                
                Buffer.BlockCopy(cipherBytes, 0, hmac, 0, 32);
                Buffer.BlockCopy(cipherBytes, 32, encryptedData, 0, encryptedData.Length);

                // Verificar HMAC
                var computedHmac = ComputeHmac(encryptedData);
                if (!AreEqual(hmac, computedHmac))
                {
                    throw new InvalidOperationException("HMAC verification failed - data may be corrupted");
                }

                using (var aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Extraer IV (primeros 16 bytes de los datos encriptados)
                    var iv = new byte[16];
                    Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
                    aes.IV = iv;

                    // Extraer datos encriptados (resto de los bytes)
                    var encrypted = new byte[encryptedData.Length - 16];
                    Buffer.BlockCopy(encryptedData, 16, encrypted, 0, encrypted.Length);

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(encrypted))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error desencriptando string");
                throw new InvalidOperationException("Error en desencriptación", ex);
            }
        }

        /// <summary>
        /// Desencriptación legacy para compatibilidad con datos antiguos
        /// </summary>
        private string DecryptStringLegacy(string cipherText)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extraer IV
                var iv = new byte[16];
                Buffer.BlockCopy(cipherBytes, 0, iv, 0, 16);
                aes.IV = iv;

                // Extraer datos encriptados
                var encrypted = new byte[cipherBytes.Length - 16];
                Buffer.BlockCopy(cipherBytes, 16, encrypted, 0, encrypted.Length);

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(encrypted))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Calcula HMAC-SHA256 para autenticación
        /// </summary>
        private byte[] ComputeHmac(byte[] data)
        {
            using (var hmac = new HMACSHA256(_hmacKey))
            {
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>
        /// Comparación segura de arrays para evitar timing attacks
        /// </summary>
        private static bool AreEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            
            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }

        /// <summary>
        /// Genera un hash seguro para verificación de integridad
        /// </summary>
        public string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifica la integridad de datos
        /// </summary>
        public bool VerifyHash(string input, string expectedHash)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(expectedHash))
                return false;

            var computedHash = ComputeHash(input);
            return string.Equals(computedHash, expectedHash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Genera una nueva clave de encriptación segura
        /// </summary>
        public static string GenerateNewEncryptionKey()
        {
            var key = new byte[32]; // 256 bits
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// Valida que una clave de encriptación tenga el formato correcto
        /// </summary>
        public static bool ValidateEncryptionKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            try
            {
                var keyBytes = Convert.FromBase64String(key);
                return keyBytes.Length == 32; // 256 bits
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Limpiar las claves de memoria
                if (_encryptionKey != null)
                {
                    Array.Clear(_encryptionKey, 0, _encryptionKey.Length);
                }
                
                if (_hmacKey != null)
                {
                    Array.Clear(_hmacKey, 0, _hmacKey.Length);
                }
                
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Configuración segura para SQLite
    /// </summary>
    public static class SecureSettings
    {
        /// <summary>
        /// Obtiene la cadena de conexión segura para SQLite
        /// </summary>
        public static string GetSecureConnectionString()
        {
            var dbPath = Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? 
                        Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_encrypted.db");
            
            var password = Environment.GetEnvironmentVariable("SQLITE_PASSWORD");
            
            // Crear directorio si no existe
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Si hay password, usar SQLite con encriptación nativa
            if (!string.IsNullOrEmpty(password))
            {
                return $"Data Source={dbPath};Password={password};Cache=Shared;Foreign Keys=True;";
            }
            
            // Conexión sin password (encriptación a nivel de aplicación)
            return $"Data Source={dbPath};Cache=Shared;Foreign Keys=True;";
        }

        /// <summary>
        /// Configuraciones de seguridad adicionales para SQLite
        /// </summary>
        public static void ApplySecuritySettings(Microsoft.EntityFrameworkCore.DbContext context)
        {
            // Configuraciones de seguridad para SQLite
            var commands = new[]
            {
                "PRAGMA foreign_keys = ON;",           // Habilitar foreign keys
                "PRAGMA journal_mode = WAL;",         // WAL mode para mejor concurrencia
                "PRAGMA synchronous = NORMAL;",       // Balance entre seguridad y performance
                "PRAGMA temp_store = MEMORY;",        // Almacenar temporales en memoria
                "PRAGMA secure_delete = ON;",         // Sobrescribir datos eliminados
                "PRAGMA auto_vacuum = INCREMENTAL;"   // Vacuum automático
            };

            foreach (var command in commands)
            {
                try
                {
                    context.Database.ExecuteSqlRaw(command);
                }
                catch (Exception ex)
                {
                    // Log warning pero continuar
                    Console.WriteLine($"⚠️ Warning applying security setting: {ex.Message}");
                }
            }
        }
    }
}