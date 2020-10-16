using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WikiDownloader.Abstractions.Models;

namespace WikiDownloader.Abstractions.Services
{
    public interface IWikiDownloaderStorage
    {
        Task SetProperty<T>(string name, T value);

        Task<T> GetProperty<T>(string name);

        Task AddPageTitlesAsync(IEnumerable<string> titles);

        Task<string[]> GetQueuedPageTitlesAsync(int limit, TimeSpan timeout);

        Task<WikiPageTitle[]> GetAllPageTitlesAsync();

        Task AddPageContent(string name, string content);

        Task AddPageReference(string source, string destination);

        Task<string> GetPageContent(string name);
    }
}
