using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces.AuthServices
{
    /// <summary>
    /// Hashes and verifies passwords. AuthService depends only on this interface,
    /// so the algorithm can be swapped in Program.cs without touching AuthService.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>Short name of the algorithm, also used as the DI key ("SHA256", "Argon2id").</summary>
        string Algorithm { get; }

        /// <summary>Hashes a password with a new random salt. Returns one string to store.</summary>
        string Hash(string password);

        /// <summary>Checks a password against a stored value. Returns false, never throws, on bad input.</summary>
        bool Verify(string password, string storedHash);
    }
}
