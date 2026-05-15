using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public class CityRepository : ICityRepository
{
    private readonly AppDbContext _context;
    public CityRepository(AppDbContext context) => _context = context;

    public async Task<City?> GetByCoordinatesAsync(Point p)
        => await _context.Cities.FirstOrDefaultAsync(c => c.Boundaries.Contains(p));

    public async Task<IEnumerable<City>> GetAllAsync() 
        => await _context.Cities.AsNoTracking().ToListAsync();
    
    public async Task<City?> GetByIdAsync(Guid id) 
        => await _context.Cities.FindAsync(id);

    public async Task AddAsync(City city) => await _context.Cities.AddAsync(city);
}