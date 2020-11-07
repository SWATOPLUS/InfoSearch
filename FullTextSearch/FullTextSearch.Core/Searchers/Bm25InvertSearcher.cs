using System;
using System.Collections.Generic;
using System.Linq;
using FullTextSearch.Core.Data;

namespace FullTextSearch.Core.Searchers
{
    public class Bm25InvertSearcher : ISearcher
    {
        private readonly SearchStats _stats;
        private readonly InvertIndex _index;
        private readonly int _documentCount;
        private readonly int? _prune;
        private readonly double _b;
        private readonly double _k1;
        private readonly double _k2;

        public string Name => $"{nameof(Bm25InvertSearcher)}: prune={_prune} b={_b}, k1={_k1}, k2={_k2}";

        public Bm25InvertSearcher(SearchStats stats, InvertIndex index, int documentCount, int? prune, double b = 1, double k1 = 1, double k2 = 1)
        {
            _stats = stats;
            _index = index; // prune.HasValue ? index.Prune(prune.Value) : index;
            _documentCount = documentCount;
            _prune = prune;
            _b = b;
            _k1 = k1;
            _k2 = k2;
        }

        public KeyValuePair<string, double>[] Search(string query)
        {
            var terms = TextUtility.BuildTerms(query)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            var position = terms.ToDictionary(x => x.Key, x => 0);
            var heap = new PriorityQueue<(int Page, int Term, int Count)>();
            var resultHeap = new PriorityQueue<(double Score, int Page)>();

            foreach (var (term, _) in terms)
            {
                if (!_index.TermIndexes.ContainsKey(term))
                {
                    continue;
                }

                var termIndex = _index.TermIndexes[term];
                var (page, count) = _index.TermIdAndPageIdToCount[termIndex].First();

                heap.Enqueue((page, termIndex, count));
            }

            while (heap.Count() > 0)
            {
                var pageId = heap.Peek().Page;
                var pageName = _index.Pages[pageId];

                var k = _k1 * (1 - _b + _b * (_stats.PagesWordCount[pageName] / _stats.AveragePageWordCount));
                var score = 0.0;

                while (heap.Count() > 0 && heap.Peek().Page == pageId)
                {
                    var (_, term, count) = heap.Dequeue();
                    var termName = _index.Terms[term];
                    var termInPageCount = count;
                    var termInQueryCount = terms[termName];

                    score += Idf(termName)
                        * ((_k1 + 1) * termInPageCount) / (k + termInPageCount)
                        * ((_k2 + 1) * termInQueryCount) / (_k2 + termInQueryCount);

                    position[termName] += 1;
                    var list = _index.TermIdAndPageIdToCount[term];

                    if (position[termName] < list.Length)
                    {
                        var (newPage, newCount) = list[position[termName]];

                        heap.Enqueue((newPage, term, newCount));
                    }
                }

                resultHeap.Enqueue((score, pageId));

                while (resultHeap.Count() > _documentCount)
                {
                    resultHeap.Dequeue();
                }
            }

            var result = new List<KeyValuePair<string, double>>();

            while (resultHeap.Count() > 0)
            {
                var (score, pageId) = resultHeap.Dequeue();

                result.Add(new KeyValuePair<string, double>(_index.Pages[pageId], score));
            }

            return result
                .OrderByDescending(x => x.Value)
                .ToArray();
        }

        private double Idf(string term)
        {
            return Math.Log((_stats.DocumentsCount + 1.0) / _stats.TermAtPagesCount[term]);
        }
    }
}
