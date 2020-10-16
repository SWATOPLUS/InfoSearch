#nullable enable
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using WikiDownloader.Services.Models;

namespace WikiDownloader.Services
{
    public class WikiApiProvider
    {
        private static string BuildAllPagesQuery(string? apContinue, int? limit)
        {
            var queryString = HttpUtility.ParseQueryString("action=query&format=json&list=allpages");
            queryString.Add("aplimit", limit?.ToString() ?? "max");
            queryString.Add("apcontinue", apContinue);

            return queryString.ToString() ?? throw new InvalidOperationException();
        }

        private readonly HttpClient _httpClient;
        private readonly Uri _apiUri;

        public WikiApiProvider(HttpClient httpClient, Uri apiUri)
        {
            _apiUri = apiUri;
            _httpClient = httpClient;
        }

        public async Task<(string[] pages, string? newApContinue)> GetAllPagesBatch(string? apContinue, int? limit)
        {
            var uriBuilder = new UriBuilder(_apiUri)
            {
                Query = BuildAllPagesQuery(apContinue, limit)
            };

            var result = await _httpClient
                .GetFromJsonAsync<WikiApiQueryResult<WikiApiAllPages>>(uriBuilder.Uri);

            var pages = result.Query.AllPages
                .Select(x => x.Title)
                .ToArray();

            var newApContinue = result.Continue?.ApContinue;

            return (pages, newApContinue);
        }
    }
}
