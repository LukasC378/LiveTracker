namespace SessionAPICommonModels.Dtos;

public class RaceDataDto
{
    public required int RaceId { get; set; }
    public int LapCount { get; set; }
    public required string GeoJsonData { get; set; }
    public required IList<DriverDto> Drivers { get; set; }
}