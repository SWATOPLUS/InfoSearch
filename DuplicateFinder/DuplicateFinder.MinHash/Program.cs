using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DuplicateFinder.Core;
using MoreLinq;

namespace DuplicateFinder.MinHash
{
    internal static class Program
    {
        private const string InputFile = "../../assets/sources-clean.json";
        private const string OutputFile = "../../assets/compare-result.json";
        private const string GroundTruthFile = "../../assets/ground_truth.tsv";

        private static void Main()
        {
            var documents = DirectoryTools.ReadStringDictionary(InputFile);
            var groundTruth = DuplicateRates.FromLines(File.ReadLines(GroundTruthFile), documents.Keys);
            var size = documents.Keys.Count;
            var duplicates1 = FindDuplicates(documents, 1);
            var duplicates5 = FindDuplicates(documents, 5);
            var duplicates10 = FindDuplicates(documents, 10);
            //var duplicates50 = FindDuplicates(documents, 50);
            //var duplicates100 = FindDuplicates(documents, 100);
            
            var result = new
            {
                DummyRate = DuplicateRates.Compare(groundTruth, DuplicateRates.Dummy, size),
                Rate1 = DuplicateRates.Compare(groundTruth, duplicates1, size),
                Rate5 = DuplicateRates.Compare(groundTruth, duplicates5, size),
                Rate10 = DuplicateRates.Compare(groundTruth, duplicates10, size),
                //Rate50 = DuplicateRates.Compare(groundTruth, duplicates50, size),
                //Rate100 = DuplicateRates.Compare(groundTruth, duplicates100, size),
            };

            DirectoryTools.SaveAsJson(result, OutputFile);
        }

        private static DuplicateRates FindDuplicates(
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

        private static ulong[] GetMinHashes(string text, int hashCount)
        {
            var words = text.Split();
            var hashes = Enumerable.Range(0, hashCount)
                .Select(x => ulong.MaxValue)
                .ToArray();

            foreach (var grams in GetNGrams(words, 3))
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