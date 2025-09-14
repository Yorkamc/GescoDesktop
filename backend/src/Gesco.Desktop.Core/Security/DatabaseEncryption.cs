using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Gesco.Desktop.Core.Security
{
    /// <summary>
    /// Servicio mejorado de encriptación para la base de datos
    /// </summary>
    public class DatabaseEncryption : IDisposable
    {
        private readonly ILogger<DatabaseEncryption> _logger;
        private readonly byte[] _encryptionKey;
        private bool _disposed = false;

        public DatabaseEncryption(ILogger<DatabaseEncryption> logger)
        {
            _logger = logger;
            _encryptionKey = GetOrCreateEncryptionKey();
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
        /// Encripta un string usando AES-256-GCM (más seguro que CBC)
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
                    aes.Mode = CipherMode.GCM; // Más seguro que CBC
                    
                    // Generar IV aleatorio para cada encriptación
                    var iv = new byte[12]; // GCM recomendado: 12 bytes
                    RandomNumberGenerator.Fill(iv);
                    
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    var cipherBytes = new byte[plainBytes.Length];
                    var tag = new byte[16]; // Authentication tag para GCM

                    using (var cipher = aes.CreateEncryptor())
                    {
                        if (cipher is not AesGcm gcm)
                        {
                            // Fallback a CBC si GCM no está disponible
                            return EncryptStringCBC(plainText);
                        }
                        
                        gcm.Encrypt(iv, plainBytes, cipherBytes, tag);
                    }

                    // Combinar IV + Tag + Datos encriptados
                    var result = new byte[iv.Length + tag.Length + cipherBytes.Length];
                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(tag, 0, result, iv.Length, tag.Length);
                    Buffer.BlockCopy(cipherBytes, 0, result, iv.Length + tag.Length, cipherBytes.Length);

                    return Convert.ToBase64String(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error encriptando string");
                throw new InvalidOperationException("Error en encriptación", ex);
            }
        }

        /// <summary>
        /// Desencripta un string usando AES-256-GCM
        /// </summary>
        public string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText);
                
                if (cipherBytes.Length < 28) // IV(12) + Tag(16) = 28 bytes mínimo
                {
                    // Podría ser formato CBC legacy, intentar desencriptar
                    return DecryptStringCBC(cipherText);
                }

                using (var aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.Mode = CipherMode.GCM;

                    // Extraer componentes
                    var iv = new byte[12];
                    var tag = new byte[16];
                    var encrypted = new byte[cipherBytes.Length - 28];

                    Buffer.BlockCopy(cipherBytes, 0, iv, 0, 12);
                    Buffer.BlockCopy(cipherBytes, 12, tag, 0, 16);
                    Buffer.BlockCopy(cipherBytes, 28, encrypted, 0, encrypted.Length);

                    var decrypted = new byte[encrypted.Length];

                    using (var cipher = aes.CreateDecryptor())
                    {
                        if (cipher is not AesGcm gcm)
                        {
                            // Fallback a CBC
                            return DecryptStringCBC(cipherText);
                        }
                        
                        gcm.Decrypt(iv, encrypted, tag, decrypted);
                    }

                    return Encoding.UTF8.GetString(decrypted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error desencriptando string");
                throw new InvalidOperationException("Error en desencriptación", ex);
            }
        }

        /// <summary>
        /// Encriptación CBC como fallback para compatibilidad
        /// </summary>
        private string EncryptStringCBC(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // Escribir IV al inicio
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Desencriptación CBC como fallback para compatibilidad
        /// </summary>
        private string DecryptStringCBC(string cipherText)
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
                // Limpiar la clave de memoria
                if (_encryptionKey != null)
                {
                    Array.Clear(_encryptionKey, 0, _encryptionKey.Length);
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