using LocalEcho.Application.Dtos;
using LocalEcho.Application.Interfaces;
using LocalEcho.Core.Entities;
using LocalEcho.Core.Interfaces;
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
    private readonly IDistrictRepository _districtRepository;
    private readonly IReportRepository _reportRepository;
    

    public MarkerService(
        IMarkerRepository markerRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        GeometryFactory geometryFactory,
        IFileService fileService,
        IIdentityRepository identityRepository,
        IDistrictRepository districtRepository, 
        IReportRepository reportRepository
    )
    {
        _markerRepository = markerRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _geometryFactory = geometryFactory;
        _fileService = fileService;
        _identityRepository = identityRepository;
        _districtRepository = districtRepository;
        _reportRepository = reportRepository;
    }

public async Task<Guid> CreateMarkerAsync(CreateMarkerDto dto, Guid userId)
{
    if (dto.Points == null || dto.Points.Count == 0)
        throw new ArgumentException("Координаты отсутствуют");

    var user = await _userRepository.GetByIdAsync(userId);
    var roles = await _identityRepository.GetRolesAsync(user!);
    bool isStaff = roles.Any(r => r is "Admin" or "Official");

    
    Geometry location;
    if (dto.Points.Count == 1) 
    {
        location = _geometryFactory.CreatePoint(new Coordinate(dto.Points[0].Lng, dto.Points[0].Lat));
    }
    else 
    {
        if (!isStaff) throw new UnauthorizedAccessException("Только верифицированные лица могут создавать зоны.");
        var coords = dto.Points.Select(p => new Coordinate(p.Lng, p.Lat)).ToList();
        if (!coords.First().Equals2D(coords.Last())) coords.Add(new Coordinate(coords.First().X, coords.First().Y));
        location = _geometryFactory.CreatePolygon(coords.ToArray());
    }
    location.SRID = 4326;

    Guid? autoDistrictId = null;
    var searchPoint = location is Point p ? p : location.Centroid;
    
    var district = await _districtRepository.GetDistrictByCoordinatesAsync(searchPoint);
    if (district != null) autoDistrictId = district.Id;

    var marker = Marker.Create(dto.Title, location, dto.Category, userId, autoDistrictId, dto.Description, dto.ScheduledAt);

    if (dto.ImageFiles != null && dto.ImageFiles.Any())
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

        var markers = await _markerRepository.GetForMapAsync(filter); 

        return markers.Select(m => new MarkerMapDto(
            m.Id,
            m.Title,
            m.Category,
            m.Status,
            m.Location.GeometryType, 
            m.Location.Coordinates.Select(c => new CoordinateDto(c.Y, c.X)).ToList(),
            new CoordinateDto(m.Location.Centroid.Y, m.Location.Centroid.X) 
        ));
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

public async Task<MarkerDetailDto> GetMarkerDetailsAsync(Guid id, Guid? currentUserId)
{
    var detail = await _markerRepository.GetDetailAsync(id, currentUserId) 
                 ?? throw new KeyNotFoundException("Метка не найдена."); 

    var resolutionDtos = detail.Marker.Resolutions
        .OrderByDescending(r => r.CreatedAt) 
        .Select(r => new MarkerResolutionDto(
            r.Comment,
            r.ResolvedByUser?.Name ?? "Сотрудник службы", 
            r.CreatedAt,
            r.Images.Select(img => img.Url).ToList()
        )).ToList();

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
        detail.Marker.ScheduledAt,
        detail.Marker.ExpiresAt,
        resolutionDtos
    );
}

public async Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, Guid userId)
{
    var marker = await _markerRepository.GetByIdAsync(id) 
                 ?? throw new KeyNotFoundException("Метка не найдена.");

    var user = await _userRepository.GetByIdAsync(userId);
    var roles = await _identityRepository.GetRolesAsync(user!);

    bool isStaff = roles.Any(r => r is "Admin" or "Official");
    bool isAuthor = marker.CreatedByUserId == userId;

    ValidateStatusTransition(marker.Category, marker.Status, dto.NewStatus, isStaff, isAuthor);

    if (isStaff && (!string.IsNullOrWhiteSpace(dto.Comment) || (dto.ImageFiles?.Count > 0)))
    {
        var resolution = new MarkerResolution(id, userId, dto.Comment ?? "Статус изменен.");

        if (dto.ImageFiles != null)
        {
            foreach (var file in dto.ImageFiles)
            {
                using var stream = file.OpenReadStream();
                var url = await _fileService.SaveFileAsync(stream, file.FileName, "uploads");
                
                resolution.Images.Add(new MarkerImage(url)); 
            }
        }
        
        marker.AddResolution(resolution); 
    }

    marker.ChangeStatus(dto.NewStatus);
    
    await _unitOfWork.SaveChangesAsync();
}

