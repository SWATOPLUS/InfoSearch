using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullTextSearch.Core;
using FullTextSearch.Core.Data;

namespace FullTextSearch.StatsBuilder
{
    internal static class Program
    {
        private const string PagesInputFileName = "../assets/pages.json";
        private const string SearchStatsOutputFileName = "../assets/search-stats.json";
        private const string InvertIndexOutputFileName = "../assets/invert-index.json";
        private const string InvertIndexBinOutputFileName = "../assets/invert-index.bin";

        private static void Main()
        {
            var pages = FileUtility.ReadJson<Dictionary<string, string>>(PagesInputFileName);
            var stats = SearchStats.Build(pages);
            var index = InvertIndex.Build(stats);

            FileUtility.WriteJson(SearchStatsOutputFileName, stats);
            FileUtility.WriteJson(InvertIndexOutputFileName, index);
            File.WriteAllBytes(InvertIndexBinOutputFileName, index.ToBytes().ToArray());
        }
    }
}
