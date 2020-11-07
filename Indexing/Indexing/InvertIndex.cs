using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Indexing
{
    public class InvertIndex
    {
        public Dictionary<string, int> TermIndexes { get; set; }
        public Dictionary<string, int> PageIndexes { get; set; }

        public string[] Terms { get; set; }
        public string[] Pages { get; set; }
        public Dictionary<int, List<(int Page, int Count)>> TermIdAndPageIdToCount { get; set; }

        public static InvertIndex Build(SearchStats stats)
        {
            var terms = stats.TermOnPageCount.Keys
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);
            
            var pages = stats.PagesWordCount.Keys
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);

            var index = new Dictionary<int, List<(int, int)>>();

            foreach (var (term, termIndex) in terms)
            {
                index[termIndex] = new List<(int, int)>();

                foreach (var (page, count)  in stats.TermOnPageCount[term])
                {
                    var pageIndex = pages[page];

                    index[termIndex].Add((pageIndex, count));
                }

                index[termIndex] = index[termIndex].OrderBy(x => x.Item1).ToList();
            }

            return new InvertIndex
            {
                Terms = terms.OrderBy(x => x.Value).Select(x => x.Key).ToArray(),
                Pages = pages.OrderBy(x => x.Value).Select(x => x.Key).ToArray(),
                TermIndexes = terms,
                PageIndexes = pages,
                TermIdAndPageIdToCount = index,
            };
        }

        public static InvertIndex Prune(InvertIndex index, int count)
        {
            return new InvertIndex
            {
                Terms = index.Terms,
                Pages = index.Pages,
                TermIndexes = index.TermIndexes,
                PageIndexes = index.PageIndexes,
                TermIdAndPageIdToCount = index.TermIdAndPageIdToCount
                    .ToDictionary(x => x.Key, x => Prune(x.Value, count)),
            };
        }

        public static List<(int Page, int Count)> Prune(
            List<(int Page, int Count)> postingList,
            int count)
        {
            return postingList
                .OrderByDescending(x => x.Count)
                .Take(count)
                .OrderBy(x => x.Page)
                .ToList();
        }
    }
}
