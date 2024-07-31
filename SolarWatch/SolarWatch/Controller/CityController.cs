using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model.DbModel;
using SolarWatch.Services.CoordinatesApi;
using SolarWatch.Services.JsonProcessor;
using SolarWatch.Services.Repositories;

namespace SolarWatch.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CityController : ControllerBase
{
    private readonly ICityDataProvider _cityDataProvider;
    private readonly ICityRepository _cityRepository;
    private readonly IConfiguration _configuration;
    private readonly IJsonProcessor _jsonProcessor;

    private readonly ILogger<CityController> _logger;

    public CityController(ILogger<CityController> logger, ICityDataProvider cityDataProvider,
        IJsonProcessor jsonProcessor, ICityRepository cityRepository, IConfiguration configuration)
    {
        _logger = logger;
        _cityDataProvider = cityDataProvider;
        _jsonProcessor = jsonProcessor;
        _cityRepository = cityRepository;
        _configuration = configuration;
    }


    [HttpGet("{countryName}/{cityName}")]
    [Authorize(Policy = "RequiredUserOrAdminRole")]
    public async Task<ActionResult<City>> GetCurrent(string cityName, string countryName, string? stateName)
    {
        try
        {
            var city = await _cityRepository.GetByName(cityName, countryName, stateName);

            if (city == null)
            {
                    var coordinateRaw = await _cityDataProvider.GetCoordinates(cityName, countryName, stateName);
                    var coordinate = _jsonProcessor.ProcessCoordinates(coordinateRaw);
                    city = new City
                    {
                        Name = cityName,
                        Country = countryName,
                        Latitude = coordinate.Lat,
                        Longitude = coordinate.Lon,
                        State = stateName ?? string.Empty
                    };
                    await _cityRepository.Add(city);
                
            }

            return Ok(city);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting coordinates");
            return NotFound("Error getting coordinates");
        }
    }

    [HttpPost("AddNew/{cityName}")]
    [Authorize(Policy = "RequiredAdminRole")]
    public async Task<ActionResult<City>> AddCityToDb(string cityName, string countryName, string? stateName)
    {
        var city = await _cityRepository.GetByName(cityName, countryName, stateName);

        if (city != null)
            throw new InvalidOperationException(
                "A city with the same name, country, and state already exists in the database.");

        try
        {
            var coordinateRaw = await _cityDataProvider.GetCoordinates(cityName, countryName, stateName);
            var coordinate = _jsonProcessor.ProcessCoordinates(coordinateRaw);

            var newCity = new City
            {
                Name = cityName,
                Country = countryName,
                Latitude = coordinate.Lat,
                Longitude = coordinate.Lon,
                State = stateName ?? string.Empty
            };

            await _cityRepository.Add(newCity);

            return newCity;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding city");
            return NotFound("Error adding city");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequiredAdminRole")]
    public async Task<ActionResult> RemoveCityFromDb(int id)
    {
        try
        {
            var city = await _cityRepository.GetById(id);
            if (city == null) return NotFound("City wasn't found.");
            await _cityRepository.Delete(city);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting city data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequiredAdminRole")]
    public async Task<ActionResult> UpdateCityInDb(int id, string cityName, string countryName, string? stateName)
    {
        var city = await _cityRepository.GetById(id);
        if (city == null) return NotFound("City wasn't found.");

        var coordinateRaw = await _cityDataProvider.GetCoordinates(cityName, countryName, stateName);
        var coordinate = _jsonProcessor.ProcessCoordinates(coordinateRaw);

        city.Name = cityName;
        city.Country = countryName;
        city.Latitude = coordinate.Lat;
        city.Longitude = coordinate.Lon;
        city.State = stateName ?? string.Empty;

        await _cityRepository.Update(city);
        return Ok();
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "RequiredUserOrAdminRole")]
    public async Task<ActionResult<City>> GetCityById(int id)
    {
        var city = await _cityRepository.GetById(id);
        if (city == null) return NotFound();
        return Ok(city);
    }

    [HttpGet("All")]
    [Authorize(Policy = "RequiredUserOrAdminRole")]
    public async Task<ActionResult<IEnumerable<City>>> GetAllCities(int id)
    {
        var cities = await _cityRepository.GetAll();
        if (cities == null) return NotFound();
        return Ok(cities);
    }
}