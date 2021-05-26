using ApiPaises013.Data;
using ApiPaises013.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPaises013.Servico
{
    public class RegionService
    {
        private readonly IMongoCollection<Region> _region;

        public RegionService(IMongoDbrep settings)
        {

            var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var database = client.GetDatabase("apiendereco");
            _region = database.GetCollection<Region>("region");

        }
        public List<Region> Get() =>
            _region.Find(_ => true).ToList();

    }
}
