using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DuplicateFinder.Core
{
    public static class SourceCodeNormalizer
    {
        private static readonly Regex WhiteSpaceRegex = new Regex("[\\s\\uFEFF\\u200B]+", RegexOptions.Compiled);

        public static string RemoveWhiteSpaces(string source)
        {
            var commentPattern = @"/[*][\w\d\s]*[*]/";

            source = Regex.Replace(source, commentPattern, " ", RegexOptions.Multiline);
            source = string.Join(" ", source.Split('\n').Select(CleanSingleLineComment));

            return WhiteSpaceRegex.Replace(source, " ");
        }

        public static string CleanSingleLineComment(string line)
        {
            var commentPattern = @"//.*";

            return Regex.Replace(line, commentPattern, string.Empty);
        }

        public static string CleanMultiLineComment(string text)
        {
            var startPattern = "/[*]";
            var endPattern = "[*]/";

            var starts = Regex.Matches(text, startPattern).Select(x => x.Index).ToArray();
            var ends = Regex.Matches(text, endPattern).Select(x => x.Index).ToArray();

            var comments = new List<(int, int)>();
            var startsQueue = new Queue<int>(starts);
            var endsQueue = new Queue<int>(ends);

            var index = 0;

            while (startsQueue.Count > 0)
            {
                var start = startsQueue.Dequeue();

                if (index > start)
                {
                    continue;
                }

                var end = 0;

                while (endsQueue.Count > 0)
                {
                    end = endsQueue.Dequeue();

                    if (end > start)
                    {
                        break;
                    }
                }

                if (end < start)
                {
                    break;
                }

                comments.Add((start, end + 2));
                index = end + 2;
            }


        }
    }
}
