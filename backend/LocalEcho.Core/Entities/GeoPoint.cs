namespace LocalEcho.Core.Entities;

public class GeoPoint
{ 
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public GeoPoint(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90) throw new ArgumentException("Invalid latitude");
        if (longitude < -180 || longitude > 180) throw new ArgumentException("Invalid longitude");
        Latitude = latitude;
        Longitude = longitude;
    }
}