using LocalEcho.Core.Entities;
using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Interfaces;

public interface ICityRepository
{
    Task<City?> GetByIdAsync(Guid id);
    Task<IEnumerable<City>> GetAllAsync();
    Task<City?> GetByCoordinatesAsync(Point p);
    Task AddAsync(City city);
}