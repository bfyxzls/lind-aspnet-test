using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestRabbitMq
{
    class Totp3
    {
        public static string GenerateTOTP(string base32Key, int timeStep, int digits)
        {
            long counter = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds / timeStep;
            byte[] key = Base32Decode(base32Key);

            using (HMACSHA1 hmac = new HMACSHA1(key))
            {
                byte[] counterBytes = BitConverter.GetBytes(counter);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(counterBytes);
                }

                byte[] hash = hmac.ComputeHash(counterBytes);
                int offset = hash[hash.Length - 1] & 0x0F;
                int binary = (hash[offset] & 0x7F) << 24 | (hash[offset + 1] & 0xFF) << 16 | (hash[offset + 2] & 0xFF) << 8 | (hash[offset + 3] & 0xFF);

                int otp = binary % (int)Math.Pow(10, digits);
                return otp.ToString($"D{digits}");
            }
        }

        private static byte[] Base32Decode(string base32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bits = base32.ToUpper().ToCharArray().Select(c => Convert.ToString(chars.IndexOf(c), 2).PadLeft(5, '0')).Aggregate((a, b) => a + b);
            return Enumerable.Range(0, bits.Length / 8).Select(i => Convert.ToByte(bits.Substring(i * 8, 8), 2)).ToArray();
        }
    }
}
