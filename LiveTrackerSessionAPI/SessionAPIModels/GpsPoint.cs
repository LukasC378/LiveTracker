using GeoJSON.Net.Geometry;
using Point = NetTopologySuite.Geometries.Point;

namespace SessionAPIModels;

public class GpsPoint : Point, IPosition
{
    public double Latitude { get; }
    public double Longitude { get; }
    public double? Altitude { get; set; }

    public GpsPoint(double latitude, double longitude) : base(longitude, latitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}