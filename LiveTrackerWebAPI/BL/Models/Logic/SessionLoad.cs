using BL.Models.ViewModels;
using LiveTrackerCommonModels.Dtos;

namespace BL.Models.Logic;

public abstract class SessionLoad
{
    public required SessionEmailVM EmailInfo { get; set; }
    public required IEnumerable<string> Emails { get; set; }
}

public class SessionToLoad : SessionLoad
{
    public required RaceDataDto RaceData { get; set; }
}

public class SessionToUnload : SessionLoad
{
}