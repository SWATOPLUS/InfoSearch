using System.Collections.Generic;

namespace WikiDownloader.GraphAnalyzerCli.Models
{
    public class PageRankResult
    {
        public int Iterations { get; set; }

        public decimal Delta { get; set; }

        public decimal Error { get; set; }
        
        public Dictionary<string, decimal> TopPageRanks { get; set; }

        public Dictionary<string, decimal> PageRanks { get; set; }
    }
}
