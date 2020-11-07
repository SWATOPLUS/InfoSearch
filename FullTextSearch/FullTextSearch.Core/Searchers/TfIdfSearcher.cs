using System;
using System.Collections.Generic;
using System.Linq;
using FullTextSearch.Core.Data;

namespace FullTextSearch.Core.Searchers
{
    public class TfIdfSearcher : ISearcher
    {
        private readonly SearchStats _stats;

        public string Name => nameof(TfIdfSearcher);

        public TfIdfSearcher(SearchStats stats)
        {
            _stats = stats;
        }

        public KeyValuePair<string, double>[] Search(string query)
        {
            var terms = TextUtility.BuildTerms(query);
            var result = new Dictionary<string, double>();

            foreach (var term in terms.Distinct())
            {
                if (!_stats.TermAtPagesCount.ContainsKey(term))
                {
                    continue;
                }

                foreach (var (page, _) in _stats.TermOnPageCount[term])
                {
                    if (!result.ContainsKey(page))
                    {
                        result[page] = 0;
                    }

                    result[page] += Tf(page, term) * Idf(term);
                }
            }

            return result
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToArray();
        }
        
        private int Tf(string page, string term)
        {
            return _stats.TermOnPageCount[term][page];
        }

        private double Idf(string term)
        {
            return Math.Log((_stats.DocumentsCount + 1.0) / _stats.TermAtPagesCount[term]);
        }
    }
}
