using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiPaises013.Domain.Entities
{
    public class Pais
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get;private set; }
        [BsonElement("name")]
        public string Name { get; private set; }
        [BsonElement("code")]
        public string Code { get; private set; }

        public Pais(string id, string name, string code)
        {
            Id = id;
            Name = name;
            Code = code;
        }
    }
}
