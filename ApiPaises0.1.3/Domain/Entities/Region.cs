using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiPaises013.Domain.Entities
{
    public class Region
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }
        [BsonElement("region")]
        public string region { get; private set; }
        [BsonElement("country")]
        public string Country { get; private set; }

        public Region(string id, string region, string country)
        {
            Id = id;
            this.region = region;
            Country = country;
        }
    }
}
