using System;
using MongoDB.Bson.Serialization.Attributes;

namespace WikiDownloader.DAL.Mongo.Entities
{
    public class WikiPageTitleEntity
    {
        [BsonId]
        public string Name { get; set; }
        
        public string ReferenceName { get; set; }

        public bool IsProcessed { get; set; }

        public DateTime ProcessingTime { get; set; }
    }
}
