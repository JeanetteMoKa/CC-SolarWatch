using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.TimeZone;

public class TimeZoneQuery(ILogger<TimeZoneQuery> logger) : ITimeZoneQuery
{
    public async Task<string> GetTimeZone(City city)
    {
        
        try
        {
            var url = $"https://timeapi.io/api/TimeZone/coordinate?latitude={city.Latitude}&longitude={city.Longitude}";

            using var client = new HttpClient();
            logger.LogInformation("Calling timeapi.io with url: {url}", url);

            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            logger.LogError($"Time Zone not found for city: {city.Name}", e);
            throw new Exception($"Time Zone  not found for city: {city.Name}");
        }
    }
}