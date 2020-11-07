using System.Linq;

namespace Indexing
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
                .ToArray();
        }
    }
}
