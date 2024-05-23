using DB.Enums;

namespace BL.Services.Interfaces;

/// <summary>
/// Service for generating JWT token
/// </summary>
public interface IJWTService
{
    int GetTokenExpirationMinutes();
    string GenerateToken(int userId, string userEmail, UserRoleEnum userRole);
}