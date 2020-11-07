using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullTextSearch.Core.Data;
using Newtonsoft.Json;

namespace FullTextSearch.StatsBuilder
{
    internal static class Program
    {
        private const string PagesInputFileName = "../assets/pages.json";
        private const string SearchStatsOutputFileName = "../assets/search-stats.json";

        private static void Main()
        {
            var pages = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PagesInputFileName));
            var stats = BuildSearchStats(pages);

            File.WriteAllText(SearchStatsOutputFileName, JsonConvert.SerializeObject(stats));
        }

        private static readonly char[] Punctuation = "()[]{}<>,.:;'\"!?+-*/^|&=\\%~"
            .ToArray();

        private static SearchStats BuildSearchStats(Dictionary<string, string> pages)
        {
            var pageTerms = pages
                .ToDictionary(x => x.Key, x => BuildTerms(x.Value));

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
                AveragePageWordCount = (decimal)pageWordCount.Values.Sum() / pageTerms.Count,
                PagesWordCount = pageWordCount,
                TermsPagesCount = termsAtPagesPagesCount,
                TermsAtPagesUsed = termsAtPagesUsed,
            };
        }

        private static string[] BuildTerms(string content)
        {
            return content
                .Split()
                .Select(x => x.Trim(Punctuation.ToArray()))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
        }
    }
}
