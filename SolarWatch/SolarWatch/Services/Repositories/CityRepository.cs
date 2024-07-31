using SolarWatch.Context;
using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.Repositories;

public class CityRepository(SolarWatchContext context) : ICityRepository
{
    public async Task<IEnumerable<City>> GetAll()
    {
        return context.Cities.ToList();
    }

    public async Task<City?> GetByName(string name, string country, string? state)
    {
        if (state == null) return context.Cities.FirstOrDefault(c => c.Name == name && c.Country == country);

        return context.Cities.FirstOrDefault(c => c.Name == name && c.Country == country && state == c.State);
    }

    public async Task<City?> GetById(int id)
    {
        return context.Cities.FirstOrDefault(c => c.Id == id);
    }

    public async Task Add(City city)
    {
        context.Add(city);
        context.SaveChanges();
    }

    public async Task Delete(City city)
    {
        context.Remove(city);
        context.SaveChanges();
    }

    public async Task Update(City city)
    {
        context.Update(city);
        context.SaveChanges();
    }
}