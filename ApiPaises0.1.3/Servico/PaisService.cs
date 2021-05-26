using ApiPaises013.Data;
using ApiPaises013.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPaises013.Servico
{
    public class PaisService
    {
        private readonly IMongoCollection<Pais> _pais;

        public PaisService(IMongoDbrep settings)
        {

            var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var database = client.GetDatabase("apiendereco");
            _pais = database.GetCollection<Pais>("pais");

        }

        public List<Pais> Get() =>
            _pais.Find(_ => true).ToList();

    }
}
