using ApiPaises013.Data;
using ApiPaises013.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace ApiPaises013.Servico
{
    public class PaisesService
    {
        private readonly IMongoCollection<Paises> _paises;


        public PaisesService(IMongoDbrep settings)
        {
            var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var database = client.GetDatabase("apiendereco");
            _paises = database.GetCollection<Paises>("paises");
        }

        public List<Paises> Get() =>
            _paises.Find(_ => true).ToList();
    }
}
