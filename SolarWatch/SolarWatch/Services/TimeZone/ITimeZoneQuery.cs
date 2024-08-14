using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.TimeZone;

public interface ITimeZoneQuery
{
    Task<string> GetTimeZone(City city);
}