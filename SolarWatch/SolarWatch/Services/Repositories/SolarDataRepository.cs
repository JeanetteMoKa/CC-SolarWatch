using SolarWatch.Context;
using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.Repositories;

public class SolarDataRepository(SolarWatchContext context) : ISolarDataRepository
{
    public async Task<IEnumerable<SolarData>> GetAll()
    {
        return context.SolarTimes.ToList();
    }

    public async Task<SolarData?> GetByCityByDate(City city, DateTime date)
    {
        return context.SolarTimes.FirstOrDefault(sD => sD.City == city && sD.Sunrise.Date == date.Date);
    }

    public async Task<SolarData?> GetById(int id)
    {
        return context.SolarTimes.FirstOrDefault(sD => sD.Id == id);
    }

    public async Task<IEnumerable<SolarData>> GetByCity(City city)
    {
        return context.SolarTimes.Where(sD => sD.City == city);
    }

    public async Task Add(SolarData solarData)
    {
        context.Add(solarData);
        context.SaveChanges();
    }

    public async Task Delete(SolarData solarData)
    {
        context.Remove(solarData);
        context.SaveChanges();
    }

    public async Task Update(SolarData solarData)
    {
        context.Update(solarData);
        context.SaveChanges();
    }
}