using DuplicateFinder.Core;

namespace DuplicateFinder
{
    internal static class Program
    {
        private const string InputDirectory = "../../assets/sources";
        private const string OutputFile = "../../assets/sources.json";

        private static void Main()
        {
            var documents = DirectoryTools.LoadDirectory(InputDirectory);

            DirectoryTools.SaveAsJson(documents, OutputFile);
        }
    }
}
