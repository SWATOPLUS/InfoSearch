using System;
using System.Collections.Generic;
using System.Linq;
using DuplicateFinder.Core.Hashers;
using MoreLinq;

namespace DuplicateFinder.Core.Analyzers
{
    public class HashDuplicateAnalyzer<T> : IDuplicateAnalyzer
    {
        private readonly IDuplicateHasher<T> _hasher;
        private readonly Dictionary<string, T[]> _hashBase = new();
        private readonly Dictionary<(T, int), List<string>> _hashGroups = new();

        public HashDuplicateAnalyzer(IDuplicateHasher<T> hasher)
        {
            _hasher = hasher;
        }

        public void AddText(string name, string text)
        {
            var hashes = _hasher.Hash(text);

            _hashBase[name] = hashes;

            foreach (var group in hashes.Select((x, i) => (x, i)))
            {
                if (!_hashGroups.ContainsKey(group))
                {
                    _hashGroups[group] = new List<string>();
                }

                _hashGroups[group].Add(name);
            }
        }

        public DuplicateRates GetAllDuplicates()
        {
            var pairs = _hashGroups.Values
                .SelectMany(g => g.Cartesian(g, (x, y) => (x, y)))
                .Where(pair => string.Compare(pair.x, pair.y, StringComparison.Ordinal) < 0)
                .Distinct()
                .Select(pair => new KeyValuePair<(string, string), double>(
                    (pair.x, pair.y),
                    _hashBase[pair.x].GetSimilarityRate(_hashBase[pair.y])));

            return new DuplicateRates(pairs);
        }

        public KeyValuePair<string, double>[] Analyze(string text)
        {
            var hashes = _hasher.Hash(text);

            return hashes
                .SelectMany((x, i) => _hashGroups[(x, i)])
                .Distinct()
                .Select(x => new KeyValuePair<string, double>(x, _hashBase[text].GetSimilarityRate(_hashBase[x])))
                .ToArray();
        }
    }
}
