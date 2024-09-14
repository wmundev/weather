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

        /// <summary>
        /// Retrieves a list of cities in Australia, limited by the specified number of cities.
        /// </summary>
        /// <param name="numberOfCities">The maximum number of cities to return. Defaults to 100. Must not exceed 500.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> containing an <see cref="IEnumerable{City}"/> of cities in Australia.
        /// Returns a <see cref="ProblemDetails"/> if the number of cities exceeds 500.
        /// </returns>
        [HttpGet]
        [Route("all")]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        [ProducesResponseType(200, Type = typeof(IEnumerable<City>))]
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

        /// <summary>
        /// Retrieves information about a specific city by its name.
        /// </summary>
        /// <param name="name">The name of the city to retrieve information for.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> containing the <see cref="DynamoDbCity"/> information.
        /// Returns a <see cref="NotFoundResult"/> if the city is not found.
        /// </returns>
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
