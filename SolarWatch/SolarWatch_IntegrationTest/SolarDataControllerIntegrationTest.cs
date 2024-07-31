using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolarWatch.Contracts;
using SolarWatch.Model.DbModel;
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
        
        var data = await response.Content.ReadFromJsonAsync<SolarData>();
        
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
        //Assert
        Assert.Equal(expected.City.Name, data.City.Name);
        Assert.Equal(expected.City.Country, data.City.Country);
        Assert.Equal(expected.City.State, data.City.State);
        Assert.Equal(expected.City.Latitude, data.City.Latitude);
        Assert.Equal(expected.City.Longitude, data.City.Longitude);
        Assert.Equal(expected.Sunrise, data.Sunrise);
        Assert.Equal(expected.Sunset, data.Sunset);
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
    
    [Fact]
    public async Task TestUserLoginWrongPassword()
    {
        var loginRequest = new AuthRequest("teszt@teszt.com", "idk");
        var loginResponse = await _client.PostAsync("api/Auth/Login",
            new StringContent(JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8, "application/json"));
        
        var response = await loginResponse.Content.ReadAsStringAsync();
        output.WriteLine(response);
        var jsonResponse = JObject.Parse(response);
        // Assert
        Assert.Contains("Invalid password", jsonResponse["Bad credentials"].ToString());
    }
    
    [Fact]
    public async Task TestUserLoginWrongEmail()
    {
        var loginRequest = new AuthRequest("asd@asd.com", "asd123");
        var loginResponse = await _client.PostAsync("api/Auth/Login",
            new StringContent(JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8, "application/json"));
        
        var response = await loginResponse.Content.ReadAsStringAsync();
        output.WriteLine(response);
        var jsonResponse = JObject.Parse(response);
        // Assert
        Assert.Contains("Invalid email", jsonResponse["Bad credentials"].ToString());
    }
}