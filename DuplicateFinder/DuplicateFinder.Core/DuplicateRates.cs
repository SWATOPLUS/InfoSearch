using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DuplicateFinder.Core
{
    public class DuplicateRates
    {
        private readonly Dictionary<(string, string), double> _data;

        public DuplicateRates(Dictionary<(string, string), double> data)
        {
            _data = data
                .GroupBy(x => OrderStrings(x.Key.Item1, x.Key.Item2))
                .ToDictionary(x => x.Key, g => g.Select(x => x.Value).Max());
        }

        private static (string, string) OrderStrings(string a, string b)
        {
            if (string.Compare(a, b, StringComparison.Ordinal) <= 0)
            {
                return (a, b);
            }

            return (b, a);
        }

        public double GetSimilarity(string file1, string file2)
        {
            _data.TryGetValue(OrderStrings(file1, file2), out var result);

            return result;
        }

        public static DuplicateRates FromLines(IEnumerable<string> lines)
        {
            return FromLines(lines, Array.Empty<string>());
        }

        public static DuplicateRates FromLines(IEnumerable<string> lines, IEnumerable<string> keys)
        {
            var keysSet = keys.ToHashSet();
            var data = new Dictionary<(string, string), double>();

            foreach (var line in lines)
            {
                var words = line.Trim().Split('\t').ToArray();
                var name1 = words[0];

                if (!keysSet.Contains(name1))
                {
                    continue;
                }

                foreach (var word in words.Skip(1))
                {
                    var parts = word.Split('=');
                    var name2 = parts[0];
                    var rate = double.Parse(parts[1], CultureInfo.InvariantCulture);

                    if (!keysSet.Contains(name2))
                    {
                        continue;
                    }

                    data.Add((name1, name2), rate);
                }
            }

            return new DuplicateRates(data);
        }

        public static DuplicateRates Dummy { get; }
            = new DuplicateRates(new Dictionary<(string, string), double>());

        public static (double FalseNegative, double FalsePositive, double True, double Total) Compare(
            DuplicateRates a,
            DuplicateRates b,
            int size)
        {
            var keysA = a._data.Keys;
            var keysB = b._data.Keys;

            var extraKeysA = keysA.Except(keysB);
            var extraKeysB = keysB.Except(keysA);
            var sameKeys = keysA.Intersect(keysB);

            var aError = extraKeysA.Select(x => a._data[x]).Sum();
            var bError = extraKeysB.Select(x => b._data[x]).Sum();
            var sameError = sameKeys.Select(x => Math.Abs(a._data[x] - b._data[x])).Sum();
            var total = aError + bError + sameError;

            return (FalseNegative: aError, FalsePositive: bError, True: sameError, Total: total);
        }
    }
}
