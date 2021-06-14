using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApiPaises013.Domain.Entities
{
    public class Paises
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get;private set; }
        [BsonElement("code")]
        public string Code { get; private set; }
        [BsonElement("name")]
        public string Name { get; private set; }

        public Paises(string id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }


        public string PaisesTeste()
        {
            var resp = String.Concat(Id, Code, Name);
            return resp;
        }
    }
}
