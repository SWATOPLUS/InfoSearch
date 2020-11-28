using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace DuplicateFinder.Core
{
    public class DuplicateRates
    {
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> Data { get; }

        public DuplicateRates(IEnumerable<KeyValuePair<(string, string), double>> data)
        {
            var cleanData = data
                .GroupBy(x => OrderStrings(x.Key.Item1, x.Key.Item2))
                .ToDictionary(x => x.Key, g => g.Select(x => x.Value).Max());

            var result = new Dictionary<string, Dictionary<string, double>>();

            foreach (var ((keyA, keyB), value) in cleanData)
            {
                if (!result.ContainsKey(keyA))
                {
                    result[keyA] = new Dictionary<string, double>();
                }

                if (!result.ContainsKey(keyB))
                {
                    result[keyB] = new Dictionary<string, double>();
                }

                result[keyA][keyB] = value;
                result[keyB][keyA] = value;
            }

            Data = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, double>>(
                result.MapValues(x => new ReadOnlyDictionary<string, double>(x) as IReadOnlyDictionary<string, double>));
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
            return Data.GetValueOrDefault(file1)?.GetValueOrDefault(file2) ?? 0.0;
        }

        public static DuplicateRates FromLines(IEnumerable<string> lines, IEnumerable<string> keys = null)
        {
            var keysSet = keys?.ToHashSet();
            var data = new Dictionary<(string, string), double>();

            foreach (var line in lines)
            {
                var words = line.Trim().Split('\t').ToArray();
                var name1 = words[0];

                if (keysSet != null && !keysSet.Contains(name1))
                {
                    continue;
                }

                foreach (var word in words.Skip(1))
                {
                    var parts = word.Split('=');
                    var name2 = parts[0];
                    var rate = double.Parse(parts[1], CultureInfo.InvariantCulture);

                    if (keysSet != null && !keysSet.Contains(name2))
                    {
                        continue;
                    }

                    data.Add((name1, name2), rate);
                }
            }

            return new DuplicateRates(data);
        }

        public static DuplicateRates Dummy { get; } = new(new Dictionary<(string, string), double>());
    }
}
