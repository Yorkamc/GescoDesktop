using BCrypt.Net;

namespace Gesco.Desktop.Core.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
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

        public static bool IsValidBCryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

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

        public static void GenerateTestHash()
        {
            string password = "admin123";
            
            Console.WriteLine("=== BCRYPT TEST ===");
            Console.WriteLine($"Password: {password}");
            
            string hash = HashPassword(password);
            Console.WriteLine($"Generated Hash: {hash}");
            Console.WriteLine($"Hash length: {hash.Length}");
            
            bool isValid = VerifyPassword(password, hash);
            Console.WriteLine($"Immediate verification: {isValid}");
            
            if (IsValidBCryptHash(hash))
            {
                Console.WriteLine("Hash structure is correct");
            }
            else
            {
                Console.WriteLine("Hash structure is incorrect");
            }
            
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

        public static string GenerateAdminHash()
        {
            var hash = HashPassword("admin123");
            Console.WriteLine($"New admin hash generated: {hash}");
            return hash;
        }

        public static bool VerifyPasswordMultiFormat(string password, string storedHash)
        {
            if (VerifyPassword(password, storedHash))
            {
                return true;
            }

            Console.WriteLine($"BCrypt verification failed for hash: {storedHash}");
            
            return false;
        }
    }
}