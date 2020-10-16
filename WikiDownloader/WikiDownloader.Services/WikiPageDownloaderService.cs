using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using WikiDownloader.Abstractions.Models;

namespace WikiDownloader.Services
{
    public class WikiPageDownloaderService
    {
        private readonly Uri _wikiUrl;

        private static string BuildQuery(string name)
        {
            var queryString = HttpUtility.ParseQueryString("redirect=no");
            queryString.Add("title", name);

            return queryString.ToString();
        }

        public WikiPageDownloaderService(Uri wikiUrl)
        {
            _wikiUrl = wikiUrl;
        }

        public async Task<WikiContentResult> GetPageContent(string name)
        {
            var uriBuilder = new UriBuilder(_wikiUrl)
            {
                Query = BuildQuery(name),
            };

            var document = await new HtmlWeb().LoadFromWebAsync(uriBuilder.Uri.ToString());
            var content = document.GetElementbyId("content");

            var redirectNodes = content
                .SelectNodes("//*[contains(@class,'redirectText')]//a");

            if (redirectNodes?.Count == 1)
            {
                var node = redirectNodes.Single();

                return new WikiContentResult
                {
                    Reference = node.Attributes["title"].Value,
                };
            }

            return new WikiContentResult
            {
                Content = content.InnerHtml,
            };
        }
    }
}
