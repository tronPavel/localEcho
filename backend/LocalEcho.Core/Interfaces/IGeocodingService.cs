using NetTopologySuite.Geometries;

namespace LocalEcho.Application.Interfaces;

public interface IGeocodingService
{
    Task<Point?> GetCoordinatesAsync(string address);
}