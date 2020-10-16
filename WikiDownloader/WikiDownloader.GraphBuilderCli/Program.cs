using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MongoDB.Bson;
using MongoDB.Driver;
using WikiDownloader.DAL.Mongo;
using WikiDownloader.GraphBuilderCli.Models;
using WikiDownloader.GraphBuilderCli.Services;

namespace WikiDownloader.GraphBuilderCli
{
    internal static class Program
    {
        private const string MongoDbConnString = "mongodb://localhost:27017/?ssl=false";
        private const string NodeKeysInfoOutputFileName = "nodes.output.json";
        private const string EdgesOutputFileName = "edges.output.json";

        private static async Task Main(string[] args)
        {
            var mongoClient = new MongoClient(MongoDbConnString);
            var storage = new MongoWikiDownloadStorage(mongoClient);

            var titles = await storage.GetAllPageTitlesAsync();
            var nodeKeysInfo = NodeKeysInfoCalculator.Build(titles);

            File.WriteAllText(NodeKeysInfoOutputFileName, nodeKeysInfo.ToJson());

            var edges = new Dictionary<string, string[]>();

            foreach (var title in nodeKeysInfo.Regular)
            {
                var content = await storage.GetPageContent(title);
                
                edges[title] = ExtractReferences(content)
                    .Select(x => MapReference(nodeKeysInfo, x))
                    .Where(x => x != null)
                    .ToArray();
            }

            File.WriteAllText(EdgesOutputFileName, edges.ToJson());
        }

        private static string MapReference(NodeKeysInfo info, string title)
        {
            if (info.Regular.Contains(title))
            {
                return title;
            }

            info.References.TryGetValue(title, out var reference);

            return reference;
        }

        private static IEnumerable<string> ExtractReferences(string content)
        {
            var document = new HtmlDocument();
            document.LoadHtml(content);

            var redirectNodes = document.DocumentNode
                .SelectNodes("//a")
                ?.Where(x => x.Attributes["href"]?.Value?.StartsWith("/wiki/") == true)
                .Select(x => x.Attributes["title"]?.Value)
                .Where(x => x != null)
                .ToArray();

            if (redirectNodes?.Any() == true)
            {
                return redirectNodes;
            }

            return Array.Empty<string>();
        }
    }
}
