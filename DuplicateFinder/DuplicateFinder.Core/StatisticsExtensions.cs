using System;
using System.Collections.Generic;
using System.Linq;
using LinqStatistics;

namespace DuplicateFinder.Core
{
    public sealed record SequenceStats(double Average, double Median, double StandardDeviation, double Minimum, double Maximum);

    public static class StatisticsExtensions
    {
        public static double DcgScore(IEnumerable<double> relevance, int count)
        {
            return relevance
                .Take(count)
                .Select((x, i) => Math.Pow(2.0, x - 1) / Math.Log2(i + 2.0))
                .Sum();
        }

        public static double NDcg(
            IReadOnlyCollection<double> expected,
            IEnumerable<double> actual,
            int count,
            double eps = 1e-10)
        {
            var expectedDcg = DcgScore(expected, count);

            if (expectedDcg < eps)
            {
                return 0;
            }

            var actualDcg = DcgScore(actual.Take(expected.Count), count);

            return actualDcg / expectedDcg;
        }

        public static double GetNGDcg10(
            IReadOnlyDictionary<string, double> expected,
            IReadOnlyDictionary<string, double> actual,
            int count)
        {
            var expectedValues = expected
                .OrderByDescending(x => x.Value)
                .Select(x => x.Value)
                .ToArray();

            var actualValues = actual
                .OrderByDescending(x => x.Value)
                .Select(x => expected.GetValueOrDefault(x.Key))
                .ToArray();

            return NDcg(expectedValues, actualValues, count);
        }

        public static SequenceStats GetNGDcg10Stats(
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> expected,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> actual)
        {
            var excessiveKeysResult = SymmetricDifference(expected.Keys.ToArray(), actual.Keys.ToArray())
                .Select(_ => 0.0);
           
            var commonKeysResult = actual.Keys.Intersect(expected.Keys)
                .Select(x => GetNGDcg10(expected[x], actual[x], 10));

            var scores = excessiveKeysResult
                .Concat(commonKeysResult)
                .ToArray();

            return new SequenceStats(0, 0, 0, 0, 0)
            {
                Average = scores.Average(), 
                Median = scores.Median(),
                StandardDeviation = scores.StandardDeviation(),
                Minimum = scores.Min(),
                Maximum = scores.Max(),
            };
        }

        public static IEnumerable<T> SymmetricDifference<T>(IReadOnlyCollection<T> a, IReadOnlyCollection<T> b)
        {
            return a.Except(b).Concat(b.Except(a));
        }

        public static IEnumerable<T> Repeat<T>(T item, int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return item;
            }
        }
    }
}
