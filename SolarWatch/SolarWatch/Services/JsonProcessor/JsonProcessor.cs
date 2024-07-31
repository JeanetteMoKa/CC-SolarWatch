using System.Text.Json;
using SolarWatch.Model;

namespace SolarWatch.Services.JsonProcessor;

public class JsonProcessor : IJsonProcessor
{
    public SolarPhenomena Process(string data, string city)
    {
        var json = JsonDocument.Parse(data);
        var results = json.RootElement.GetProperty("results");

        var phenomena = new SolarPhenomena(
            GetDate(results.GetProperty("sunrise").GetString()),
            GetDate(results.GetProperty("sunset").GetString()),
            city
        );

        return phenomena;
    }

    public Coordinate ProcessCoordinates(string data)
    {
        var json = JsonDocument.Parse(data);
        var result = json.RootElement[0];

        var coordinate = new Coordinate(
            result.GetProperty("lat").GetDouble(),
            result.GetProperty("lon").GetDouble()
        );

        return coordinate;
    }

    private DateTime GetDate(string date)
    {
        return DateTime.Parse(date);
    }
}