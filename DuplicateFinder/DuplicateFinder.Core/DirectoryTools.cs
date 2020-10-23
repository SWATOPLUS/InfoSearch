using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DuplicateFinder.Core
{
    public static class DirectoryTools
    {
        public static Dictionary<string, string> LoadDirectory(string directory)
        {
            var result = new Dictionary<string, string>();
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var text = File.ReadAllText(file);

                result.Add(name, text);
            }

            return result;
        }

        public static void SaveAsJson(object data, string fileName)
        {
            var text = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(fileName, text);
        }

        public static Dictionary<string, string> ReadStringDictionary(string fileName)
        {
            var text = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
        }
    }
}
