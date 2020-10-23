using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using WikiDownloader.Abstractions.Models;
using WikiDownloader.Abstractions.Services;
using WikiDownloader.DAL.Mongo.Entities;

namespace WikiDownloader.DAL.Mongo
{
    public class MongoWikiDownloadStorage : IWikiDownloaderStorage
    {
        private const string DataBaseName = "simple-wiki-download";

        private const string PropertiesCollection = "properties";
        private const string PageTitlesCollection = "page-titles";
        private const string PageContentCollection = "page-content";

        private readonly IMongoClient _mongoClient;

        public MongoWikiDownloadStorage(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }
        
        public async Task SetProperty<T>(string name, T value)
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<AppPropertyEntity<T>>(PropertiesCollection);

            var entity = new AppPropertyEntity<T>
            {
                Name = name,
                Value = value
            };

            await collection.ReplaceOneAsync(x => x.Name == name, entity,
                new ReplaceOptions {IsUpsert = true});
        }

        public async Task<T> GetProperty<T>(string name)
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<AppPropertyEntity<T>>(PropertiesCollection);

            var entity = await collection
                .AsQueryable()
                .Where(x => x.Name == name)
                .SingleOrDefaultAsync();

            if (entity == null)
            {
                return default;
            }

            return entity.Value;
        }

        public async Task AddPageTitlesAsync(IEnumerable<string> titles)
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageTitleEntity>(PageTitlesCollection);

            var entities = titles.Select(x => new WikiPageTitleEntity
            {
                Name = x,
            });

            foreach (var entity in entities)
            {
                await collection.ReplaceOneAsync(x => x.Name == entity.Name, entity,
                    new ReplaceOptions {IsUpsert = true});
            }
        }

        public async Task<string[]> GetQueuedPageTitlesAsync(int limit, TimeSpan timeout)
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageTitleEntity>(PageTitlesCollection);

            var now = DateTime.UtcNow;
            var skipTime = now.Subtract(timeout);


            var cursor = await collection
                .FindAsync(
                    x => x.ProcessingTime < skipTime && !x.IsProcessed, 
                    new FindOptions<WikiPageTitleEntity, WikiPageTitleEntity>
                {
                    Limit = limit,
                });

            var entities = await cursor.ToListAsync();
            var output = new List<string>();

            foreach (var entity in entities)
            {
                var oldProcessingTime = entity.ProcessingTime;

                entity.ProcessingTime = now;

                var replaceResult = await collection
                    .ReplaceOneAsync(x => x.Name == entity.Name && x.ProcessingTime == oldProcessingTime, entity);

                if (replaceResult.ModifiedCount > 0)
                {
                    output.Add(entity.Name);
                }
            }

            return output.ToArray();
        }

        public async Task<WikiPageTitle[]> GetAllPageTitlesAsync()
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageTitleEntity>(PageTitlesCollection);

            var cursor = await collection
                .FindAsync(x => true);

            var entities = await cursor.ToListAsync();

            return entities
                .Select(MapWikiPageTitle)
                .ToArray();
        }

        public async Task AddPageContent(string name, string content)
        {
            var contentCollection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageContentEntity>(PageContentCollection);

            await contentCollection.ReplaceOneAsync(x => x.Name == name, new WikiPageContentEntity
                {
                    Name = name,
                    Content = content,
                },
                new ReplaceOptions {IsUpsert = true});

            var titleCollection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageTitleEntity>(PageTitlesCollection);

            await titleCollection.ReplaceOneAsync(x => x.Name == name, new WikiPageTitleEntity
                {
                    Name = name,
                    IsProcessed = true,
                },
                new ReplaceOptions {IsUpsert = true});
        }

        public async Task AddPageReference(string source, string destination)
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageTitleEntity>(PageTitlesCollection);

            await collection.ReplaceOneAsync(x => x.Name == source, new WikiPageTitleEntity
                {
                    Name = source,
                    ReferenceName = destination,
                    IsProcessed = true,
                },
                new ReplaceOptions { IsUpsert = true });
        }

        public async Task<string> GetPageContent(string name)
        {
            var collection = _mongoClient
                .GetDatabase(DataBaseName)
                .GetCollection<WikiPageContentEntity>(PageContentCollection);

            var cursor = await collection
                .FindAsync(
                    x => x.Name == name,
                    new FindOptions<WikiPageContentEntity, WikiPageContentEntity>
                    {
                        Limit = 1,
                    });

            var entities = await cursor.ToListAsync();

            return entities.SingleOrDefault()?.Content;
        }

        private static WikiPageTitle MapWikiPageTitle(WikiPageTitleEntity entity)
        {
            return new WikiPageTitle
            {
                IsProcessed = entity.IsProcessed,
                Name = entity.Name,
                ReferenceName = entity.ReferenceName,
            };
        }
    }
}
