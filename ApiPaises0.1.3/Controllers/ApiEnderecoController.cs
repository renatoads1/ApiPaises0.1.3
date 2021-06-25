using ApiPaises013.Domain.Entities;
using ApiPaises013.Servico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ApiPaises013.Controllers
{



    [ApiController]
    [Route("[controller]")]
    public class ApiEnderecoController : ControllerBase
    {
        private readonly PaisesService _paisesService;
        private readonly RegionService _regionService;
        private readonly CityService _cityService;
        private readonly ILogger<ApiEnderecoController> _logger;

        public ApiEnderecoController(ILogger<ApiEnderecoController> logger,
            PaisesService paises,
            RegionService region,
            CityService city)
        {
            _cityService = city;
            _regionService = region;
            _paisesService = paises;
            _logger = logger;
        }


        [HttpGet("pais")]
        public ActionResult<List<Paises>> Getpais() { 
            return  _paisesService.Get();
        }
        [HttpGet("pais/{pais}")]
        public ActionResult<Paises> GetpaisForVar(string pais)
        {
            return _paisesService.GetpaisForVar(pais);
        }

        [HttpGet("region")]
        public ActionResult<List<Region>> Getregion()
        { 
            return _regionService.Get();
        }

        [HttpGet("region/{codPais}")]
        public ActionResult<List<Region>> Getregion(string codPais)
        {
            return _regionService.GetByCountry(codPais);
        }

        [HttpGet("city")]
        public ActionResult<List<City>> Get()
        {
            return _cityService.Get();
        }

        [HttpGet("city/{codPais}/{region}")]
        public ActionResult<List<City>> GetByCity(string codPais,string region)
        {
            return _cityService.GetByCountryRegion(codPais,region);
        }
    }
}
