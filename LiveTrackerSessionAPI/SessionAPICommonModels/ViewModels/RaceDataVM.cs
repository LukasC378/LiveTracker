namespace SessionAPICommonModels.ViewModels;

public class RaceDataVM
{
    public required IList<DriverVM> DriversData { get; set; }
    public int LapCount { get; set; }
}