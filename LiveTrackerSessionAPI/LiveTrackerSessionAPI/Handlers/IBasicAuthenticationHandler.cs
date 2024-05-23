namespace LiveTrackerSessionAPI.Handlers;

public interface IBasicAuthenticationHandler
{
    bool CheckUser(string username, string password);
}