using BCrypt.Net;

namespace Gesco.Desktop.Core.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        // Método para generar hash de prueba
        public static void GenerateTestHash()
        {
            string password = "admin123";
            string hash = HashPassword(password);
            
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine($"Verify: {VerifyPassword(password, hash)}");
        }
    }
}