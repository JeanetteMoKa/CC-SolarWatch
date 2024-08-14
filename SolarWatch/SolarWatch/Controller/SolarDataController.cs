using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model.DbModel;
using SolarWatch.Services.CoordinatesApi;
using SolarWatch.Services.JsonProcessor;
using SolarWatch.Services.Repositories;
using SolarWatch.Services.SolarApi;

namespace SolarWatch.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolarDataController : ControllerBase
{
    private readonly ICityDataProvider _cityDataProvider;
    private readonly ICityRepository _cityRepository;
    private readonly IJsonProcessor _jsonProcessor;
    private readonly ILogger<SolarDataController> _logger;
    private readonly ISolarDataProvider _solarDataProvider;
    private readonly ISolarDataRepository _solarDataRepository;

    public SolarDataController(ILogger<SolarDataController> logger,
        ISolarDataProvider solarDataProvider, IJsonProcessor jsonProcessor,
        ISolarDataRepository solarDataRepository, ICityRepository cityRepository,
        ICityDataProvider cityDataProvider)
    {
        _logger = logger;
        _solarDataProvider = solarDataProvider;
        _jsonProcessor = jsonProcessor;
        _solarDataRepository = solarDataRepository;
        _cityRepository = cityRepository;
        _cityDataProvider = cityDataProvider;
    }

    [HttpGet("{cityName}/{date:datetime}")]
    [Authorize(Policy = "RequiredUserOrAdminRole")]
    public async Task<ActionResult<SolarData>> GetCurrent(
        [FromRoute] string cityName,
        [FromRoute] DateTime date, 
        [FromQuery] string countryName, 
        [FromQuery] string? stateName)
    {
        try
        {
            Console.WriteLine($"Received date: {date}"); // Log the date for debugging
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

            var solarData = await _solarDataRepository.GetByCityByDate(city, date);

            if (solarData == null)
            {
                    var solarDataRaw = await _solarDataProvider.GetCurrent(city, date);
                    var solarPhenomena = _jsonProcessor.Process(solarDataRaw, cityName);

                    solarData = new SolarData
                    {
                        City = city,
                        Sunrise = solarPhenomena.SunRise,
                        Sunset = solarPhenomena.SunSet
                    };

                    await _solarDataRepository.Add(solarData);
                
            }

            return Ok(solarData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting solar data");
            return NotFound("Error getting solar data");
        }
    }

    [HttpPost("AddNew/{cityName}/{date:datetime}")]
    [Authorize(Policy = "RequiredAdminRole")]
    public async Task<ActionResult<SolarData>> AddNewSolarData(string cityName, string countryName, string? stateName,
        DateTime date)
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

            var solarData = await _solarDataRepository.GetByCityByDate(city, date);

            if (solarData == null)
            {
                var solarDataRaw = await _solarDataProvider.GetCurrent(city, date);
                var solarPhenomena = _jsonProcessor.Process(solarDataRaw, cityName);

                solarData = new SolarData
                {
                    City = city,
                    Sunrise = solarPhenomena.SunRise,
                    Sunset = solarPhenomena.SunSet
                };

                await _solarDataRepository.Add(solarData);
            }

            return Ok(solarData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding solar data");
            return NotFound("Error adding solar data");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RequiredAdminRole")]
    public async Task<ActionResult> DeleteSolarDataById(int id)
    {
        try
        {
            var solarData = await _solarDataRepository.GetById(id);
            if (solarData == null) return NotFound($"No data with id {id}");

            await _solarDataRepository.Delete(solarData);
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting solar data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "RequiredAdminRole")]
    public async Task<ActionResult> UpdateSolarDataById(int id, int cityId, DateTime date)
    {
        try
        {
            var solarData = await _solarDataRepository.GetById(id);
            if (solarData == null) return NotFound($"No data with id {id}");

            var city = await _cityRepository.GetById(cityId);
            if (city == null) return NotFound($"No city with id {cityId}");

            var solarDataRaw = await _solarDataProvider.GetCurrent(city, date);
            var solarPhenomena = _jsonProcessor.Process(solarDataRaw, city.Name);

            solarData.City = city;
            solarData.Sunrise = solarPhenomena.SunRise;
            solarData.Sunset = solarPhenomena.SunSet;

            await _solarDataRepository.Update(solarData);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating solar data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("All")]
    [Authorize(Policy = "RequiredUserOrAdminRole")]
    public async Task<ActionResult<IEnumerable<SolarData>>> GetAll()
    {
        var data = await _solarDataRepository.GetAll();
        return Ok(data);
    }
}