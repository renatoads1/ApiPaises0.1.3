using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPaises013.Domain.Entities
{
    public class City
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("city")]
        public string city { get; private set; }
        [BsonElement("region")]
        public string Region { get; private set; }
        [BsonElement("country")]
        public string Country { get; private set; }
        [BsonElement("latitude")]
        public string Latitude { get; private set; }
        [BsonElement("longitude")]
        public string Longitude { get; private set; }

        public City(string id, string city, string region, string country, string latitude, string longitude)
        {
            Id = id;
            this.city = city;
            Region = region;
            Country = country;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
