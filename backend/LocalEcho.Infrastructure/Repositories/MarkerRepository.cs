using LocalEcho.Core.Entities;
using LocalEcho.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LocalEcho.Infrastructure.Repositories;

public class MarkerRepository : IMarkerRepository
{
    private readonly AppDbContext _context;

    public MarkerRepository(AppDbContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Marker?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Markers.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IEnumerable<Marker>> GetAllAsync(CancellationToken ct = default)
        => await _context.Markers.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Marker marker, CancellationToken ct = default)
        => await _context.Markers.AddAsync(marker, ct);

    public void Update(Marker marker)
        => _context.Markers.Update(marker);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}