using System.Collections.Generic;
using System.Text;
using System;
using System.Security.Cryptography;

namespace ModernSlavery.Core.Extensions
{
    public static class Base62Converter
    {
        private const string characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static byte[] characterSetBytes = Encoding.ASCII.GetBytes(characterSet);
        private const int byteSize = byte.MaxValue + 1;
        private static int baseSize = characterSet.Length;

        public static string Encode(string value, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            if (encoding == null) encoding = Encoding.UTF8;

            var bytes = encoding.GetBytes(value);
            bytes = Encode(bytes);
            value=encoding.GetString(bytes);
            return value;
        }

        public static byte[] Encode(byte[] bytes)
        {
            bytes = BaseConvert(bytes, byteSize, baseSize);
            var builder = new List<byte>();
            for (var i = 0; i < bytes.Length; i++)
            {
                builder.Add(characterSetBytes[bytes[i]]);
            }
            return builder.ToArray();
        }

        public static string Decode(string value, Encoding encoding= null)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;

            if (encoding == null) encoding = Encoding.UTF8;

            var bytes = encoding.GetBytes(value);
            bytes = Decode(bytes);
            value = encoding.GetString(bytes);
            return value;
        }

        public static byte[] Decode(byte[] bytes)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                var index=Array.IndexOf<byte>(characterSetBytes, bytes[i]);
                if (index < 0) throw new ArgumentOutOfRangeException(nameof(bytes),$"Character #{bytes[i]} {(char)bytes[i]} is not a base{baseSize} character");
                bytes[i] = (byte)index;
            }

            bytes = BaseConvert(bytes, 62, 256);

            return bytes;
        }

        private static byte[] BaseConvert(byte[] source, int sourceBase, int targetBase)
        {
            var result = new List<int>();
            int count;
            while ((count = source.Length) > 0)
            {
                var quotient = new List<byte>();
                var remainder = 0;
                for (var i = 0; i != count; i++)
                {
                    var accumulator = source[i] + remainder * sourceBase;
                    var digit = System.Convert.ToByte((accumulator - (accumulator % targetBase)) / targetBase);
                    remainder = accumulator % targetBase;
                    if (quotient.Count > 0 || digit != 0)
                    {
                        quotient.Add(digit);
                    }
                }

                result.Insert(0, remainder);
                source = quotient.ToArray();
            }

            var output = new byte[result.Count];
            for (var i = 0; i < result.Count; i++)
                output[i] = (byte)result[i];

            return output;
        }
    }
}
