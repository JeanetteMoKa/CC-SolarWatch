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

namespace SolarWatch_Tests;

[TestFixture]
public class CityControllerTests
{
    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CityController>>();
        _cityDataProviderMock = new Mock<ICityDataProvider>();
        _jsonProcessorMock = new Mock<IJsonProcessor>();
        _cityRepositoryMock = new Mock<ICityRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["Roles:User"]).Returns("User");
        _configurationMock.Setup(c => c["Roles:Admin"]).Returns("Admin");

        _controller =
            new CityController(
                _loggerMock.Object,
                _cityDataProviderMock.Object,
                _jsonProcessorMock.Object,
                _cityRepositoryMock.Object,
                _configurationMock.Object
            );

        // Set up the HttpContext for the controller
        var httpContext = new DefaultHttpContext
        {
            User = CreateUserPrincipal("User") // Default to "User" role
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            ActionDescriptor = new ControllerActionDescriptor()
        };
    }

    private Mock<ILogger<CityController>> _loggerMock;
    private Mock<ICityDataProvider> _cityDataProviderMock;
    private Mock<IJsonProcessor> _jsonProcessorMock;
    private Mock<ICityRepository> _cityRepositoryMock;
    private Mock<IConfiguration> _configurationMock;
    private CityController _controller;

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
    public async Task GetCurrentReturnsNotFoundResultIfCityDataProviderFails()
    {
        // Arrange
        _cityDataProviderMock.Setup(x => x.GetCoordinates(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        _controller.ControllerContext.HttpContext.User = CreateUserPrincipal("User");

        // Act
        var result = await _controller.GetCurrent("city", "country", "state");

        // Assert
        Assert.IsInstanceOf(typeof(NotFoundObjectResult), result.Result);
    }

    [Test]
    public async Task GetCurrentReturnsOkResultIfCityDataIsValid()
    {
        // Arrange
        var cityName = "city";
        var countryName = "country";
        var stateName = "state";

        // Mock city data
        var expectedCity = new City
        {
            Name = cityName,
            Country = countryName,
            Latitude = 10.0,
            Longitude = 20.0,
            State = stateName ?? string.Empty
        };

        var coordinateData = "{\"lat\": 10.0, \"lon\": 20.0}";
        var coordinate = new Coordinate(10.0, 20.0);

        // Setup mocks
        _cityRepositoryMock.Setup(x => x.GetByName(cityName, countryName, stateName))
            .ReturnsAsync((City)null); // Simulate city not found
        _cityDataProviderMock.Setup(x => x.GetCoordinates(cityName, countryName, stateName))
            .ReturnsAsync(coordinateData);
        _jsonProcessorMock.Setup(x => x.ProcessCoordinates(coordinateData))
            .Returns(coordinate);
        _cityRepositoryMock.Setup(x => x.Add(It.IsAny<City>()));

        _controller.ControllerContext.HttpContext.User = CreateUserPrincipal("Admin");


        // Act
        var result = await _controller.GetCurrent(cityName, countryName, stateName);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result.Result);
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var actualCity = okResult.Value as City;
        Assert.NotNull(actualCity);
        Assert.That(actualCity.Name, Is.EqualTo(expectedCity.Name));
        Assert.That(actualCity.Country, Is.EqualTo(expectedCity.Country));
        Assert.That(actualCity.Latitude, Is.EqualTo(expectedCity.Latitude));
        Assert.That(actualCity.Longitude, Is.EqualTo(expectedCity.Longitude));
        Assert.That(actualCity.State, Is.EqualTo(expectedCity.State));

        // Verify the Add method was called
        _cityRepositoryMock.Verify(x => x.Add(It.IsAny<City>()), Times.Once);
    }
}