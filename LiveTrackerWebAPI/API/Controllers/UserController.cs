using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// User controller
/// </summary>
[Route("user")]
public class UserController : Controller
{
    #region Declaration

    private readonly IUserService _userService;

    /// <summary>
    /// User controller constructor
    /// </summary>
    /// <param name="userService"></param>
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    #endregion

    #region GET Methods
    /// <summary>
    /// Returns current user if logged, else return Unauthorized
    /// </summary>
    /// <response code="401">Unauthorized when user is not logged in</response>
    /// <response code="500">Unexpected error</response>
    /// <returns></returns>
    [HttpGet]
    public async Task<UserDto?> GetCurrentUser()
    {
        return await _userService.GetCurrentUserDto();
    }

    /// <summary>
    /// Refresh user access
    /// </summary>
    /// <returns></returns>
    [HttpGet("refresh")]
    public async Task Refresh()
    {
        await _userService.Refresh();
    }

    /// <summary>
    /// For user authorization
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("authorize")]
    public async Task<UserDto> Authorize()
    {
        return await _userService.GetCurrentUserDtoWithAuthorization();
    }

    [HttpGet("organizersForSearch")]
    public async Task<IList<UserVM>> GetOrganizersForSearch([FromQuery] string searchTerm = "", [FromQuery] int offset = 0, [FromQuery] int limit = 50)
    {
        return await _userService.GetOrganizersForSearch(searchTerm, offset, limit);
    }

    //[Authorize(Roles = UserRole.NormalUser)]
    [HttpGet("organizers")]
    public async Task<IList<OrganizerVM>> GetOrganizers(OrganizersFilter filter)
    {
        return await _userService.GetOrganizers(filter);
    }

    #endregion

    #region POST Methods
    /// <summary>
    /// Endpoint for login user
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <response code="401">Unauthorized when wrong password</response>
    /// <response code="500">Unexpected error</response>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<UserDto> Login([FromForm] string username, [FromForm] string password)
    {
        return await _userService.Login(username, password);
    }

    #endregion

    #region DELETE Methods
    /// <summary>
    /// Logs out user
    /// </summary>
    [Authorize]
    [HttpDelete("logout")]
    public async Task Logout()
    {
        await _userService.Logout();
    }

    #endregion
}