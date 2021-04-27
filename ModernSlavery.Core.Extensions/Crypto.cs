using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ModernSlavery.Core.Extensions
{
    public static class Crypto
    {
        public static string GetPBKDF2(string password,
            byte[] salt,
            KeyDerivationPrf prfunction = KeyDerivationPrf.HMACSHA1,
            int iterationCount = 10000,
            int hashSizeInBit = 256)
        {
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    prfunction,
                    iterationCount,
                    hashSizeInBit / 8));
        }

        public static byte[] GetSalt(int saltSizeInBit = 128)
        {
            // generate a salt using a secure PRNG
            var salt = new byte[saltSizeInBit / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }


        public static string GetSHA512Checksum(string text, bool base64encode = true)
        {
            var checksumData = Encoding.UTF8.GetBytes(text);
            var hash = SHA512.Create().ComputeHash(checksumData);
            var calculatedChecksum = base64encode ? Convert.ToBase64String(hash) : Encoding.UTF8.GetString(hash);
            return calculatedChecksum;
        }

        public static string GetSHA256Checksum(this string text)
        {
            var checksumData = Encoding.UTF8.GetBytes(text);
            var hash = SHA256.Create().ComputeHash(checksumData);
            var calculatedChecksum = Convert.ToBase64String(hash);
            return calculatedChecksum;
        }

        /// <summary>
        ///  Returns the hash code for this string which will remain constant out of application domain
        /// see https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        /// <param name="text">The source text</param>
        /// <returns>A 32-bit signed integer deterministic hash code.</returns>
        public static int GetDeterministicHashCode(this string text)
        {
            unchecked //Prevents OverflowException in checked context
            {
                var hash1 = (5381 << 16) + 5381;
                var hash2 = hash1;

                for (var i = 0; i < text.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ text[i];
                    if (i == text.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ text[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static string GeneratePasscode(char[] charset, int passcodeLength)
        {
            //Ensure characters are distict and mixed up
            charset = charset.Distinct().ToList().Randomise().ToArray();

            var chars = new char[passcodeLength];

            //Generate a load of random numbers
            var randomData = new byte[chars.Length];
            using (var generator = new RNGCryptoServiceProvider())
            {
                generator.GetBytes(randomData);
            }

            //use the randome number to pick from the character set
            Parallel.For(0, chars.Length, i => { chars[i] = charset[randomData[i] % charset.Length]; });

            return new string(chars);
        }
    }
}