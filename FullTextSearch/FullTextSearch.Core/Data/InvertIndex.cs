using System.Collections.Generic;
using System.Linq;

namespace FullTextSearch.Core.Data
{
    public class InvertIndex
    {
        public Dictionary<string, int> TermIndexes { get; set; }
        public Dictionary<string, int> PageIndexes { get; set; }

        public string[] Terms { get; set; }
        public string[] Pages { get; set; }
        public Dictionary<int, (int Page, int Count)[]> TermIdAndPageIdToCount { get; set; }

        public static InvertIndex Build(SearchStats stats)
        {
            var terms = stats.TermOnPageCount.Keys
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);
            
            var pages = stats.PagesWordCount.Keys
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);

            var index = new Dictionary<int, (int, int)[]>();

            foreach (var (term, termIndex) in terms)
            {
                var postingList = new List<(int, int)>();

                foreach (var (page, count)  in stats.TermOnPageCount[term])
                {
                    var pageIndex = pages[page];

                    postingList.Add((pageIndex, count));
                }

                index[termIndex] = postingList.OrderBy(x => x.Item1).ToArray();
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

        public InvertIndex Prune(int count)
        {
            return new InvertIndex
            {
                Terms = Terms,
                Pages = Pages,
                TermIndexes = TermIndexes,
                PageIndexes = PageIndexes,
                TermIdAndPageIdToCount = TermIdAndPageIdToCount
                    .ToDictionary(x => x.Key, x => Prune(x.Value, count)),
            };
        }

        private static (int Page, int Count)[] Prune(
            IEnumerable<(int Page, int Count)> postingList,
            int count)
        {
            return postingList
                .OrderByDescending(x => x.Count)
                .Take(count)
                .OrderBy(x => x.Page)
                .ToArray();
        }
    }
}
