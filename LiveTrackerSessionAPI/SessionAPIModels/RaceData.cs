using System.Collections.Concurrent;
using GeoJSON.Net.Geometry;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace SessionAPIModels;

public class RaceData
{
    public int RaceId { get; set; }
    public IList<(LineString Line, Polygon BoundingBox)> Sectors { get; set; } = new List<(LineString Line, Polygon BoundingBox)>();
    public ConcurrentDictionary<string, Driver> Drivers { get; set; } = new();
    public (LineString Line, Polygon BoundingBox)? SectorAfterFinish { get; set; }
    public int LapCount { get; set; }
    public bool IsCircuit => LapCount is not 0 && SectorAfterFinish is null;
    public int CurrentMaxSector { get; set; }
}