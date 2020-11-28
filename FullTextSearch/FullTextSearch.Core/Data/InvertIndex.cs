using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FullTextSearch.Core.Data
{
    public class InvertIndex
    {
        public Dictionary<string, int> TermIndexes { get; set; }
        public Dictionary<string, int> PageIndexes { get; set; }

        public string[] Terms { get; set; }
        public string[] Pages { get; set; }
        public Dictionary<int, (int Page, int Count)[]> TermIdAndPageIdToCount { get; set; }

        public IEnumerable<byte> ToBytes()
        {
            var result = new List<byte>();

            var dict = TermIdAndPageIdToCount
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToArray();

            result.AddRange(ArrayToBytes(Terms, StringToBytes));
            result.AddRange(ArrayToBytes(Pages, StringToBytes));
            result.AddRange(ArrayToBytes(dict, x => ArrayToBytes(x, IntIntToBytes)));

            return result;
        }

        public static InvertIndex FromBytes(byte[] bytes)
        {
            var result = new InvertIndex();
            var offset = 0;
            (int Page, int Count)[][] dict;

            (result.Terms, offset) = ArrayFromBytes(bytes, StringFromBytes, offset);
            (result.Pages, offset) = ArrayFromBytes(bytes, StringFromBytes, offset);
            (dict, _) = ArrayFromBytes(bytes, (b, o) => ArrayFromBytes(b, IntIntFromBytes, o), offset);

            result.TermIdAndPageIdToCount = dict
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.i, x => x.x);

            result.PageIndexes = result.Pages.
                Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);

            result.TermIndexes = result.Terms.
                Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);

            return result;
        }

        private static IEnumerable<byte> IntIntToBytes((int, int) item)
        {
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(item.Item1));
            result.AddRange(BitConverter.GetBytes(item.Item2));

            return result;
        }

        private static ((int, int), int) IntIntFromBytes(byte[] bytes, int offset)
        {
            var a = BitConverter.ToInt32(bytes, offset);
            var b = BitConverter.ToInt32(bytes, offset + 4);

            return ((a, b), offset + 8);
        }

        private static IEnumerable<byte> StringToBytes(string s)
        {
            var result = new List<byte>();
            var bytes = Encoding.UTF8.GetBytes(s);
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);

            return result;
        }

        private static (string, int) StringFromBytes(byte[] bytes, int offset)
        {
            var length = BitConverter.ToInt32(bytes, offset);

            return (Encoding.UTF8.GetString(bytes, offset + 4, length), offset + 4 + length);
        }

        private static IEnumerable<byte> ArrayToBytes<T>(
            IReadOnlyCollection<T> array,
            Func<T, IEnumerable<byte>> converter)
        {
            var result = new List<byte>();

            foreach (var item in array)
            {
                result.AddRange(converter(item));
            }

            return BitConverter.GetBytes(result.Count)
                .Concat(result);
        }

        private static (T[], int) ArrayFromBytes<T>(byte[] bytes, Func<byte[], int, (T, int)> converter, int offset)
        {
            var length = BitConverter.ToInt32(bytes, offset);
            var result = new List<T>();
            var position = offset + 4;

            while (position < length + offset)
            {
                T item;

                (item, position) = converter(bytes, position);

                result.Add(item);
            }

            return (result.ToArray(), offset + 4 + length);
        }

        public static InvertIndex Build(SearchStats stats)
        {
            var terms = stats.TermOnPageCount.Keys
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);

            var pages = stats.PagesWordCount.Keys
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x, x => x.i);

            var index = new Dictionary<int, (int, int)[]>();

            foreach (var (term, termIndex) in terms)
            {
                var postingList = new List<(int, int)>();

                foreach (var (page, count) in stats.TermOnPageCount[term])
                {
                    var pageIndex = pages[page];

                    postingList.Add((pageIndex, count));
                }

                index[termIndex] = postingList.OrderBy(x => x.Item1).ToArray();
            }

            return new InvertIndex
            {
                Terms = terms.OrderBy(x => x.Value).Select(x => x.Key).ToArray(),
                Pages = pages.OrderBy(x => x.Value).Select(x => x.Key).ToArray(),
                TermIndexes = terms,
                PageIndexes = pages,
                TermIdAndPageIdToCount = index,
            };
        }

        public InvertIndex Prune(int count)
        {
            return new InvertIndex
            {
                Terms = Terms,
                Pages = Pages,
                TermIndexes = TermIndexes,
                PageIndexes = PageIndexes,
                TermIdAndPageIdToCount = TermIdAndPageIdToCount
                    .ToDictionary(x => x.Key, x => Prune(x.Value, count)),
            };
        }

        private static (int Page, int Count)[] Prune(
            IEnumerable<(int Page, int Count)> postingList,
            int count)
        {
            return postingList
                .OrderByDescending(x => x.Count)
                .Take(count)
                .OrderBy(x => x.Page)
                .ToArray();
        }
    }
}
