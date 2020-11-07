using System.Linq;
using DuplicateFinder.Core;

namespace DuplicateFinder.TextAnalyzer
{
    internal static class Program
    {
        private const string InputFile = "../../assets/sources-clean.json";
        private const string OutputFile = "../../assets/text-stats.json";

        private static void Main()
        {
            var documents = DirectoryTools.ReadStringDictionary(InputFile);
            var allText = string.Join(' ', documents.Values);

            var chars = allText
                .Distinct()
                .Except(TextTools.SourceCodeChars)
                .ToArray();

            var words = allText
                .Split()
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count())
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            var result = new
            {
                chars,
                words,
            };

            DirectoryTools.SaveAsJson(result, OutputFile);
        }
    }
}
