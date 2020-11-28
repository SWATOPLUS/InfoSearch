using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FullTextSearch.Core;
using FullTextSearch.Core.Data;
using FullTextSearch.Core.Searchers;

namespace FullTextSearch
{
    internal static class Program
    {
        private const string QueriesInputFileName = "../assets/queries.tsv";
        private const string SearchStatsInputFileName = "../assets/search-stats.json";
        private const string InvertIndexInputFileName = "../assets/invert-index.json";

        private const string SearcherSummariesOutputFileName = "../assets/searcher-summaries.json";

        private static readonly string[] TestQueries =
        {
            "coronovirus in belarus",
            "who won junior eurovision in 2005",
            "science about full-text search",
        };

        private static void Main()
        {
            var stats = FileUtility.ReadJson<SearchStats>(SearchStatsInputFileName);
            var index = FileUtility.ReadJson<InvertIndex>(InvertIndexInputFileName);
            var index50 = index.Prune(50);

            var searches = new List<ISearcher>
            {
                new TfIdfSearcher(stats),
            };

            var b = 0.89;
            var k1 = 3.13;
            var k2 = 6.96;

            searches.Add(new Bm25Searcher(stats, b, k1, k2));
            searches.Add(new Bm25InvertSearcher(stats, index, 10, null, b, k1, k2));
            searches.Add(new Bm25InvertSearcher(stats, index50, 10, 50, b, k1, k2));

            var queries = FileUtility
                .ReadTsv(QueriesInputFileName)
                .ToArray();

            var list = searches
                .Select(searcher => TestSearcher(queries, searcher))
                .ToArray();

            FileUtility.WriteJson(SearcherSummariesOutputFileName, list, true);
        }

        private static SearcherSummary TestSearcher(IReadOnlyList<string[]> queries, ISearcher searcher)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            var results = queries
                .Select(x => x[0])
                .Select(searcher.Search)
                .ToArray();
            
            sw.Stop();

            var top1 = 0;
            var top10 = 0;

            for (var i = 0; i < results.Length; i++)
            {
                var expected = queries[i][1];
                var actual = results[i];

                if (actual.Take(1).Any(x => x.Key == expected))
                {
                    top1++;
                }

                if (actual.Take(10).Any(x => x.Key == expected))
                {
                    top10++;
                }
            }

            return new SearcherSummary
            {
                Name = searcher.Name,
                Time = sw.Elapsed,
                Top1 = (double)top1 / queries.Count,
                Top10 = (double)top10 / queries.Count,
                TestQueries = TestQueries.ToDictionary(
                        x => x,
                        q => searcher
                            .Search(q)
                            .OrderByDescending(x => x.Value)
                            .ToDictionary(x => x.Key, x => x.Value)),
            };
        }

        private class SearcherSummary
        {
            public string Name { get; set; }

            public TimeSpan Time { get; set; }

            public double Top1 { get; set; }

            public double Top10 { get; set; }

            public Dictionary<string, Dictionary<string, double>> TestQueries { get; set; }
        }
    }
}
