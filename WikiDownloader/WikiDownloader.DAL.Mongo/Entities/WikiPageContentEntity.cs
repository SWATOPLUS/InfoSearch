using MongoDB.Bson.Serialization.Attributes;

namespace WikiDownloader.DAL.Mongo.Entities
{
    internal class WikiPageContentEntity
    {
        [BsonId]
        public string Name { get; set; }

        public string Content { get; set; }
    }
}
