namespace SolarWatch.Services.CoordinatesApi;

public interface ICityDataProvider
{
    Task<string> GetCoordinates(string city, string country, string? state);
}