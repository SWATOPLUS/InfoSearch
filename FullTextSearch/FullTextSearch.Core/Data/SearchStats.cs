using System.Collections.Generic;
using System.Linq;

namespace FullTextSearch.Core.Data
{
    public class SearchStats
    {
        public Dictionary<string, Dictionary<string, int>> TermOnPageCount { get; set; }
        public Dictionary<string, int> TermAtPagesCount { get; set; }
        public Dictionary<string, int> PagesWordCount { get; set; }
        public double AveragePageWordCount { get; set; }
        public int TermsCount { get; set; }
        public int DocumentsCount { get; set; }

        public static SearchStats Build(Dictionary<string, string> pages)
        {
            var pageTerms = pages
                .ToDictionary(x => x.Key, x => TextUtility.BuildTerms(x.Value));

            var allTerms = pageTerms.Values
                .SelectMany(x => x)
                .Distinct()
                .ToArray();

            var termsAtPagesPagesCount = allTerms.ToDictionary(x => x, x => new Dictionary<string, int>());
            var termsAtPagesUsed = allTerms.ToDictionary(x => x, x => 0);
            var pageWordCount = new Dictionary<string, int>();

            foreach (var (name, terms) in pageTerms)
            {
                foreach (var group in terms.GroupBy(x => x))
                {
                    termsAtPagesPagesCount[group.Key][name] = group.Count();
                    termsAtPagesUsed[group.Key] += 1;
                }

                pageWordCount[name] = terms.Length;
            }

            return new SearchStats
            {
                TermsCount = pageTerms.Count,
                AveragePageWordCount = (double)pageWordCount.Values.Sum() / pageTerms.Count,
                PagesWordCount = pageWordCount,
                TermOnPageCount = termsAtPagesPagesCount,
                TermAtPagesCount = termsAtPagesUsed,
                DocumentsCount = pages.Count,
            };
        }
    }
}
