using idunno.Authentication.Basic;
using LiveTrackerSessionAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SessionAPICommonModels;

namespace LiveTrackerSessionAPI.Controllers;

[Route("token")]
public class TokenController : ControllerBase
{
    private readonly IJWTService _jwtService;

    public TokenController(IJWTService jwtService)
    {
        _jwtService = jwtService;
    }

    [Authorize(Policy = BasicAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("generate")]
    public string GenerateToken([FromBody] TokenInput tokenInput)
    {
        return _jwtService.GenerateToken(tokenInput);
    }
}