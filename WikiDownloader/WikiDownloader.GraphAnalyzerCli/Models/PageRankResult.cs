using System.Collections.Generic;

namespace WikiDownloader.GraphAnalyzerCli.Models
{
    public class PageRankResult
    {
        public int Iterations { get; set; }

        public double Delta { get; set; }

        public double Error { get; set; }
        
        public Dictionary<string, double> TopPageRanks { get; set; }

        public Dictionary<string, double> PageRanks { get; set; }
    }
}
