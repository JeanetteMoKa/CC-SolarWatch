using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.SolarApi;

public class SolarOrgApi : ISolarDataProvider
{
    private readonly ILogger<SolarOrgApi> _logger;

    public SolarOrgApi(ILogger<SolarOrgApi> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetCurrent(City city, DateTime date)
    {
        var url =
            $"https://api.sunrise-sunset.org/json?lat={city.Latitude}&lng={city.Longitude}&formatted=0&date={date:yyyy-MM-dd}&tzid=Europe/Budapest";


        using var client = new HttpClient();
        _logger.LogInformation("Calling OpenWeather API with url: {url}", url);
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}