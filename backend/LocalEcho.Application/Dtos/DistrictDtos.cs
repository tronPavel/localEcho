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
    Dictionary<string, int> CategoryBreakdown // Категория -> Количество
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
    List<CoordinateDto> Geometry
);