using System.Linq;

namespace FullTextSearch.Core
{
    public static class TextUtility
    {
        public static readonly char[] Punctuation = "()[]{}<>,.:;\"!?+-*/^|&=\\%~"
            .ToArray();

        public static string[] BuildTerms(string content)
        {
            return content
                .Split()
                .Select(x => x.Trim(Punctuation.ToArray()))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.ToLowerInvariant())
                .ToArray();
        }
    }
}
