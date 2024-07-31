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
using SolarWatch.Services.CoordinatesApi;
using SolarWatch.Services.JsonProcessor;
using SolarWatch.Services.Repositories;
using SolarWatch.Services.SolarApi;

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
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["Roles:User"]).Returns("User");
        _configurationMock.Setup(c => c["Roles:Admin"]).Returns("Admin");


        _dataController =
            new SolarDataController(_loggerMock.Object,
                _solarDataProviderMock.Object,
                _jsonProcessorMock.Object,
                _solarDataRepositoryMock.Object,
                _cityRepositoryMock.Object,
                _cityDataProviderMock.Object,
                _configurationMock.Object);

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
    private Mock<IConfiguration> _configurationMock;
    private SolarDataController _dataController;

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

        // Mock solar phenomena data
        var solarData = new SolarData
        {
            City = city,
            Sunrise = DateTime.Now,
            Sunset = DateTime.Now
        };

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


        _dataController.ControllerContext.HttpContext.User = CreateUserPrincipal("Admin");

        // Act
        var result = await _dataController.GetCurrent(cityName, date, countryName, stateName);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var actualSolarData = okResult.Value as SolarData;
        Assert.NotNull(actualSolarData);
        Assert.That(actualSolarData.Sunrise, Is.EqualTo(expectedSolarPhenomena.SunRise));
        Assert.That(actualSolarData.Sunset, Is.EqualTo(expectedSolarPhenomena.SunSet));
        Assert.That(actualSolarData.City.Name, Is.EqualTo(city.Name));
        Assert.That(actualSolarData.City.Country, Is.EqualTo(city.Country));
        Assert.That(actualSolarData.City.Latitude, Is.EqualTo(city.Latitude));
        Assert.That(actualSolarData.City.Longitude, Is.EqualTo(city.Longitude));
        Assert.That(actualSolarData.City.State, Is.EqualTo(city.State));

        // Verify the Add methods were called
        _cityRepositoryMock.Verify(x => x.Add(It.IsAny<City>()), Times.Once);
        _solarDataRepositoryMock.Verify(x => x.Add(It.IsAny<SolarData>()), Times.Once);
    }
}