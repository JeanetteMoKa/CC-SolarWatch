using SolarWatch.Model.DbModel;

namespace SolarWatch.Services.Repositories;

public interface ISolarDataRepository
{
    public Task<IEnumerable<SolarData>> GetAll();
    public Task<SolarData?> GetByCityByDate(City city, DateTime date);
    public Task<SolarData?> GetById(int id);
    public Task<IEnumerable<SolarData>> GetByCity(City city);
    public Task Add(SolarData solarData);
    public Task Delete(SolarData solarData);
    public Task Update(SolarData solarData);
}