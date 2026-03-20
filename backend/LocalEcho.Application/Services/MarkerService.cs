using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
using LocalEcho.Core.Models;
using NetTopologySuite.Geometries;

namespace LocalEcho.Application.Services;

public class MarkerService : IMarkerService
{
    private readonly IMarkerRepository _markerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GeometryFactory _geometryFactory;

    public MarkerService(
        IMarkerRepository markerRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        GeometryFactory geometryFactory)
    {
        _markerRepository = markerRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _geometryFactory = geometryFactory;
    }

    public async Task<Guid> CreateMarkerAsync(CreateMarkerDto dto, Guid userId, Guid districtId)
    {
        var point = _geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));
        
        var marker = Marker.Create(
            dto.Title, 
            point, 
            dto.Category, 
            userId,      
            districtId,   
            dto.Description,
            dto.ImageUrl
        );

        await _markerRepository.AddAsync(marker);
        await _unitOfWork.SaveChangesAsync(); 

        return marker.Id;
    }

    public async Task<IEnumerable<MarkerMapDto>> GetMapMarkersAsync(GetMarkersQueryParams queryParams)
    {
        MarkerCategory? category = null;
        if (!string.IsNullOrEmpty(queryParams.Category) && Enum.TryParse<MarkerCategory>(queryParams.Category, true, out var c)) category = c;

        MarkerStatus? status = null;
        if (!string.IsNullOrEmpty(queryParams.Status) && Enum.TryParse<MarkerStatus>(queryParams.Status, true, out var s)) status = s;

        var filter = new MarkerFilter { 
            Category = category, Status = status, MinLat = queryParams.MinLat, MaxLat = queryParams.MaxLat, MinLng = queryParams.MinLng, MaxLng = queryParams.MaxLng, Limit = queryParams.Limit 
        };

        var previews = await _markerRepository.GetPreviewsAsync(filter);

        return previews.Select(p => new MarkerMapDto(
            p.Id, p.Latitude, p.Longitude, p.Category, p.Status, p.Title
        ));
    }

    public async Task<MarkerDetailDto> GetMarkerDetailsAsync(Guid id, Guid? currentUserId)
    {
        var detail = await _markerRepository.GetDetailAsync(id, currentUserId) 
                     ?? throw new KeyNotFoundException("Метка не найдена или была удалена."); 
                     
        return new MarkerDetailDto(detail.Marker.Id, detail.Marker.Title, detail.Marker.Description, detail.Marker.ImageUrl, detail.Marker.Category, detail.Marker.Status, detail.Marker.CreatedByUserId, detail.Creator?.Name, detail.Creator?.AvatarUrl, detail.Marker.Rating, detail.UserVote, detail.Marker.CreatedAt, detail.Marker.UpdatedAt);
    }

    public async Task UpdateDescriptionAsync(Guid id, UpdateDescriptionDto dto)
    {
        // В будущем сюда тоже можно добавить параметр userId, чтобы проверять:
        // if (marker.CreatedByUserId != userId) throw new ForbiddenException("Чужая метка");
        var marker = await _markerRepository.GetByIdAsync(id) 
                     ?? throw new KeyNotFoundException("Метка не найдена.");
                     
        marker.UpdateDescription(dto.Description);
        _markerRepository.Update(marker);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(Guid id, MarkerStatus newStatus)
    {
        var marker = await _markerRepository.GetByIdAsync(id) 
                     ?? throw new KeyNotFoundException("Метка не найдена.");
                     
        marker.ChangeStatus(newStatus);
        _markerRepository.Update(marker);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task VoteAsync(Guid markerId, VoteDto dto, Guid voterId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var marker = await _markerRepository.GetByIdAsync(markerId) 
                         ?? throw new KeyNotFoundException("Метка для голосования не найдена.");

            var existingVote = await _markerRepository.GetVoteAsync(markerId, voterId);
            
            int delta = 0;

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == dto.IsUpvote)
                {
                    delta = existingVote.IsUpvote ? -1 : 1;
                    _markerRepository.RemoveVote(existingVote);
                }
                else
                {
                    delta = dto.IsUpvote ? 2 : -2;
                    existingVote.ChangeType(dto.IsUpvote);
                }
            }
            else
            {
                delta = dto.IsUpvote ? 1 : -1;
                await _markerRepository.AddVoteAsync(new Vote(markerId, voterId, dto.IsUpvote));
            }

            if (delta != 0)
            {
                marker.UpdateRating(marker.Rating + delta);
                await _userRepository.UpdatePointsAsync(marker.CreatedByUserId, delta);
                await _unitOfWork.SaveChangesAsync(); 
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}