public async Task DeleteMarkerAsync(Guid id, Guid userId)
{
    var marker = await _markerRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException();
    
    var user = await _userRepository.GetByIdAsync(userId);
    var roles = await _identityRepository.GetRolesAsync(user!);
    
    if (marker.CreatedByUserId != userId && !roles.Any(r => r is "Admin" or "Moderator"))
        throw new UnauthorizedAccessException();

    var fileUrls = marker.Images.Select(i => i.Url).ToList();
    var resolutionFiles = marker.Resolutions.SelectMany(r => r.Images).Select(i => i.Url);
    fileUrls.AddRange(resolutionFiles);

    await _markerRepository.DeleteAsync(marker);
    await _unitOfWork.SaveChangesAsync();

    foreach (var url in fileUrls) await _fileService.DeleteFileAsync(url);
}
private void ValidateStatusTransition(MarkerCategory category, MarkerStatus current, MarkerStatus next, bool isStaff, bool isAuthor)
{
    if (!isStaff && !isAuthor) 
        throw new UnauthorizedAccessException("Forbidden");

    var restrictedForUsers = new[] { 
        MarkerStatus.Resolved, MarkerStatus.Accepted, MarkerStatus.Rejected, 
        MarkerStatus.Review, MarkerStatus.Ongoing, MarkerStatus.Passed 
    };

    if (restrictedForUsers.Contains(next) && !isStaff)
        throw new UnauthorizedAccessException("ForbiddenRole");

    switch (category)
    {
        case MarkerCategory.Issue:
            var issueValid = new[] { MarkerStatus.Active, MarkerStatus.InProgress, MarkerStatus.Resolved };
            if (!issueValid.Contains(next)) throw new InvalidOperationException("InvalidStatusForIssue");
            break;

        case MarkerCategory.Event:
            var eventValid = new[] { MarkerStatus.Upcoming, MarkerStatus.Ongoing, MarkerStatus.Passed, MarkerStatus.Archived };
            if (!eventValid.Contains(next)) throw new InvalidOperationException("InvalidStatusForEvent");
            break;

        case MarkerCategory.Announcement:
            var announceValid = new[] { MarkerStatus.Current, MarkerStatus.Archived };
            if (!announceValid.Contains(next)) throw new InvalidOperationException("InvalidStatusForAnnounce");
            break;

        case MarkerCategory.Suggestion:
            var suggestValid = new[] { MarkerStatus.Review, MarkerStatus.Accepted, MarkerStatus.Rejected };
            if (!suggestValid.Contains(next)) throw new InvalidOperationException("InvalidStatusForSuggestion");
            break;
            
        case MarkerCategory.Project:
            if (next == MarkerStatus.Review || next == MarkerStatus.Accepted) 
                throw new InvalidOperationException("InvalidStatusForProject");
            break;
    }
}

public async Task ReportMarkerAsync(Guid markerId, CreateReportDto dto, Guid reporterId)
{
    var marker = await _markerRepository.GetByIdAsync(markerId) 
                 ?? throw new KeyNotFoundException("Метка не найдена.");

    var report = new Report(markerId, reporterId, dto.Reason, dto.Comment);
    await _reportRepository.AddAsync(report);
    await _unitOfWork.SaveChangesAsync(); 

    var currentActiveReports = await _reportRepository.GetActiveCountForMarkerAsync(markerId);
    
    if (currentActiveReports >= 5 && !marker.IsHidden)
    {
        marker.Hide();
        await _unitOfWork.SaveChangesAsync();
    }
}

}
