using LocalEcho.Application.Dtos;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models;

namespace LocalEcho.Application.Services;

public class OfficialService : IOfficialService
{
    private readonly IMarkerRepository _markerRepo;

    public OfficialService(IMarkerRepository markerRepo) => _markerRepo = markerRepo;

    public async Task<IEnumerable<MarkerWorkItemDto>> GetQueueAsync(WorkItemsQueryParams query, CancellationToken ct)
    {
        var filter = new MarkerWorkFilter(
            query.DistrictId,
            query.Status,
            query.Category,
            query.Limit);
        
        var tasks = await _markerRepo.GetOfficialTasksAsync(filter, ct);

        return tasks.Select(t => new MarkerWorkItemDto(
            t.Id, t.Title, 
            Enum.Parse<MarkerCategory>(t.Category), 
            Enum.Parse<MarkerStatus>(t.Status),
            t.CreatorName,
            query.DistrictId, 
            t.DistrictName,
            t.CreatedAt,
            t.Rating
        ));
    }
}