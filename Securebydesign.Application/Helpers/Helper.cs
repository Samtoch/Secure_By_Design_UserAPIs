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

        public static string ComputeStringToSha256Hash(string plainText)
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
    }
}
