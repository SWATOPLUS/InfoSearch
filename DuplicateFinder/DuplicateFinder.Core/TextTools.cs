using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DuplicateFinder.Core
{
    public static class TextTools
    {
        public static readonly IReadOnlyList<char> EnglishCharsHigh = GetChars('A', 'Z');
        public static readonly IReadOnlyList<char> RussianCharsHigh = GetChars('А', 'Я');
        public static readonly IReadOnlyList<char> EnglishCharsLow = GetChars('a', 'z');
        public static readonly IReadOnlyList<char> RussianCharsLow = GetChars('а', 'я');
        public static readonly IReadOnlyList<char> NumberChars = GetChars('0', '9');
        public static readonly IReadOnlyList<char> Punctuation = "()[]{}<>,.:;'\"!?+-*/^|&"
            .ToImmutableArray();

        public static readonly IReadOnlyList<char> SourceCodeChars = EnglishCharsHigh
            .Concat(EnglishCharsLow)
            .Concat(NumberChars)
            .Concat(Punctuation)
            .ToImmutableArray();

        public static IReadOnlyList<char> GetChars(char start, char end)
        {
            return Enumerable.Range(start, end - start + 1)
                .Select(x => (char) x)
                .ToImmutableArray();
        }
    }
}
