using System;
using System.Security.Cryptography;
using System.Text;

namespace PHARMA.Common
{
    /// <summary>
    /// PBKDF2 password hashing (.NET Framework 4.5 / C# 5).
    /// Stored format: PBKDF2$v1${iterations}${base64salt}${base64hash}
    /// </summary>
    public static class PasswordHasher
    {
        public const string Prefix = "PBKDF2$v1$";
        private const int DefaultIterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string CreateHash(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Pbkdf2(password, salt, DefaultIterations, HashSize);
            return string.Format(
                "PBKDF2$v1${0}${1}${2}",
                DefaultIterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool IsHashedFormat(string stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            return stored.StartsWith(Prefix, StringComparison.Ordinal);
        }

        public static bool VerifyPassword(string password, string stored)
        {
            if (password == null || string.IsNullOrEmpty(stored))
                return false;

            if (!IsHashedFormat(stored))
                return false;

            string[] parts = stored.Split('$');
            if (parts.Length != 5)
                return false;
            if (!string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal))
                return false;
            if (!string.Equals(parts[1], "v1", StringComparison.Ordinal))
                return false;

            int iterations;
            if (!int.TryParse(parts[2], out iterations) || iterations < 1000)
                return false;

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[3]);
                expected = Convert.FromBase64String(parts[4]);
            }
            catch
            {
                return false;
            }

            if (salt.Length == 0 || expected.Length == 0)
                return false;

            byte[] actual = Pbkdf2(password, salt, iterations, expected.Length);
            return FixedTimeEquals(actual, expected);
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return derive.GetBytes(outputBytes);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
