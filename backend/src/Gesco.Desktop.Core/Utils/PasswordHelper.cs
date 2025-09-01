using BCrypt.Net;

namespace Gesco.Desktop.Core.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Usar WorkFactor 12 consistentemente
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Método para generar hash de prueba y verificar
        public static void GenerateTestHash()
        {
            string password = "admin123";
            string hash = HashPassword(password);
            bool isValid = VerifyPassword(password, hash);
            
            Console.WriteLine("=== BCRYPT TEST ===");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine($"Verify: {isValid}");
            Console.WriteLine($"Hash length: {hash.Length}");
            
            // Verificar estructura del hash
            if (hash.StartsWith("$2a$12$") || hash.StartsWith("$2b$12$"))
            {
                Console.WriteLine("✅ Hash structure is correct");
            }
            else
            {
                Console.WriteLine("❌ Hash structure is incorrect");
            }
        }

        // Método para verificar un hash específico
        public static bool TestSpecificHash(string password, string existingHash)
        {
            Console.WriteLine($"Testing password '{password}' against hash: {existingHash}");
            bool result = VerifyPassword(password, existingHash);
            Console.WriteLine($"Result: {result}");
            return result;
        }

        // Generar hash correcto para admin123 (para usar en datos semilla)
        public static string GenerateAdminHash()
        {
            return HashPassword("admin123");
        }
    }
}