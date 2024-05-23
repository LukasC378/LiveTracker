using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BL.Models.Logic;
using BL.Services.Interfaces;
using DB.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BL.Services;

public class JWTService : IJWTService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    private const int ExpirationMinutes = 60;

    public JWTService(IConfiguration configuration)
    {
        _secretKey = configuration.GetSection("JWTSettings")["SecretKey"]!;
        _issuer = configuration.GetSection("JWTSettings")["Issuer"]!;
        _audience = configuration.GetSection("JWTSettings")["Audience"]!;
    }

    public int GetTokenExpirationMinutes() => ExpirationMinutes;

    public string GenerateToken(int userId, string userEmail, UserRoleEnum userRole)
    {
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var userRoleString = userRole switch
        {
            UserRoleEnum.NormalUser => UserRole.NormalUser,
            UserRoleEnum.Organizer => UserRole.Organizer,
            _ => throw new NotImplementedException($"Unknown user role {userRole}")
        };

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            //new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Role, userRoleString)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}