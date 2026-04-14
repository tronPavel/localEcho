using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Infrastructure.Data;
using LocalEcho.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public class DistrictRepository : IDistrictRepository
{
    private readonly AppDbContext _context;

    public DistrictRepository(AppDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<District?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Districts.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<District>> GetAllActiveAsync(CancellationToken ct = default)
        => await _context.Districts.Where(d => d.IsActive).ToListAsync(ct);

    public async Task<string?> GetNameByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Districts.Where(d => d.Id == id).Select(d => d.Name).FirstOrDefaultAsync(ct);

    public async Task AddAsync(District district, CancellationToken ct = default)
        => await _context.Districts.AddAsync(district, ct);
    public async Task<District?> GetDistrictByCoordinatesAsync(Point p, CancellationToken ct = default)
    {
        return await _context.Districts
            .FirstOrDefaultAsync(d => d.IsActive && d.Boundaries.Contains(p), ct);
    }
}