using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
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
                //.Except(TextTools.RussianCharsHigh)
                //.Except(TextTools.RussianCharsLow)
                .ToArray();
            
            var result = new
            {
                Chars = chars,
            };

            DirectoryTools.SaveAsJson(result, OutputFile);
        }
    }
}
