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
    private readonly IFileService _fileService;
    private readonly IIdentityRepository _identityRepository;
    

    public MarkerService(
        IMarkerRepository markerRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        GeometryFactory geometryFactory,
        IFileService fileService,
        IIdentityRepository identityRepository)
    {
        _markerRepository = markerRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _geometryFactory = geometryFactory;
        _fileService = fileService;
        _identityRepository = identityRepository;
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
            dto.Description
        );

        if (dto.ImageFiles != null && dto.ImageFiles.Count > 0)
        {
            foreach (var file in dto.ImageFiles)
            {
                using var stream = file.OpenReadStream();
                var url = await _fileService.SaveFileAsync(stream, file.FileName, "uploads");
            
                marker.Images.Add(MarkerImage.ForMarker(url, marker.Id));
            }
        }

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
                     ?? throw new KeyNotFoundException("Метка не найдена."); 
        
        MarkerResolutionDto? resolutionDto = null;
        if (detail.Marker.Resolution != null)
        {
            resolutionDto = new MarkerResolutionDto(
                detail.Marker.Resolution.Comment,
                detail.Marker.Resolution.ResolvedByUser?.Name ?? "Сотрудник службы", 
                detail.Marker.Resolution.CreatedAt,
                detail.Marker.Resolution.Images.Select(img => img.Url).ToList()
            );
        }

        return new MarkerDetailDto(
            detail.Marker.Id,
            detail.Marker.Title,
            detail.Marker.Description,
            detail.Marker.Images.Select(i => i.Url).ToList(),
            detail.Marker.Category,
            detail.Marker.Status,
            detail.Marker.CreatedByUserId,
            detail.Creator?.Name ?? "Аноним",
            detail.Creator?.AvatarUrl,
            detail.Marker.Rating,
            detail.UserVote,
            detail.Marker.CreatedAt,
            detail.Marker.UpdatedAt,
            resolutionDto
        );
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
    public async Task UpdateMarkerAsync(Guid id, UpdateMarkerDto dto, Guid userId)
    {
        var marker = await _markerRepository.GetByIdAsync(id) 
                     ?? throw new KeyNotFoundException("Метка не найдена.");

        var user = await _userRepository.GetByIdAsync(userId);
        var roles = await _identityRepository.GetRolesAsync(user!);

        bool isOwner = marker.CreatedByUserId == userId;
        bool isStaff = roles.Any(r => r == "Admin" || r == "Moderator");

        if (!isOwner && !isStaff)
            throw new UnauthorizedAccessException("У вас нет прав редактировать чужую метку.");

        marker.UpdateContent(dto.Title, dto.Description);

        if (dto.KeepImageUrls != null)
        {
            var imagesToRemove = marker.Images.Where(img => !dto.KeepImageUrls.Contains(img.Url)).ToList();
            foreach (var img in imagesToRemove)
            {
                await _fileService.DeleteFileAsync(img.Url);
                marker.Images.Remove(img);
            }
        }

        if (dto.NewImageFiles != null)
        {
            foreach (var file in dto.NewImageFiles)
            {
                using var stream = file.OpenReadStream();
                var url = await _fileService.SaveFileAsync(stream, file.FileName, "uploads");
                marker.Images.Add(MarkerImage.ForMarker(url, marker.Id));
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, Guid userId)
    {
        var marker = await _markerRepository.GetByIdAsync(id) 
                     ?? throw new KeyNotFoundException("Метка не найдена.");

        var user = await _userRepository.GetByIdAsync(userId);
        var roles = await _identityRepository.GetRolesAsync(user!);

        bool isOfficial = roles.Contains("Official");
        bool isAdmin = roles.Contains("Admin");
        bool isAuthor = marker.CreatedByUserId == userId;

        if (dto.NewStatus == MarkerStatus.Resolved)
        {
            if (!isOfficial && !isAdmin)
                throw new UnauthorizedAccessException("Закрывать задачи могут только представители официальных служб.");

            var resolution = new MarkerResolution(id, userId, dto.OfficialComment ?? "Решено.");

            if (dto.ProofImage != null)
            {
                using var stream = dto.ProofImage.OpenReadStream();
                var url = await _fileService.SaveFileAsync(stream, dto.ProofImage.FileName, "uploads");
                resolution.Images.Add(MarkerImage.ForResolution(url, resolution.Id));
            }

            marker.SetResolution(resolution);
        }
        else
        {
            if (!isAuthor && !isOfficial && !isAdmin)
                throw new UnauthorizedAccessException("Нет доступа к смене статуса этой метки.");

            marker.ChangeStatus(dto.NewStatus);
        }

        await _unitOfWork.SaveChangesAsync();
    }
    

    public async Task DeleteMarkerAsync(Guid id, Guid userId)
    {
        var marker = await _markerRepository.GetByIdAsync(id) 
                     ?? throw new KeyNotFoundException("Метка не найдена.");

        var user = await _userRepository.GetByIdAsync(userId);
        var roles = await _identityRepository.GetRolesAsync(user!);

        bool isStaff = roles.Any(r => r == "Admin" || r == "Moderator");
        if (marker.CreatedByUserId != userId && !isStaff)
            throw new UnauthorizedAccessException("Нет прав для удаления.");

        var fileUrls = marker.Images.Select(i => i.Url).ToList();
        if (marker.Resolution != null)
        {
            fileUrls.AddRange(marker.Resolution.Images.Select(i => i.Url));
        }

        await _markerRepository.DeleteAsync(marker);
        await _unitOfWork.SaveChangesAsync();

        foreach (var url in fileUrls)
        {
            await _fileService.DeleteFileAsync(url);
        }
    }
}