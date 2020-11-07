using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullTextSearch.Core.Data;

namespace FullTextSearch.Core
{
    public class DocumentSearcher
    {
        private readonly SearchStats _stats;

        public DocumentSearcher(SearchStats stats)
        {
            _stats = stats;
        }

        public KeyValuePair<string, double>[] TfIdf(string query)
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

        public KeyValuePair<string, double>[] Bm25(string query, int b = 1, int k1 = 1, int k2 = 1)
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

                    var k = k1 * (1 - b + b * (_stats.PagesWordCount[page] / _stats.AveragePageWordCount));

                    result[page] += Idf(term) 
                        * ((k1 + 1) * termInPageCount) / (k + termInPageCount) 
                        * ((k2 + 1) * termInQueryCount) / (k2 + termInQueryCount);
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
            return Math.Log((double)(_stats.DocumentsCount + 1) / _stats.TermAtPagesCount[term]);
        }
    }
}
