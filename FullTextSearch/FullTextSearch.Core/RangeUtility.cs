using System.Collections.Generic;

namespace FullTextSearch.Core
{
    public static class RangeUtility
    {
        public static IEnumerable<double> Range(double from, double to, double step)
        {
            while (from < to)
            {
                yield return from;

                from += step;
            }
        }
    }
}
