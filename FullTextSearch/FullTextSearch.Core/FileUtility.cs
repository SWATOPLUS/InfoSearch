using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace FullTextSearch.Core
{
    public static class FileUtility
    {
        public static IEnumerable<string[]> ReadTsv(string path)
        {
            return File.ReadLines(path)
                .Select(line => line.Split('\t'));
        }

        public static T ReadJson<T>(string path)
        {
            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void WriteJson(string path, object value, bool isPretty = false)
        {
            var json = JsonConvert.SerializeObject(value, isPretty ? Formatting.Indented : Formatting.None);

            File.WriteAllText(path, json);
        }
    }
}
