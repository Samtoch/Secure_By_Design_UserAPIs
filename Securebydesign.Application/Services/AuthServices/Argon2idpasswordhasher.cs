using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using Securebydesign.Application.Interfaces.AuthServices;

namespace Securebydesign.Application.Services.AuthServices
{
    /// <summary>Argon2id cost settings, bound from appsettings.json (PasswordHashing:Argon2id).</summary>
    public sealed class Argon2idOptions
    {
        public int MemorySizeKb { get; set; } = 19456;   // 19 MiB per hash (OWASP minimum)
        public int Iterations { get; set; } = 2;
        public int Parallelism { get; set; } = 1;
        public int SaltSize { get; set; } = 16;          // 128-bit salt
        public int HashSize { get; set; } = 32;          // 256-bit hash
    }

    /// <summary>
    /// Argon2id password hashing (recommended).
    /// Stored format: "$argon2id$m=19456,t=2,p=1$salt$hash" (salt and hash in hex).
    /// Requires: dotnet add package Konscious.Security.Cryptography.Argon2
    /// </summary>
    public sealed class Argon2idPasswordHasher(IOptions<Argon2idOptions> options) : IPasswordHasher
    {
        private readonly Argon2idOptions _options = options.Value;

        public string Algorithm => "Argon2id";

        public string Hash(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            // Create a random salt, unique for every call
            byte[] salt = RandomNumberGenerator.GetBytes(_options.SaltSize);

            // Hash the password with Argon2id using the salt and current cost settings
            byte[] hash = ComputeHash(password, salt,
                _options.MemorySizeKb, _options.Iterations, _options.Parallelism, _options.HashSize);

            // Store the settings with the salt and hash so verification can repeat them exactly
            return $"$argon2id$m={_options.MemorySizeKb},t={_options.Iterations},p={_options.Parallelism}" +
                   $"${Convert.ToHexStringLower(salt)}${Convert.ToHexStringLower(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            if (password is null || string.IsNullOrEmpty(storedHash))
                return false;

            // "$argon2id$m=..,t=..,p=..$salt$hash" splits into ["", "argon2id", settings, salt, hash]
            string[] parts = storedHash.Split('$');
            if (parts.Length != 5 || parts[1] != "argon2id")
                return false;

            string[] settings = parts[2].Split(',');
            if (settings.Length != 3
                || !TryReadSetting(settings[0], "m", out int memory)
                || !TryReadSetting(settings[1], "t", out int iterations)
                || !TryReadSetting(settings[2], "p", out int parallelism))
                return false;

            try
            {
                byte[] salt = Convert.FromHexString(parts[3]);
                byte[] expected = Convert.FromHexString(parts[4]);

                // Re-hash with the SAME salt and settings that were stored
                byte[] actual = ComputeHash(password, salt, memory, iterations, parallelism, expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;   // stored salt or hash isn't valid hex
            }
        }

        private static byte[] ComputeHash(string password, byte[] salt,
            int memoryKb, int iterations, int parallelism, int hashSize)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                MemorySize = memoryKb,
                Iterations = iterations,
                DegreeOfParallelism = parallelism
            };

            return argon2.GetBytes(hashSize);
        }

        private static bool TryReadSetting(string setting, string key, out int value)
        {
            value = 0;
            return setting.StartsWith(key + "=")
                && int.TryParse(setting.AsSpan(key.Length + 1), out value)
                && value > 0;
        }
    }
}
