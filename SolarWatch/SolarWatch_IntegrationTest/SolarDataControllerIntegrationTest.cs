using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using SolarWatch.Contracts;
using SolarWatch.Model;
using SolarWatch.Model.DbModel;
using SolarWatch.Model.DTO;
using Xunit.Abstractions;

namespace SolarWatch_IntegrationTest;

[Collection("IntegrationTests")]
public class MyControllerIntegrationTest
{
    private readonly SolarWatchWebApplicationFactory _app;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper output;

    public MyControllerIntegrationTest(ITestOutputHelper output)
    {
        _app = new SolarWatchWebApplicationFactory();
        _client = _app.CreateClient();
        this.output = output;
    }


    [Fact]
    public async Task TestGetCurrentEndPoint()
    {
        var loginRequest = new AuthRequest("admin@admin.com", "Password123!");
        var loginResponse = await _client.PostAsync("api/Auth/Login",
            new StringContent(JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8, "application/json"));
        var authResponse = JsonConvert.DeserializeObject<AuthResponse>(await loginResponse.Content.ReadAsStringAsync());
        var userToken = authResponse.Token;
        
        // Assert
        Assert.NotNull(authResponse.Token);
        Assert.Equal("admin@admin.com", authResponse.Email);
        Assert.Equal("admin", authResponse.UserName);
        output.WriteLine(userToken);

         _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
         //Make the request to the endpoint
        var response = await _client.GetAsync( "api/SolarData/Budapest/2024-07-29?countryName=Hungary");
        response.EnsureSuccessStatusCode();
        
        var data = await response.Content.ReadFromJsonAsync<SolarDataWTimeZoneDto>();
        
        var expected = new SolarDataWTimeZoneDto()
        {
           SolarData = new SolarData()
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
            },
            TimeZoneData = new TimeZoneData("CEST", 7200)
        };
        //Assert
        Assert.Equal(expected.SolarData.City.Name, data.SolarData.City.Name);
        Assert.Equal(expected.SolarData.City.Country, data.SolarData.City.Country);
        Assert.Equal(expected.SolarData.City.State, data.SolarData.City.State);
        Assert.Equal(expected.SolarData.City.Latitude, data.SolarData.City.Latitude);
        Assert.Equal(expected.SolarData.City.Longitude, data.SolarData.City.Longitude);
        Assert.Equal(expected.SolarData.Sunrise, data.SolarData.Sunrise);
        Assert.Equal(expected.SolarData.Sunset, data.SolarData.Sunset);
    }
    
    [Fact]
    public async Task TestUserLogin()
    {
        var loginRequest = new AuthRequest("teszt@teszt.com", "asd123");
        var loginResponse = await _client.PostAsync("api/Auth/Login",
            new StringContent(JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8, "application/json"));
        var authResponse = JsonConvert.DeserializeObject<AuthResponse>(await loginResponse.Content.ReadAsStringAsync());
        
        // Assert
        Assert.NotNull(authResponse.Token);
        Assert.Equal("teszt@teszt.com", authResponse.Email);
        Assert.Equal("tesztelek", authResponse.UserName);
    }
    
}