using DB.Database;

namespace BL.Services;

public class BaseService
{
    #region Declaration
    protected RaceTrackerDbContext DbContext { get; }
    public BaseService(RaceTrackerDbContext dbContext)
    {
        DbContext = dbContext;
    }
    #endregion
}