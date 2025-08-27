using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gesco.Desktop.Core.Security
{
    public class DatabaseEncryption
    {
        private readonly string _encryptionKey;

        public DatabaseEncryption(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public static class SecureSettings
    {
        public static string GetSecureConnectionString()
        {
            var dbPath = Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? "data/gesco_encrypted.db";
            var password = Environment.GetEnvironmentVariable("SQLITE_PASSWORD");
            
            if (!string.IsNullOrEmpty(password))
            {
                return $"Data Source={dbPath};Password={password}";
            }
            
            return $"Data Source={dbPath}";
        }
    }
}
