using SolarWatch.Model;

namespace SolarWatch.Services.JsonProcessor;

public interface IJsonProcessor
{
    SolarPhenomena Process(string data, string city);

    Coordinate ProcessCoordinates(string data);

    TimeZoneData ProcessTimeZoneData(string data);
}