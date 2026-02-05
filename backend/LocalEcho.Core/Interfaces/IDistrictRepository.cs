using LocalEcho.Core.Entities;

namespace LocalEcho.Core.Interfaces;

public interface IDistrictRepository
{
    Task<District?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<District>> GetAllActiveAsync(CancellationToken ct = default);
    Task<string?> GetNameByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(District district, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}