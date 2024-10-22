using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SolarWatch.Controller;
using SolarWatch.Model;
using SolarWatch.Model.DbModel;
using SolarWatch.Model.DTO;
using SolarWatch.Services.CoordinatesApi;
using SolarWatch.Services.JsonProcessor;
using SolarWatch.Services.Repositories;
using SolarWatch.Services.SolarApi;
using SolarWatch.Services.TimeZone;

namespace SolarWatch_Tests;

[TestFixture]
public class SolarDataControllerTests
{
    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SolarDataController>>();
        _solarDataProviderMock = new Mock<ISolarDataProvider>();
        _jsonProcessorMock = new Mock<IJsonProcessor>();
        _cityRepositoryMock = new Mock<ICityRepository>();
        _solarDataRepositoryMock = new Mock<ISolarDataRepository>();
        _cityDataProviderMock = new Mock<ICityDataProvider>();
        _timeZoneQueryMock = new Mock<ITimeZoneQuery>();


        _dataController =
            new SolarDataController(_loggerMock.Object,
                _solarDataProviderMock.Object,
                _jsonProcessorMock.Object,
                _solarDataRepositoryMock.Object,
                _cityRepositoryMock.Object,
                _cityDataProviderMock.Object,
                _timeZoneQueryMock.Object);

        // Set up the HttpContext for the controller
        var httpContext = new DefaultHttpContext
        {
            User = CreateUserPrincipal("User") // Default to "User" role
        };
        _dataController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            ActionDescriptor = new ControllerActionDescriptor()
        };
    }

    private Mock<ILogger<SolarDataController>> _loggerMock;
    private Mock<ISolarDataProvider> _solarDataProviderMock;
    private Mock<IJsonProcessor> _jsonProcessorMock;
    private Mock<ICityRepository> _cityRepositoryMock;
    private Mock<ISolarDataRepository> _solarDataRepositoryMock;
    private Mock<ICityDataProvider> _cityDataProviderMock;
    private SolarDataController _dataController;
    private Mock<ITimeZoneQuery> _timeZoneQueryMock;

    private ClaimsPrincipal CreateUserPrincipal(string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }

    [Test]
    public async Task GetCurrentReturnsNotFoundResultIfSunriseSunsetDataProviderFails()
    {
        // Arrange
        _solarDataProviderMock.Setup(x => x.GetCurrent(It.IsAny<City>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception());

        _dataController.ControllerContext.HttpContext.User = CreateUserPrincipal("User");

        // Act
        var result = await _dataController.GetCurrent("city", DateTime.Now, "country", "state");

        // Assert
        Assert.IsInstanceOf(typeof(NotFoundObjectResult), result.Result);
    }

    [Test]
    public async Task GetCurrentReturnsOkResultIfSunriseSunsetDataIsValid()
    {
        // Arrange
        var cityName = "city";
        var countryName = "country";
        var stateName = "state";
        var date = DateTime.Now;

        // Mock city data
        var city = new City
        {
            Name = cityName,
            Country = countryName,
            Latitude = 10.0,
            Longitude = 20.0,
            State = stateName ?? string.Empty
        };

        var coordinateData = "{\"lat\": 10.0, \"lon\": 20.0}";
        var coordinate = new Coordinate(10.0, 20.0);

        var solarPhenomenaData = "{}";
        var expectedSolarPhenomena = new SolarPhenomena(DateTime.Now, DateTime.Now, cityName);

        // Setup mocks
        _cityRepositoryMock.Setup(x => x.GetByName(cityName, countryName, stateName))
            .ReturnsAsync((City)null); // Simulate city not found
        _cityDataProviderMock.Setup(x => x.GetCoordinates(cityName, countryName, stateName))
            .ReturnsAsync(coordinateData);
        _jsonProcessorMock.Setup(x => x.ProcessCoordinates(coordinateData))
            .Returns(coordinate);
        _cityRepositoryMock.Setup(x => x.Add(It.IsAny<City>()));

        _solarDataRepositoryMock.Setup(x => x.GetByCityByDate(It.IsAny<City>(), It.IsAny<DateTime>()))
            .ReturnsAsync((SolarData)null); // Simulate solar data not found
        _solarDataProviderMock.Setup(x => x.GetCurrent(It.IsAny<City>(), It.IsAny<DateTime>()))
            .ReturnsAsync(solarPhenomenaData);
        _jsonProcessorMock.Setup(x => x.Process(solarPhenomenaData, cityName))
            .Returns(expectedSolarPhenomena);
        _solarDataRepositoryMock.Setup(x => x.Add(It.IsAny<SolarData>()));
        _timeZoneQueryMock.Setup(x => x.GetTimeZone(It.IsAny<City>()));


        _dataController.ControllerContext.HttpContext.User = CreateUserPrincipal("Admin");

        // Act
        var result = await _dataController.GetCurrent(cityName, date, countryName, stateName);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var resultValue = okResult.Value as SolarDataWTimeZoneDto;
        Assert.NotNull(resultValue);
        Assert.That(resultValue.SolarData.Sunrise, Is.EqualTo(expectedSolarPhenomena.SunRise));
        Assert.That(resultValue.SolarData.Sunset, Is.EqualTo(expectedSolarPhenomena.SunSet));
        Assert.That(resultValue.SolarData.City.Name, Is.EqualTo(city.Name));
        Assert.That(resultValue.SolarData.City.Country, Is.EqualTo(city.Country));
        Assert.That(resultValue.SolarData.City.Latitude, Is.EqualTo(city.Latitude));
        Assert.That(resultValue.SolarData.City.Longitude, Is.EqualTo(city.Longitude));
        Assert.That(resultValue.SolarData.City.State, Is.EqualTo(city.State));

        // Verify the Add methods were called
        _cityRepositoryMock.Verify(x => x.Add(It.IsAny<City>()), Times.Once);
        _solarDataRepositoryMock.Verify(x => x.Add(It.IsAny<SolarData>()), Times.Once);
    }
}