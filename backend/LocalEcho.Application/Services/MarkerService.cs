using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;

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

    public async Task<IEnumerable<MarkerDto>> GetAllAsync()
    {
        var markers = await _repository.GetAllAsync();
        var currentUserId = _userContext.IsAuthenticated ? _userContext.UserId : (Guid?)null;
    
        // ВАЖНО: Это не супер-эффективно (N+1), для продакшена нужен Dapper или join query.
        // Для учебного проекта пойдет. 
        var dtos = new List<MarkerDto>();

        foreach (var m in markers)
        {
            int userVote = 0;
            if (currentUserId.HasValue)
            {
                var vote = await _repository.GetVoteAsync(m.Id, currentUserId.Value);
                if (vote != null) userVote = vote.IsUpvote ? 1 : -1;
            }

            dtos.Add(new MarkerDto(
                m.Id, m.Title, m.Location.Latitude, m.Location.Longitude, 
                m.Description, m.ImageUrl, m.Category, m.Status, 
                m.CreatedByUserId, m.Rating, userVote, m.CreatedAt, m.UpdatedAt
            ));
        }
        return dtos;
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
}