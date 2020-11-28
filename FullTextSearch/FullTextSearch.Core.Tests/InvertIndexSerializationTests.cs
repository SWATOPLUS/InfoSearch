using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FullTextSearch.Core.Data;
using Xunit;

namespace FullTextSearch.Core.Tests
{
    public class InvertIndexSerializationTests
    {
        private const string InvertIndexOutputFileName = "../../../../assets/invert-index.json";
        private const string InvertIndexBinOutputFileName = "../../../../assets/invert-index.bin";

        [Fact]
        public void Test()
        {
            var index1 = FileUtility.ReadJson<InvertIndex>(InvertIndexOutputFileName);
            var index2 = InvertIndex.FromBytes(File.ReadAllBytes(InvertIndexBinOutputFileName));

            AssertSameArrays(index1.Pages, index2.Pages);
            AssertSameArrays(index1.Terms, index2.Terms);
            AssertSameArrays(index1.PageIndexes.ToArray(), index2.PageIndexes.ToArray());
            AssertSameArrays(index1.TermIndexes.ToArray(), index2.TermIndexes.ToArray());
            AssertSameArrays(index1.Pages, index2.Pages);
            AssertSameArrays(index1.TermIdAndPageIdToCount.Keys.ToArray(), index2.TermIdAndPageIdToCount.Keys.ToArray());

            foreach (var key in index1.TermIdAndPageIdToCount.Keys)
            {
                AssertSameArrays(index1.TermIdAndPageIdToCount[key], index2.TermIdAndPageIdToCount[key]);
            }
        }

        private void AssertSameArrays<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
        {
            if (a.Count != b.Count)
            {
                throw new InvalidOperationException($"The arrays have different length {a.Count} vs {b.Count}");
            }

            for (var i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    throw new InvalidOperationException($"The arrays have different item {i}");
                }
            }
        }
    }
}
