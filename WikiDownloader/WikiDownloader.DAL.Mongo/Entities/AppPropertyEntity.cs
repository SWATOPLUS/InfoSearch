using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WikiDownloader.DAL.Mongo.Entities
{
    public class AppPropertyEntity<T>
    {
        [BsonId]
        public string Name { get; set; }

        public T Value { get; set; }
    }
}
