using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace Gesco.Desktop.Core.Utils
{
    public static class PasswordHelper
    {
        private static readonly ILogger? _logger = null; // Opcional: agregar logging

        /// <summary>
        /// Genera un hash BCrypt para una contraseña
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                // Usar WorkFactor 12 para balance entre seguridad y rendimiento
                var hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
                
                Console.WriteLine($"Generated hash for '{password}': {hash}");
                Console.WriteLine($"Hash length: {hash.Length}");
                
                return hash;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating hash: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verifica una contraseña contra su hash BCrypt
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            {
                Console.WriteLine($"Verification failed: password or hash is null/empty");
                return false;
            }

            try
            {
                Console.WriteLine($"Verifying password: '{password}'");
                Console.WriteLine($"Against hash: {hash}");
                Console.WriteLine($"Hash length: {hash.Length}");
                
                // Verificar que el hash tenga el formato correcto de BCrypt
                if (!IsValidBCryptHash(hash))
                {
                    Console.WriteLine($"Hash format is invalid: {hash}");
                    return false;
                }

                var result = BCrypt.Net.BCrypt.Verify(password, hash);
                Console.WriteLine($"Verification result: {result}");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying password: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica si un string tiene el formato correcto de hash BCrypt
        /// </summary>
        public static bool IsValidBCryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            // Un hash BCrypt válido debe:
            // 1. Comenzar con $2a$, $2b$, $2x$, o $2y$
            // 2. Tener al menos 60 caracteres
            // 3. Seguir el patrón: $2a$rounds$salt+hash
            
            var isValidLength = hash.Length >= 60;
            var isValidPrefix = hash.StartsWith("$2a$") || 
                               hash.StartsWith("$2b$") || 
                               hash.StartsWith("$2x$") || 
                               hash.StartsWith("$2y$");
            
            var parts = hash.Split('$');
            var isValidStructure = parts.Length == 4 && !string.IsNullOrEmpty(parts[3]);
            
            Console.WriteLine($"Hash validation - Length: {isValidLength} ({hash.Length}), Prefix: {isValidPrefix}, Structure: {isValidStructure}");
            
            return isValidLength && isValidPrefix && isValidStructure;
        }

        /// <summary>
        /// Genera y verifica un hash de prueba
        /// </summary>
        public static void GenerateTestHash()
        {
            string password = "admin123";
            
            Console.WriteLine("=== BCRYPT TEST ===");
            Console.WriteLine($"Password: {password}");
            
            // Generar nuevo hash
            string hash = HashPassword(password);
            Console.WriteLine($"Generated Hash: {hash}");
            Console.WriteLine($"Hash length: {hash.Length}");
            
            // Verificar inmediatamente
            bool isValid = VerifyPassword(password, hash);
            Console.WriteLine($"Immediate verification: {isValid}");
            
            // Verificar estructura del hash
            if (IsValidBCryptHash(hash))
            {
                Console.WriteLine("✅ Hash structure is correct");
            }
            else
            {
                Console.WriteLine("❌ Hash structure is incorrect");
            }
            
            // Probar contra el hash problemático de la BD
            string problematicHash = "$2a$12$LQV.K4/OOOgwdEXCfC7jC.QLwpZ9HkqhXfOr9p6mTyYFEYGHZcP/a";
            Console.WriteLine($"\n=== TESTING PROBLEMATIC HASH ===");
            Console.WriteLine($"Problematic hash: {problematicHash}");
            Console.WriteLine($"Problematic hash length: {problematicHash.Length}");
            Console.WriteLine($"Is valid BCrypt format: {IsValidBCryptHash(problematicHash)}");
            
            bool problematicResult = VerifyPassword(password, problematicHash);
            Console.WriteLine($"Verification against problematic hash: {problematicResult}");
            
            Console.WriteLine("\n=== RECOMMENDED ACTION ===");
            Console.WriteLine("The hash in your database appears to be truncated or corrupted.");
            Console.WriteLine($"Use this new hash instead: {hash}");
        }

        /// <summary>
        /// Genera un hash correcto para admin123 (para usar en datos semilla)
        /// </summary>
        public static string GenerateAdminHash()
        {
            var hash = HashPassword("admin123");
            Console.WriteLine($"New admin hash generated: {hash}");
            return hash;
        }

        /// <summary>
        /// Verifica contraseñas contra múltiples formatos (para migración)
        /// </summary>
        public static bool VerifyPasswordMultiFormat(string password, string storedHash)
        {
            // Intentar verificación BCrypt normal
            if (VerifyPassword(password, storedHash))
            {
                return true;
            }

            // Si falla, podría ser un hash en otro formato o corrupto
            Console.WriteLine($"BCrypt verification failed for hash: {storedHash}");
            
            // Aquí podrías agregar verificación de otros formatos si es necesario
            // Por ejemplo, MD5, SHA1, etc. (NO RECOMENDADO para producción)
            
            return false;
        }
    }
}