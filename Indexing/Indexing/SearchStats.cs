using System.Collections.Generic;

namespace Indexing
{
    public class SearchStats
    {
        public Dictionary<string, Dictionary<string, int>> TermOnPageCount { get; set; }
        public Dictionary<string, int> TermAtPagesCount { get; set; }
        public Dictionary<string, int> PagesWordCount { get; set; }
        public double AveragePageWordCount { get; set; }
        public int TermsCount { get; set; }
        public int DocumentsCount { get; set; }
    }
}
