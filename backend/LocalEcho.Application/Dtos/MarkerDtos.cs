using LocalEcho.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace LocalEcho.Application.Dtos;

public record CoordinateDto(double Lat, double Lng);

public record CreateMarkerDto(
    string Title, 
    MarkerCategory Category, 
    string? Description,
    List<IFormFile>? ImageFiles,
    List<CoordinateDto> Points, 
    DateTime? StartDate, 
    DateTime? EndDate 
);

public record UpdateMarkerDto(
    string Title,
    string? Description,
    List<IFormFile>? NewImageFiles,
    List<string>? KeepImageUrls 
);

public record MarkerMapDto(
    Guid Id,
    string Title,
    MarkerCategory Category,
    MarkerStatus Status,
    string GeometryType, 
    List<CoordinateDto> Coordinates, 
    CoordinateDto Centroid,
    bool IsOfficial 
);

public record GetMarkersQueryParams(
    string? Category = null,
    string? Status = null,
    double? MinLat = null,
    double? MaxLat = null,
    double? MinLng = null,
    double? MaxLng = null,
    int? Limit = null
);
public record MarkerDetailDto(
    Guid Id,
    string Title,
    string? Description,
    List<string> ImageUrls, 
    MarkerCategory Category,
    MarkerStatus Status,
    Guid CreatorId,
    string CreatorName,
    string? CreatorAvatarUrl,
    int Rating,
    int UserVote,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ScheduledAt, 
    DateTime? ExpiresAt,
    bool IsOfficial, 
    List<MarkerResolutionDto> Resolutions 
);

public record MarkerResolutionDto(
    string Comment,
    string AuthorName,
    DateTime CreatedAt,
    List<string> ImageUrls
);

public record ChangeStatusDto(
    MarkerStatus NewStatus,
    string? Comment,         
    List<IFormFile>? ImageFiles
);

public record MarkerWorkItemDto(
    Guid Id,
    string Title,
    MarkerCategory Category,
    MarkerStatus Status,
    string CreatorName,
    Guid? DistrictId,    
    string DistrictName,  
    DateTime CreatedAt,   
    int Rating            
);

public record WorkItemsQueryParams(
    Guid? CityId, 
    Guid? DistrictId,
    MarkerStatus? Status,
    MarkerCategory? Category, 
    int Limit = 50
);

public record VoteDto(bool IsUpvote);