using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WikiDownloader.Abstractions.Services;
using WikiDownloader.Services;

namespace WikiDownloader.ContentLoaderWeb.HostedServices
{
    public class ContentDownloaderJob : BackgroundService
    {
        private readonly WikiPageDownloaderService _downloaderService;
        private readonly IWikiDownloaderStorage _storage;

        public ContentDownloaderJob(
            WikiPageDownloaderService downloaderService,
            IWikiDownloaderStorage storage
            )
        {
            _downloaderService = downloaderService;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var titles = await _storage.GetQueuedPageTitlesAsync(50, TimeSpan.FromSeconds(30));

                    foreach (var title in titles)
                    {
                        await ProcessTitle(title);
                    }
                }
                catch { }
            }
        }

        private async Task ProcessTitle(string title)
        {
            var result = await _downloaderService.GetPageContent(title);

            if (result.Reference != null)
            {
                await _storage.AddPageReference(title, result.Reference);
            }
            else
            {
                await _storage.AddPageContent(title, result.Content);
            }
        }
    }
}
