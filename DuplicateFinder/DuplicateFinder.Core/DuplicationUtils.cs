using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;

namespace DuplicateFinder.Core
{
    public static class DuplicationUtils
    {
        public static DuplicateRates FindDuplicatesBySimHash(
            Dictionary<string, string> documents,
            int bytesCount)
        {
            var simHashes = new Dictionary<string, BitArray>();

            foreach (var (name, text) in documents)
            {
                simHashes[name] = GetSimHash(text, bytesCount);
            }

            var result = new Dictionary<(string, string), double>();

            foreach (var documentOne in documents.Keys)
            {
                foreach (var documentTwo in documents.Keys)
                {
                    if (string.Compare(documentOne, documentTwo, StringComparison.Ordinal) >= 0)
                    {
                        continue;
                    }

                    var rate = CompareBitArrays(simHashes[documentOne], simHashes[documentTwo]);

                    if (rate >= 0.02)
                    {
                        result[(documentOne, documentTwo)] = rate;
                    }
                }
            }

            return new DuplicateRates(result);
        }

        public static DuplicateRates FindDuplicatesByMinHash(
            Dictionary<string, string> documents,
            int hashCount)
        {
            var minHashes = new Dictionary<string, ulong[]>();

            foreach (var (name, text) in documents)
            {
                minHashes[name] = GetMinHashes(text, hashCount);
            }

            var candidates = minHashes
                .SelectMany(group => group.Value.Select((x, i) => (Name: group.Key, Index: i, Hash: x)))
                .GroupBy(x => (x.Hash, x.Index))
                .ToDictionary(x => x.Key, g => g.Select(x => x.Name).ToArray())
                .Where(x => x.Value.Length > 1)
                .ToDictionary(x => x.Key, x => x.Value);

            var neighbors = candidates.Values
                .SelectMany(x => x)
                .Distinct()
                .ToDictionary(x => x, x => new HashSet<string>());

            foreach (var (_, value) in candidates)
            {
                foreach (var name in value)
                {
                    foreach (var item in value)
                    {
                        if (name == item)
                        {
                            continue;
                        }

                        neighbors[name].Add(item);
                    }
                }
            }

            var result = new Dictionary<(string, string), double>();

            foreach (var (name, set) in neighbors)
            {
                foreach (var item in set)
                {
                    var rate = CompareMinVectors(minHashes[name], minHashes[item]);

                    result.Add((name, item), rate);
                }
            }

            return new DuplicateRates(result);
        }

        private static BitArray GetSimHash(string text, int byteCount)
        {
            var words = text.Split();
            var result = new int[byteCount * 8];

            foreach (var grams in GetNGrams(words, 5))
            {
                var gram = string.Join(' ', grams);
                var hash = new BitArray(HashTools.GetByteHash(gram, byteCount));

                for (var i = 0; i < hash.Length; i++)
                {
                    if (hash[i])
                    {
                        result[i]++;
                    }
                    else
                    {
                        result[i]--;
                    }
                }
            }

            var bits = result
                .Select(x => x > 0)
                .ToArray();

            return new BitArray(bits);
        }

        private static ulong[] GetMinHashes(string text, int hashCount)
        {
            var words = text.Split();
            var hashes = Enumerable.Range(0, hashCount)
                .Select(x => ulong.MaxValue)
                .ToArray();

            foreach (var grams in GetNGrams(words, 5))
            {
                var gram = string.Join(' ', grams);
                var newHashes = new ulong[hashCount];

                foreach (var i in Enumerable.Range(0, hashCount))
                {
                    newHashes[i] = HashTools.GetLongHash(gram, i);
                }

                hashes = MergeMinVectors(hashes, newHashes);
            }

            return hashes;
        }

        private static double CompareBitArrays(BitArray a, BitArray b)
        {
            var count = a.Count;
            var matched = a.Cast<bool>().Cartesian(b.Cast<bool>(), (x, y) => x == y ? 1 : 0).Sum();

            return (double)matched / count;
        }

        private static double CompareMinVectors(IReadOnlyCollection<ulong> a, IReadOnlyCollection<ulong> b)
        {
            var count = a.Count;
            var matched = a.Cartesian(b, (x, y) => x == y ? 1 : 0).Sum();

            return (double)matched / count;
        }
        
        private static ulong[] MergeMinVectors(IEnumerable<ulong> a, IEnumerable<ulong> b)
        {
            return a.Zip(b, Math.Min)
                .ToArray();
        }

        private static IEnumerable<T[]> GetNGrams<T>(IEnumerable<T> sequence, int n)
        {
            var queue = new Queue<T>();

            foreach (var item in sequence)
            {
                queue.Enqueue(item);

                if (queue.Count != n)
                {
                    continue;
                }

                yield return queue.ToArray();

                queue.Dequeue();
            }
        }
    }
}
