using NLog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Securebydesign.Application.Helpers
{
    public static class Helper
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private const int SaltSize = 16; // 128-bit salt

        public static string ComputeStringToSha256Hash_(string plainText)
        {
            // Create a SHA256 hash from string   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(plainText));

                StringBuilder stringbuilder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    stringbuilder.Append(bytes[i].ToString("x2"));
                }
                return stringbuilder.ToString();
            }
        }

        public static string ComputeStringToSha256Hash(string plainText)
        {
            ArgumentNullException.ThrowIfNull(plainText);
            
            //string saltText = "SEC1@DESIGN7CODE";
            //byte[] saltBytes = Encoding.UTF8.GetBytes(saltText);

            // Create a random 16-byte salt, unique for every call RandomNumberGenerator.GetBytes(16);
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Hash salt + plaintext with SHA-256
            byte[] hash = SHA256.HashData([.. salt, .. Encoding.UTF8.GetBytes(plainText)]);

            // Return salt and hash together so the salt can be reused to verify
            return $"{Convert.ToHexStringLower(salt)}:{Convert.ToHexStringLower(hash)}";
        }

        public static bool VerifySha256Hash(string plainText, string storedValue)
        {
            string[] parts = storedValue.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromHexString(parts[0]);
            byte[] expected = Convert.FromHexString(parts[1]);
            byte[] actual = SHA256.HashData([.. salt, .. Encoding.UTF8.GetBytes(plainText)]);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

    }
}
