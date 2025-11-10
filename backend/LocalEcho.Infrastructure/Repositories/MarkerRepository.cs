using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public class MarkerRepository : IMarkerRepository
{
    private readonly AppDbContext _context;

    public MarkerRepository(AppDbContext context)
    {
        _context = context;//?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(Marker marker)
    {
        var dbMarker = new DataMarker
        {
            Id = marker.Id,
            Title = marker.Title,
            Location = new Point(marker.Location.Longitude, marker.Location.Latitude) { SRID = 4326 },
            Description = marker.Description,
            Category = marker.Category.ToString(),
            Status = marker.Status.ToString(),
            CreatedAt = marker.CreatedAt,
            UpdatedAt = marker.UpdatedAt
        };
        await _context.Markers.AddAsync(dbMarker); // AddAsync: staging (подготовка), не сохраняет сразу.
    }

    public async Task<IEnumerable<Marker>> GetAllAsync()
    {
        var dbMarkers = await _context.Markers.ToListAsync();
        return dbMarkers.Select(dm => new Marker( // Маппинг: Data → Core.
            dm.Title,
            new GeoPoint(dm.Location.Y, dm.Location.X), // Point → GeoPoint: Y=Lat, X=Long.
            Enum.Parse<MarkerCategory>(dm.Category), // String → enum.
            dm.Description
        ) { /* Установка Id, CreatedAt и т.д. через reflection или доп. конструктор в Marker, но для простоты добавьте public setter или метод SetId в Marker. */ }
        ).ToList();
    }

    /*public async Task<Marker> GetByIdAsync(Guid id)
    {
        var dbMarker = await _context.Markers.FirstOrDefaultAsync(m => m.Id == id); // FirstOrDefaultAsync: SQL WHERE Id = @id.
        if (dbMarker == null) return null;
        // Маппинг аналогично GetAll, верните Marker.
        return /* маппинг #1#;
    }

    public async Task UpdateAsync(Marker marker)
    {
        var dbMarker = await _context.Markers.FindAsync(marker.Id); // FindAsync: эффективный поиск по PK.
        if (dbMarker != null)
        {
            // Маппинг обновлений: Title, Location и т.д. (только изменившиеся).
            dbMarker.Title = marker.Title;
            // ... аналогично Add.
            _context.Markers.Update(dbMarker); // Update: marking для SaveChanges.
        }
    }*/

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync(); // Выполняет INSERT/UPDATE в БД. Почему async? Для не-блокирующего I/O.
    }
}