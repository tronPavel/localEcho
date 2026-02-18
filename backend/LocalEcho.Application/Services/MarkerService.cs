using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Models;

namespace LocalEcho.Application.Services;

public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _repository;
    private readonly IUserContext _userContext;

    public MarkerService(IMarkerRepository repository, IUserContext userContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    public async Task<Guid> CreateMarkerAsync(CreateMarkerDto dto)
    {
        if (!_userContext.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated");

        var userId = _userContext.UserId;
        var districtId = _userContext.DistrictId;

        var location = new GeoPoint(dto.Latitude, dto.Longitude);
        
        var marker = Marker.Create(
            dto.Title, 
            location, 
            dto.Category, 
            userId, 
            districtId, 
            dto.Description,
            dto.ImageUrl
        );

        await _repository.AddAsync(marker);
        await _repository.SaveChangesAsync();

        return marker.Id;
    }

    public async Task<MarkerDto> GetByIdAsync(Guid id)
    {
        var m = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException($"Marker {id} not found");
        var currentUserId = _userContext.IsAuthenticated ? _userContext.UserId : (Guid?)null;
        int userVote = 0;
        
        if (currentUserId.HasValue)
        {
            var vote = await _repository.GetVoteAsync(m.Id, currentUserId.Value);
            if (vote != null) userVote = vote.IsUpvote ? 1 : -1;
        }
        return new MarkerDto(
            m.Id, 
            m.Title,
            m.Location.Latitude, 
            m.Location.Longitude,
            m.Description, 
            m.ImageUrl,
            m.Category, 
            m.Status,
            m.CreatedByUserId,
            m.Rating, 
            userVote,
            m.CreatedAt, 
            m.UpdatedAt
        );
    }

    public async Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto)
    {
        // if (_userContext.UserId != marker.CreatedByUserId) throw ...
        
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException();

        marker.UpdateDescription(dto.Description);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(Guid id, MarkerStatus newStatus)
    {
        var marker = await _repository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException();

        marker.ChangeStatus(newStatus);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }
    public async Task VoteAsync(Guid markerId, VoteDto dto)
    {
        if (!_userContext.IsAuthenticated) throw new UnauthorizedAccessException();
        var userId = _userContext.UserId;

        var marker = await _repository.GetByIdAsync(markerId) 
                     ?? throw new KeyNotFoundException("Marker not found");

        var existingVote = await _repository.GetVoteAsync(markerId, userId);

        if (existingVote != null)
        {
            if (existingVote.IsUpvote == dto.IsUpvote)
            {
                _repository.RemoveVote(existingVote);
            }
            else
            {
                existingVote.ChangeType(dto.IsUpvote);
                // _repository.UpdateVote(existingVote); // EF Core сам отследит изменения, но можно явно
            }
        }
        else
        {
            var newVote = new Vote(markerId, userId, dto.IsUpvote);
            await _repository.AddVoteAsync(newVote);
        }

        await _repository.SaveChangesAsync();

        var newRating = await _repository.CalculateRatingAsync(markerId);
        marker.UpdateRating(newRating);
        _repository.Update(marker);
        await _repository.SaveChangesAsync();
    }
    public async Task<IEnumerable<MarkerDto>> GetFilteredMarkersAsync(GetMarkersQueryParams queryParams)
    {
        MarkerCategory? category = null;
        if (!string.IsNullOrEmpty(queryParams.Category) && Enum.TryParse<MarkerCategory>(queryParams.Category, out var c))
            category = c;

        MarkerStatus? status = null;
        if (!string.IsNullOrEmpty(queryParams.Status) && Enum.TryParse<MarkerStatus>(queryParams.Status, out var s))
            status = s;

        var filter = new MarkerFilter
        {
            Category = category,
            Status = status,
            MinLat = queryParams.MinLat,
            MaxLat = queryParams.MaxLat,
            MinLng = queryParams.MinLng,
            MaxLng = queryParams.MaxLng
        };

        var currentUserId = _userContext.IsAuthenticated ? _userContext.UserId : (Guid?)null;

        var markersWithVotes = await _repository.GetFilteredAsync(filter, currentUserId);

        return markersWithVotes.Select(item => new MarkerDto(
            item.Marker.Id,
            item.Marker.Title,
            item.Marker.Location.Latitude,
            item.Marker.Location.Longitude,
            item.Marker.Description,
            item.Marker.ImageUrl,
            item.Marker.Category,
            item.Marker.Status,
            item.Marker.CreatedByUserId,
            item.Marker.Rating,
            item.UserVote, 
            item.Marker.CreatedAt,
            item.Marker.UpdatedAt
        ));
    }
}