using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DuplicateFinder.Core
{
    public static class HashTools
    {
        private static readonly HashAlgorithm HashAlgorithm = new MD5CryptoServiceProvider();
        private static readonly Random Random = new Random(42);
        private static readonly List<ulong> Salts = new List<ulong>();

        public static ulong GetLongHash(string input, int saltId = 0)
        {
            while (saltId >= Salts.Count)
            {
                var bytes = new byte[8];
                Random.NextBytes(bytes);
                Salts.Add(BitConverter.ToUInt64(bytes, 0));
            }

            var data = Encoding.UTF8.GetBytes(input)
                .Concat(BitConverter.GetBytes(Salts[saltId]))
                .ToArray();

            var hash = HashAlgorithm.ComputeHash(data);

            return BitConverter.ToUInt64(hash, 0);
        }

        public static Dictionary<ulong, string[]> HashValues(Dictionary<string, string> dict)
        {
            var hashes = new List<(ulong, string)>();

            foreach (var (key, value) in dict)
            {
                var hash = GetLongHash(value);

                hashes.Add((hash, key));
            }

            return hashes
                .GroupBy(x => x.Item1)
                .ToDictionary(x => x.Key, g => g.Select(x => x.Item2).ToArray());
        }
    }
}
