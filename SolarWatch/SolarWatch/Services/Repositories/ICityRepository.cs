using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.Repositories;

public interface ICityRepository
{
    public Task<IEnumerable<City>> GetAll();
    public Task<City?> GetByName(string name, string country, string? state);
    public Task<City?> GetById(int id);
    public Task Add(City city);
    public Task Delete(City city);
    public Task Update(City city);
}