using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LiveTrackerSessionAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using SessionAPICommonModels;

namespace LiveTrackerSessionAPI.Services;

public class JWTService : IJWTService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JWTService(IConfiguration configuration)
    {
        _secretKey = configuration.GetSection("JWTSettings")["SecretKey"]!;
        _issuer = configuration.GetSection("JWTSettings")["Issuer"]!;
        _audience = configuration.GetSection("JWTSettings")["Audience"]!;
    }

    public string GenerateToken(TokenInput tokenInput)
    {
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, tokenInput.RaceId.ToString()),
            new Claim(ClaimTypes.Name, tokenInput.UserName),
            new Claim(ClaimTypes.UserData, tokenInput.UserId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: tokenInput.ScheduledFrom.AddHours(-1),
            expires: tokenInput.ScheduledTo.AddHours(4),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}