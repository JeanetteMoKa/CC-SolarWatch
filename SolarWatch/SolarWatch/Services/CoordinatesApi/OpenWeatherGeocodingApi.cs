using System.Globalization;
using CsvHelper;
using SolarWatch.Model;

namespace SolarWatch.Services.CoordinatesApi;

public class OpenWeatherGeocodingApi : ICityDataProvider
{
    private static readonly string WorkDir = AppDomain.CurrentDomain.BaseDirectory;
    
    private readonly ILogger<OpenWeatherGeocodingApi> _logger;

    public OpenWeatherGeocodingApi(ILogger<OpenWeatherGeocodingApi> logger, IConfiguration configuration)
    {
        _logger = logger;
    }

    public async Task<string> GetCoordinates(string city, string country, string? state)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_WEATHER_APIKEY");
        _logger.LogInformation(apiKey);

        var filePathCountries = Path.Combine(WorkDir, "Resources", "CountryCodesISO3166.csv");

        if (!File.Exists(filePathCountries))
        {
            _logger.LogError("File not found: {filePathCountries}", filePathCountries);
            throw new FileNotFoundException("Country codes file not found.", filePathCountries);
        }

        var filePathStates = Path.Combine(WorkDir, "Resources", "StateCodes.csv");

        if (!File.Exists(filePathStates))
        {
            _logger.LogError("File not found: {filePathStates}", filePathStates);
            throw new FileNotFoundException("State codes file not found.", filePathStates);
        }

        using var readerCountries = new StreamReader(filePathCountries);
        using var csvReaderCountries = new CsvReader(readerCountries, CultureInfo.InvariantCulture);

        var countryCodesEnumerable = csvReaderCountries.GetRecords<CountryCodes>();

        using var readerStates = new StreamReader(filePathStates);
        using var csvReaderStates = new CsvReader(readerStates, CultureInfo.InvariantCulture);

        var stateCodesEnumerable = csvReaderStates.GetRecords<StateCodes>();

        try
        {
            var cc = countryCodesEnumerable.FirstOrDefault(x => x.Name.ToLower().Contains(country.ToLower()));
            var sc = state != null
                ? $"{stateCodesEnumerable.FirstOrDefault(x => x.StateName.ToLower().Contains(state.ToLower())).StateCode},"
                : "";

            var url = country.ToLower() == "usa" || country.ToLower() == "united states of america"
                ? $"http://api.openweathermap.org/geo/1.0/direct?q={city},{sc}{cc.Alpha2}&appid={apiKey}"
                : $"http://api.openweathermap.org/geo/1.0/direct?q={city},{cc.Alpha2}&appid={apiKey}";

            using var client = new HttpClient();
            _logger.LogInformation("Calling OpenWeather API with url: {url}", url);

            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            _logger.LogError("Country code not found for country: {country}", country);
            throw new Exception($"Country code not found for country: {country}");
        }
    }
}