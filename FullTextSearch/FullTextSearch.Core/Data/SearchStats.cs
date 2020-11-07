using System.Collections.Generic;

namespace FullTextSearch.Core.Data
{
    public class SearchStats
    {
        public Dictionary<string, Dictionary<string, int>> TermsPagesCount;
        public Dictionary<string, int> TermsAtPagesUsed;
        public Dictionary<string, int> PagesWordCount;
        public decimal AveragePageWordCount;
        public int TermsCount;
    }
}
