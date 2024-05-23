using GeoJSON.Net.Geometry;

namespace SessionAPIModels;

public class Driver
{
    public Driver(Guid id)
    {
        Id = id;
        Position = null;
        LapCount = 0;
        SectorId = 0;
    }

    public Guid Id { get; set; }
    public IPosition? Position { get; set; }
    public int LapCount { get; set; }
    public int SectorId { get; set; }
    public int FinalPosition { get; set; }
    public bool Finished => FinalPosition != 0;
}