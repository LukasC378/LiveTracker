namespace LiveTrackerSessionAPI.Repository;

public interface IRepository
{
    Task SaveData(int sessionId, string data);
}