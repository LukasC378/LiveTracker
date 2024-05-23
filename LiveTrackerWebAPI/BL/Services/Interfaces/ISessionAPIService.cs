using LiveTrackerCommonModels.Dtos;
using LiveTrackerModels;

namespace BL.Services.Interfaces;

public interface ISessionApiService
{
    Task<string> GetSessionToken(TokenInput tokenInput);
    Task LoadRace(RaceDataDto raceDataDto);
    Task<IEnumerable<Guid>?> UnloadRace(int raceId);
}