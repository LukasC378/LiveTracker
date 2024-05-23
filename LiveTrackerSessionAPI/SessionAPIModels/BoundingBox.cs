using GeoJSON.Net.Geometry;

namespace SessionAPIModels;

public class BoundingBox
{
    public required Position TopLeft { get; set; }
    public required Position TopRight { get; set; }
    public required Position BottomLeft { get; set; }
    public required Position BottomRight { get; set; }
}