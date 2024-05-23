using SessionAPICommonModels;

namespace LiveTrackerSessionAPI.Services.Interfaces;

public interface IJWTService
{
    string GenerateToken(TokenInput tokenInput);
}