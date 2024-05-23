using SessionAPICommonModels;
using SessionAPICommonModels.Dtos;

namespace LiveTrackerSessionAPI.Services.Interfaces;

public interface ITrackService
{
    void LoadRaceData(RaceDataDto raceDataDto);
    Task UpdateGpsData(IList<GpsData> gpsData);

    //only for testing
    Tuple<List<GpsData>, int> GetRaceState(int raceId);
    IEnumerable<string>? UnloadRaceData(int raceId);
}