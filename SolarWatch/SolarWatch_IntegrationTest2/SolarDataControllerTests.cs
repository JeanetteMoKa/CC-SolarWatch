using System.Net.Http.Json;
using SolarWatch.Model.DbModel;
using Xunit.Abstractions;

namespace SolarWatch_IntegrationTest2;

[Collection("Integration")]
public class SolarDataControllerTests
{
    
    private readonly ITestOutputHelper _testOutputHelper;

    public SolarDataControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetSolarData()
    {
        var app = new SolarWatchApiFactory();
        var client = app.CreateClient();

        var response = await client.GetAsync("api/SolarData/Budapest/2024-07-29?countryName=Hungary");
        
        response.EnsureSuccessStatusCode();

        var solarWatch = await response.Content.ReadFromJsonAsync<SolarData>();

        var expected = new SolarData
        {
            City = new City
            {
                Name = "Budapest",
                Country = "Hungary",
                Latitude = 47.4979937,
                Longitude = 19.0403594,
            },
            Sunrise = new DateTime(2024, 7, 29, 5, 18, 43),
            Sunset = new DateTime(2024, 7, 29, 20, 22, 15)
        };
        Assert.NotNull(solarWatch);
        Assert.Equal(solarWatch.City, expected.City);
        Assert.Equal(solarWatch.Sunrise, expected.Sunrise);
    }
}