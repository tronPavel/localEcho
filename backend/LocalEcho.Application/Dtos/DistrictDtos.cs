namespace LocalEcho.Application.Dtos;

public record DistrictBriefDto(Guid Id, string Name);

public record DistrictMapDto(
    Guid Id, 
    string Name, 
    List<CoordinateDto> Geometry, 
    CoordinateDto Centroid     
);

public record DistrictStatsDto(
    int TotalMarkers,           
    int ResidentsCount,         
    int ResolvedIssues,         
    double SuccessRate,         
    int OngoingEvents,          
    int NewSuggestionsCount,    
    Dictionary<string, int> CategoryBreakdown
);

public record DistrictDetailDto(
    Guid Id,
    string Name,
    string? Description,
    DistrictStatsDto Stats
);

public record CreateDistrictDto(
    string Name,
    string? Description,
    Guid CityId,
    List<CoordinateDto> Geometry 
);