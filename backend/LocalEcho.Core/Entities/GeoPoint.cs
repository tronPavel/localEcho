namespace LocalEcho.Core.Entities;

public sealed class GeoPoint
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public GeoPoint(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90");
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180");

        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";
}