using System.Collections.Generic;

namespace FullTextSearch.Core.Searchers
{
    public interface ISearcher
    {
        string Name { get; }

        KeyValuePair<string, double>[] Search(string query);
    }
}
