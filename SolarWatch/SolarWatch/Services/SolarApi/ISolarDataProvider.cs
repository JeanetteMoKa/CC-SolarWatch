using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.SolarApi;

public interface ISolarDataProvider
{
    Task<string> GetCurrent(City city, DateTime date);
}