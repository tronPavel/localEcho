using LocalEcho.Application.Dtos;

namespace LocalEcho.Core.Interfaces;

public interface IOfficialService
{
    Task<IEnumerable<MarkerWorkItemDto>> GetQueueAsync(WorkItemsQueryParams query, CancellationToken ct);
}