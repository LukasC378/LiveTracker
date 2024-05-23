namespace LiveTrackerSessionAPI.Handlers;

public class BasicAuthenticationHandler : IBasicAuthenticationHandler
{
    private readonly string _username;
    private readonly string _password;

    public BasicAuthenticationHandler(IConfiguration configuration)
    {
        var authSection = configuration.GetSection("Authentication");
        _username = authSection["Username"]!;
        _password = authSection["Password"]!;
    }

    public bool CheckUser(string username, string password)
    {
        return username == _username && password == _password;
    }
}