using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DuplicateFinder.Core
{
    public static class SourceCodeNormalizer
    {
        private static readonly Regex WhiteSpaceRegex = new Regex("[\\s\\uFEFF\\u200B]+", RegexOptions.Compiled);
        private static readonly Regex UsingRegex = new Regex("\\s*using\\s+.+;");
        private static readonly Regex ImportRegex = new Regex("\\s*import\\s+.+;");
        private static readonly Regex NameSpaceRegex = new Regex("\\s*namespace\\s+");
        private static readonly Regex IncludeRegex = new Regex("\\s*#include\\s+");

        public static string Normalize(string source)
        {
            var lines = source.Split('\n')
                .Select(x => WhiteSpaceRegex.Replace(x, " ").Trim())
                .Select(CleanSingleLineComment)
                .Select(CleanPythonSingleLineComment)
                .Where(x => !IsDumpLine(x));

            source = string.Join(" ", lines);
            source = CleanMultiLineComment(source);
            source = AddSpacesForOperators(source);
            source = CleanStringLiterals(source);
            source = CleanCharLiterals(source);

            return WhiteSpaceRegex.Replace(source, " ").Trim();
        }

        public static bool IsDumpLine(string line)
        {
            return UsingRegex.IsMatch(line)
                   || ImportRegex.IsMatch(line)
                   || NameSpaceRegex.IsMatch(line)
                   || IncludeRegex.IsMatch(line);
        }

        public static string AddSpacesForOperators(string source)
        {
            foreach (var c in TextTools.Punctuation)
            {
                source = source.Replace($"{c}", $" {c} ");
            }

            return source;
        }

        public static string CleanSingleLineComment(string line)
        {
            var commentPattern = @"//.*";

            return Regex.Replace(line, commentPattern, string.Empty);
        }

        public static string CleanPythonSingleLineComment(string line)
        {
            var commentPattern = @"#.*";

            return Regex.Replace(line, commentPattern, string.Empty);
        }

        public static string CleanMultiLineComment(string text)
        {
            var pattern = "(/[*])|([*]/)";
            var start = "/*";
            var end = "*/";

            var matches = Regex.Matches(text, pattern).ToArray();

            if (matches.Length < 1)
            {
                return text;
            }

            var builder = new StringBuilder();

            var index = 0;
            var inComment = false;

            foreach (var match in matches)
            {
                if (!inComment && match.Value == start)
                {
                    inComment = true;
                    builder.Append(text.Substring(index, match.Index - index));
                    builder.Append(' ');
                }

                if (inComment && match.Value == end)
                {
                    inComment = false;
                    index = match.Index + 2;
                }
            }

            if (!inComment)
            {
                builder.Append(text.Substring(index, text.Length - index));
            }

            return builder.ToString();
        }

        public static string CleanStringLiterals(string text)
        {
            var pattern = "\"";

            var matches = Regex.Matches(text, pattern).ToArray();

            if (matches.Length < 1)
            {
                return text;
            }

            var builder = new StringBuilder();

            var index = 0;
            var inLiteral = false;

            foreach (var match in matches)
            {
                if (!inLiteral)
                {
                    inLiteral = true;
                    builder.Append(text.Substring(index, match.Index - index));
                    builder.Append(' ');
                } else {
                    inLiteral = false;
                    index = match.Index + 2;
                }
            }

            if (!inLiteral)
            {
                builder.Append(text.Substring(index, text.Length - index));
            }

            return builder.ToString();
        }

        public static string CleanCharLiterals(string text)
        {
            var pattern = "'";

            var matches = Regex.Matches(text, pattern).ToArray();

            if (matches.Length < 1)
            {
                return text;
            }

            var builder = new StringBuilder();

            var index = 0;
            var inLiteral = false;

            foreach (var match in matches)
            {
                if (!inLiteral)
                {
                    inLiteral = true;
                    builder.Append(text.Substring(index, match.Index - index));
                    builder.Append(' ');
                }
                else
                {
                    inLiteral = false;
                    index = match.Index + 2;
                }
            }

            if (!inLiteral)
            {
                builder.Append(text.Substring(index, text.Length - index));
            }

            return builder.ToString();
        }
    }
}
