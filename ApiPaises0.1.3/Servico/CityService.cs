using ApiPaises013.Data;
using ApiPaises013.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace ApiPaises013.Servico
{
    public class CityService
    {
        private readonly IMongoCollection<City> _city;

        public CityService(IMongoDbrep settings)
        {
            var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var database = client.GetDatabase("apiendereco");
            _city = database.GetCollection<City>("city");

        }
        public List<City> Get() =>
            _city.Find(_ => true).ToList();

        public List<City> GetByCountryRegion(string country,string region) {
            return _city.Find(_ => _.Country == country && _.Region.Contains(region)).ToList();
        }

    }
}
