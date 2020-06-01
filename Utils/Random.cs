using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace TauManager.Utils
{
    public static class Random
    {
        public static string GetRandom256ByteString()
        {
            // The length of this string is 32 chars on purpose -
            // it MUST be one of the integer divisors for 256.
            string randomChars = "ABCDEFGHJKLMNPQRSTUVWXYZ01234567";
            RandomNumberGenerator r = RandomNumberGenerator.Create();
            byte[] randomBytes = new Byte[256];
            r.GetBytes(randomBytes);

            var resultString = "";

            List<char> chars = new List<char>();
            foreach (var b in randomBytes)
            {
                chars.Add(randomChars[b % randomChars.Length]);
            }
            resultString = new string(chars.ToArray());

            return resultString;
        }

    }
}