using System.Net.Http.Json;
using LocalEcho.Application.Interfaces;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly GeometryFactory _geometryFactory;

    public GeocodingService(HttpClient httpClient, GeometryFactory geometryFactory)
    {
        _httpClient = httpClient;
        _geometryFactory = geometryFactory;
        
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LocalEcho-App");
        }
    }

    public async Task<Point?> GetCoordinatesAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
            
            var results = await _httpClient.GetFromJsonAsync<List<NominatimResponse>>(url);

            if (results == null || results.Count == 0) return null;

            var first = results[0];
            
            if (double.TryParse(first.lat, System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(first.lon, System.Globalization.CultureInfo.InvariantCulture, out var lon))
            {
                return _geometryFactory.CreatePoint(new Coordinate(lon, lat));
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private record NominatimResponse(string lat, string lon);
}