using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace DuplicateFinder.Core
{
    public static class SequenceExtensions
    {
        public static double GetSimilarityRate<T>(this IReadOnlyCollection<T> a, IReadOnlyCollection<T> b)
        {
            if (a.Count != b.Count)
            {
                throw new InvalidOperationException("Sequences should have same length");
            }

            var count = a.Count;
            var matched = a.Cartesian(b, (x, y) => Equals(x, y) ? 1 : 0).Sum();

            return (double)matched / count;
        }

        public static IEnumerable<T[]> GetNGrams<T>(this IEnumerable<T> sequence, int n)
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
