using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MongoDB.Driver;
using WikiDownloader.DAL.Mongo;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace WikiDownloader.PageExtractor
{
    internal static class Program
    {
        private const string MongoDbConnString = "mongodb://localhost:27017/?ssl=false";
        private const string ArticleListFileName = "../assets/selected_docs.tsv";
        private const string ExtractInfoOutputFileName = "../assets/extract-info.json";
        private const string ExtractedPagesOutputFileName = "../assets/pages.json";

        private static async Task Main()
        {
            var mongoClient = new MongoClient(MongoDbConnString);
            var storage = new MongoWikiDownloadStorage(mongoClient);

            var links = await File.ReadAllLinesAsync(ArticleListFileName);
            var titles = await storage.GetAllPageTitlesAsync();
            var titlesDict = titles.ToDictionary(x => x.Name);

            var refs = new List<string>();
            var errors = new List<string>();
            var pages = new Dictionary<string, string>();

            foreach (var link in links)
            {
                var name = link.Replace("_", " ");

                if (!titlesDict.ContainsKey(name))
                {
                    errors.Add(link);

                    continue;
                }

                var title = titlesDict[name];

                var content = await storage.GetPageContent(title.ReferenceName ?? title.Name);

                if (content == null)
                {
                    errors.Add(link);

                    continue;
                }

                var document = new HtmlDocument();
                document.LoadHtml(content);

                pages[link] = document.DocumentNode.InnerText;
            }

            var extractInfo = new { errors, refs };

            await File.WriteAllTextAsync(ExtractInfoOutputFileName, JsonConvert.SerializeObject(extractInfo));
            await File.WriteAllTextAsync(ExtractedPagesOutputFileName, JsonConvert.SerializeObject(pages));
        }
    }
}
