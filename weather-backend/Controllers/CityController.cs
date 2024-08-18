using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weather_backend.Dto;
using weather_backend.Models;
using weather_backend.Services;

namespace weather_backend.Controllers
{
    [ApiController]
    [Route("city")]
    public class CityController(CityList cityList) : ControllerBase
    {
        private readonly CityList _cityList = cityList ?? throw new ArgumentNullException(nameof(cityList));

        [HttpGet]
        [Route("all")]
        public ActionResult<IEnumerable<City>> GetCities([FromQuery(Name = "num")] int numberOfCities = 100)
        {
            if (numberOfCities > 500)
                return BadRequest(new ProblemDetails {Type = "too large", Detail = "size is too large"});

            var allCitiesInAustralia = _cityList.GetAllCitiesInAustralia().Take(numberOfCities);
            return Ok(allCitiesInAustralia);
        }

        // [HttpGet]
        // [Route("/populate")]
        // public async Task<ActionResult<int>> PopulateDynamoDbDatabase()
        // {
        //     await _cityList.PopulateDynamoDbDatabase();
        //     return Ok(1);
        // }

        [HttpGet]
        [Route("{name}")]
        public async Task<ActionResult<DynamoDbCity>> GetCityInformation(string name)
        {
            var city = await _cityList.GetCityInfo(name);
            if (city is null)
            {
                return NotFound();
            }

            return Ok(city);
        }
    }
}
