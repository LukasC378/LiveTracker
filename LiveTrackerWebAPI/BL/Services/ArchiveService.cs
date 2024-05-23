using BL.Models.ViewModels;
using BL.Services.Interfaces;
using DB.Database;
using LiveTrackerCommonModels.ViewModels;

namespace BL.Services;

public class ArchiveService : BaseService, IArchiveService
{
    #region Declaration

    public ArchiveService(RaceTrackerDbContext dbContext) : base(dbContext)
    {
    }

    #endregion

    #region Public Methods

    public async Task CrateArchiveTable(int sessionId)
    {
        await DbContext.CreateArchiveTable(sessionId);
    }

    public async Task<IEnumerable<RaceDataVM>> GetChunkFromArchive(int sessionId, int start, int length)
    {
        return await DbContext.GetChunkFromArchive(sessionId, start, length);
    }

    public async Task<ArchiveInfoVM> GetInfo(int sessionId)
    {
        var dbResult = await DbContext.GetArchivedSessionInfo(sessionId);
        return new ArchiveInfoVM
        {
            Count = dbResult.Count,
            Time = dbResult.Time
        };
    }

    #endregion
}