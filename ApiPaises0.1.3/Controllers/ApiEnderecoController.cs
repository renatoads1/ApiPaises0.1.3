using ApiPaises013.Domain.Entities;
using ApiPaises013.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiPaises013.Servico;

namespace ApiPaises013.Controllers
{

    

    [ApiController]
    [Route("[controller]")]
    public class ApiEnderecoController : ControllerBase
    {
        private readonly PaisService _paisService;
        private readonly RegionService _regionService;
        private readonly ILogger<ApiEnderecoController> _logger;

        public ApiEnderecoController(ILogger<ApiEnderecoController> logger,
            PaisService pais,
            RegionService region)
        {
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

    }
}
