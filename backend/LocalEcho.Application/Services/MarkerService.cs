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
        if (dto.Points == null || dto.Points.Count == 0)
            throw new ArgumentException("Координаты отсутствуют");

        // Получаем актуальные роли из базы для защиты
        var user = await _userRepository.GetByIdAsync(userId);
        var roles = await _identityRepository.GetRolesAsync(user!);
        bool isStaff = roles.Any(r => r is "Admin" or "Official");

        Geometry location;

        // 1. Сборка геометрии в зависимости от кол-ва точек
        if (dto.Points.Count == 1) // Обычная точка
        {
            var p = dto.Points[0];
            location = _geometryFactory.CreatePoint(new Coordinate(p.Lng, p.Lat));
        }
        else // Полигон (Зона)
        {
            if (!isStaff) throw new UnauthorizedAccessException("Только верифицированные лица могут создавать зоны.");

            var coords = dto.Points.Select(p => new Coordinate(p.Lng, p.Lat)).ToList();
        
            // Замыкаем полигон для PostGIS (первая точка == последняя)
            if (!coords.First().Equals2D(coords.Last()))
                coords.Add(new Coordinate(coords.First().X, coords.First().Y));

            location = _geometryFactory.CreatePolygon(coords.ToArray());
        }

        // 2. Создание сущности
        var marker = Marker.Create(dto.Title, location, dto.Category, userId, districtId, dto.Description, dto.ScheduledAt);
        // 3. Атомарная работа с файлами
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

        var markers = await _markerRepository.GetForMapAsync(filter); // фильтр по BoundingBox

        return markers.Select(m => new MarkerMapDto(
            m.Id,
            m.Title,
            m.Category,
            m.Status,
            m.Location.GeometryType, // "Point", "Polygon" или "LineString"
            m.Location.Coordinates.Select(c => new CoordinateDto(c.Y, c.X)).ToList(),
            new CoordinateDto(m.Location.Centroid.Y, m.Location.Centroid.X) // Всегда возвращаем точку-центр
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

    bool isStaff = roles.Any(r => r is "Admin" or "Official");
    bool isAuthor = marker.CreatedByUserId == userId;

    // ВАЛИДАЦИЯ ПРАВ (БИЗНЕС-ПРАВИЛА)
    // 1. Статус "Resolved/Accepted/Rejected" - только стафф (УК/Админ)
    var finalStatuses = new[] { MarkerStatus.Resolved, MarkerStatus.Accepted, MarkerStatus.Rejected };
    if (finalStatuses.Contains(dto.NewStatus) && !isStaff)
        throw new UnauthorizedAccessException("Только официальные лица могут подтверждать решение задач.");

    // 2. Обычный пользователь может менять статус только у СВОИХ объявлений или событий (например, отменить)
    if (!isAuthor && !isStaff)
        throw new UnauthorizedAccessException("Нет прав доступа.");

    // 3. ПЕРЕХОД К СОЗДАНИЮ RESOLUTION
    if (dto.NewStatus == MarkerStatus.Resolved || dto.NewStatus == MarkerStatus.Accepted)
    {
        // Создаем резолюцию
        var resolution = new MarkerResolution(id, userId, dto.Comment ?? "Выполнено официальной службой.");

        // Сохраняем фото "После" (те самые ImageFiles из ChangeStatusDto)
        if (dto.ImageFiles != null && dto.ImageFiles.Any())
        {
            foreach (var file in dto.ImageFiles)
            {
                using var stream = file.OpenReadStream();
                var url = await _fileService.SaveFileAsync(stream, file.FileName, "uploads");
                resolution.Images.Add(MarkerImage.ForResolution(url, resolution.Id));
            }
        }

        // Применяем решение к маркеру
        marker.SetResolution(resolution);
    }
    else
    {
        // Просто смена статуса (InProgress, Archived и т.д.)
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