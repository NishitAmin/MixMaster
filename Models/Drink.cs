using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Mix_Master.Models
{
    public class Drink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }
    }
}