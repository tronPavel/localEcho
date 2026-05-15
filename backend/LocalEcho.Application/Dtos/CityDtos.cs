namespace LocalEcho.Application.Dtos;

public record CityBriefDto(Guid Id, string Name, double Lat, double Lng);

public record CityMapDto(
    Guid Id, 
    string Name, 
    List<CoordinateDto> Geometry
);

public record CreateCityDto(
    string Name,
    List<CoordinateDto> Geometry
);