using System;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Driver;
using WikiDownloader.DAL.Mongo;
using WikiDownloader.Services;

namespace WikiDownloader
{
    internal static class Program
    {
        private const string EnApiUrl = "https://en.wikipedia.org/w/api.php";
        private const string SimpleApiUrl = "https://simple.wikipedia.org/w/api.php";
        private const string TinyApiUrl = "https://tn.wikipedia.org/w/api.php";

        private const string MongoDbConnString = "mongodb://localhost:27017/?ssl=false";
        private const string ApContinuePropName = "apContinue";


        internal static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();
            var wikiApiProvider = new WikiApiProvider(httpClient, new Uri(TinyApiUrl));

            var mongoClient = new MongoClient(MongoDbConnString);
            var storage = new MongoWikiDownloadStorage(mongoClient);

            var apContinue = await storage.GetProperty<string>(ApContinuePropName);

            while (apContinue != string.Empty)
            {
                var (pages, newApContinue) = await wikiApiProvider.GetAllPagesBatch(apContinue, null);
                apContinue = newApContinue ?? string.Empty;

                await storage.AddPageTitlesAsync(pages);
                await storage.SetProperty(ApContinuePropName, newApContinue);
            }

            await storage.SetProperty(ApContinuePropName, string.Empty);
        }
    }
}
