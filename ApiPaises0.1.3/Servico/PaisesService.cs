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
            //var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var client = new MongoClient("mongodb+srv://renatoads1:r3n4t0321@cluster0.k1iwv.mongodb.net/apiendereco?retryWrites=true&w=majority");
            var database = client.GetDatabase("apiendereco");
            _paises = database.GetCollection<Paises>("paises");
        }

        public List<Paises> Get() =>
            _paises.Find(_ => true).ToList();

        public Paises GetpaisForVar(string pais) {

            return _paises.Find(_ => _.Name.Contains(pais)).FirstOrDefault();
        }

        public List<Paises> GetDez() =>
            _paises.Find(_ => true).Limit(10).ToList();
    }
}
