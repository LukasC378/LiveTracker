using BL.Models.ViewModels;
using LiveTrackerCommonModels.ViewModels;

namespace BL.Services.Interfaces;

public interface IArchiveService
{
    Task CrateArchiveTable(int sessionId);
    Task<IEnumerable<RaceDataVM>> GetChunkFromArchive(int sessionId, int start, int length);
    Task<ArchiveInfoVM> GetInfo(int sessionId);
}