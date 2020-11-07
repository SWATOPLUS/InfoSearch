using System;
using System.Collections.Generic;
using System.Linq;
using FullTextSearch.Core.Data;

namespace FullTextSearch.Core.Searchers
{
    public class Bm25Searcher : ISearcher
    {
        private readonly SearchStats _stats;
        private readonly double _b;
        private readonly double _k1;
        private readonly double _k2;

        public string Name => $"{nameof(Bm25Searcher)}: b={_b}, k1={_k1}, k2={_k2}";

        public Bm25Searcher(SearchStats stats, double b = 1, double k1 = 1, double k2 = 1)
        {
            _stats = stats;
            _b = b;
            _k1 = k1;
            _k2 = k2;
        }

        public KeyValuePair<string, double>[] Search(string query)
        {
            var terms = TextUtility.BuildTerms(query)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            var result = new Dictionary<string, double>();

            foreach (var (term, termInQueryCount) in terms)
            {
                if (!_stats.TermAtPagesCount.ContainsKey(term))
                {
                    continue;
                }

                foreach (var (page, termInPageCount) in _stats.TermOnPageCount[term])
                {
                    if (!result.ContainsKey(page))
                    {
                        result[page] = 0;
                    }

                    var k = _k1 * (1 - _b + _b * (_stats.PagesWordCount[page] / _stats.AveragePageWordCount));

                    result[page] += Idf(term) 
                        * ((_k1 + 1) * termInPageCount) / (k + termInPageCount) 
                        * ((_k2 + 1) * termInQueryCount) / (_k2 + termInQueryCount);
                }
            }

            return result
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToArray();
        }

        private double Idf(string term)
        {
            return Math.Log((_stats.DocumentsCount + 1.0) / _stats.TermAtPagesCount[term]);
        }
    }
}
