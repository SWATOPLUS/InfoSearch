using System;
using System.Collections.Generic;
using System.Linq;

namespace Indexing
{
    public class DocumentSearcher
    {
        private readonly SearchStats _stats;
        private readonly InvertIndex _index;

        public DocumentSearcher(SearchStats stats, InvertIndex index)
        {
            _stats = stats;
            _index = index;
        }
        
        public KeyValuePair<string, double>[] Bm25(string query, int documentsCount, int b = 1, int k1 = 1, int k2 = 1)
        {
            var terms = TextUtility.BuildTerms(query)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            var position = terms.ToDictionary(x => x.Key, x => 0);
            var heap = new PriorityQueue<(int Page, int Term)>();
            var resultHeap = new PriorityQueue<(double Score, int Page)>();

            foreach (var (term, _) in terms)
            {
                var termIndex = _index.TermIndexes[term];
                var pageIndex = _index.TermIdAndPageIdToCount[termIndex].First().Page;

                heap.Enqueue((pageIndex, termIndex));
            }

            while (heap.Count() > 0)
            {
                var topPageId = heap.Peek().Page;
                var topPageName = _index.Pages[topPageId];

                var k = k1 * (1 - b + b * (_stats.PagesWordCount[topPageName] / _stats.AveragePageWordCount));
                var score = 0.0;

                while (heap.Count() > 0 && heap.Peek().Page == topPageId)
                {
                    var (_, term) = heap.Dequeue();
                    var termName = _index.Terms[term];
                    var termInPageCount = _stats.TermOnPageCount[termName][topPageName];
                    var termInQueryCount = terms[termName];

                    score += Idf(termName)
                        * ((k1 + 1) * termInPageCount) / (k + termInPageCount)
                        * ((k2 + 1) * termInQueryCount) / (k2 + termInQueryCount);

                    position[termName] += 1;
                    var list = _index.TermIdAndPageIdToCount[term];

                    if (position[termName] < list.Count)
                    {
                        heap.Enqueue((list[position[termName]].Page, term));
                    }
                }

                resultHeap.Enqueue((score, topPageId));

                while (resultHeap.Count() > documentsCount)
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
