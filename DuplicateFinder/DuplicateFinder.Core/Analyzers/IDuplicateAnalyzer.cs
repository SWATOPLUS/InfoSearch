using System.Collections.Generic;

namespace DuplicateFinder.Core.Analyzers
{
    public interface IDuplicateAnalyzer
    {
        void AddText(string name, string text);

        KeyValuePair<string, double>[] Analyze(string text);

        DuplicateRates GetAllDuplicates();
    }
}
