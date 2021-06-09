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
        private readonly PaisService _paisService;
        private readonly RegionService _regionService;
        private readonly CityService _cityService;
        private readonly ILogger<ApiEnderecoController> _logger;

        public ApiEnderecoController(ILogger<ApiEnderecoController> logger,
            PaisService pais,
            RegionService region,
            CityService city)
        {
            _cityService = city;
            _regionService = region;
            _paisService = pais;
            _logger = logger;
        }

        [HttpGet("pais")]
        public ActionResult<List<Pais>> Getpais() { 
            return  _paisService.Get();
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
