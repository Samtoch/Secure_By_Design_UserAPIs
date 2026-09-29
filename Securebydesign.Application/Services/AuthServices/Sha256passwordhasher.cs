using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Securebydesign.Application.Interfaces.AuthServices;

namespace Securebydesign.Application.Services.AuthServices
{
    /// <summary>
    /// Salted SHA-256. Kept for comparison and for verifying older hashes.
    /// It is fast, so it is NOT recommended for new password storage.
    /// Stored format: "salt:hash" (both hex).
    /// </summary>
    public sealed class Sha256PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;   // 128-bit salt

        public string Algorithm => "SHA256";

        public string Hash(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            // Create a random 16-byte salt, unique for every call
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Hash salt + password with SHA-256
            byte[] hash = ComputeHash(password, salt);

            // Return salt and hash together so the salt can be reused to verify
            return $"{Convert.ToHexStringLower(salt)}:{Convert.ToHexStringLower(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            if (password is null || string.IsNullOrEmpty(storedHash))
                return false;

            string[] parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromHexString(parts[0]);
                byte[] expected = Convert.FromHexString(parts[1]);
                byte[] actual = ComputeHash(password, salt);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;   // stored salt or hash isn't valid hex
            }
        }

        private static byte[] ComputeHash(string password, byte[] salt) =>
            SHA256.HashData([.. salt, .. Encoding.UTF8.GetBytes(password)]);
    }
}
