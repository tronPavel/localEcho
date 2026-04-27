using LocalEcho.Core.Entities;
using NetTopologySuite.Geometries;

namespace LocalEcho.Core.Interfaces;

public interface IDistrictRepository
{
    Task<District?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<District>> GetAllActiveAsync(CancellationToken ct = default);
    Task<string?> GetNameByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(District district, CancellationToken ct = default);
    Task<District?> GetDistrictByCoordinatesAsync(Point p, CancellationToken ct = default);
    Task<bool> IsPointInDistrictAsync(Point p, Guid districtId);
    Task<DistrictAnalytics> GetAnalyticsAsync(Guid districtId, CancellationToken ct = default);
    Task<bool> IsOverlappingOtherDistrictsAsync(Guid districtId, Polygon boundaries);
}