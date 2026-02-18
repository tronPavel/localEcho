using LocalEcho.Core.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LocalEcho.Infrastructure.Repositories;

public static class MarkerQueryableExtensions
{
    private static readonly GeometryFactory _geometryFactory = 
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static IQueryable<Marker> InBoundingBox(
        this IQueryable<Marker> source,
        double minLat, double maxLat, double minLng, double maxLng)
    {
        var coordinates = new[]
        {
            new Coordinate(minLng, minLat),
            new Coordinate(maxLng, minLat),
            new Coordinate(maxLng, maxLat),
            new Coordinate(minLng, maxLat),
            new Coordinate(minLng, minLat)
        };

        var ring = _geometryFactory.CreateLinearRing(coordinates);
        var bbox = _geometryFactory.CreatePolygon(ring);

        return source.Where(m => 
            EF.Property<Point>(m, "Location").Intersects(bbox));
    }
}