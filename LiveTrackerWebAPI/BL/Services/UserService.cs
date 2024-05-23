using System.Security.Claims;
using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using BL.Utils;
using DB.Database;
using DB.Entities;
using DB.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BL.Services;

/// <summary>
/// User service
/// </summary>
public class UserService: BaseService, IUserService
{
    #region Declaration
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJWTService _jwtService;

    private const int RefreshTokenExpirationDays = 1;

    /// <summary>
    /// User service constructor
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="jwtService"></param>
    public UserService(RaceTrackerDbContext dbContext, IHttpContextAccessor httpContextAccessor, IJWTService jwtService) : base(dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
    }
    #endregion

    #region Public_Methods

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<UserDto> Login(string username, string password)
    {
        var user = await DbContext.User.SingleOrDefaultAsync(x => x.Username == username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password + user.PasswordSalt, user.HashedPassword))
            throw new UnauthorizedAccessException();

        var accessToken = CreateAccessToken(user.Id, user.Email, user.Role);
        var refreshToken = await CreateRefreshToken(user.Id);

        SetCookies(accessToken, refreshToken, user.Username);

        return MapUserToDto(user);
    }

    /// <summary>
    /// Refresh user access token
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task Refresh()
    {
        if (_httpContextAccessor is not { HttpContext: { } })
            throw new Exception("No http context");

        if (!(_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("X-Username", out var userName) &&
              _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken)))
            throw new UnauthorizedAccessException();


        var user = await DbContext.User
            .Join(DbContext.UserRefreshToken,
                u => u.Id,
                urt => urt.UserId,
                (u, urt) => new { User = u, UserRefreshToken = urt })
            .Where(joined => joined.UserRefreshToken.RefreshToken == refreshToken && joined.User.Username == userName)
            .Select(joined => joined.User)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new UnauthorizedAccessException();

        var accessToken  = CreateAccessToken(user.Id, user.Email, user.Role);
        SetCookies(accessToken);
    }

    /// <summary>
    /// For user authorization
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<User> GetCurrentUser()
    {
        var user = await DbContext.User.SingleOrDefaultAsync(x => x.Id == GetCurrentUserId());
        if (user == null)
            throw new UnauthorizedAccessException();

        return user;
    }

    /// <summary>
    /// For user authorization
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<UserDto> GetCurrentUserDtoWithAuthorization()
    {
        var user = await DbContext.User.SingleOrDefaultAsync(x => x.Id == GetCurrentUserId());
        if(user == null)
            throw new UnauthorizedAccessException();

        return MapUserToDto(user);
    }

    /// <summary>
    /// Returns current logged user or null
    /// </summary>
    /// <returns></returns>
    public async Task<UserDto?> GetCurrentUserDto()
    {
        if (!int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return null;

        var user =  await DbContext.User.SingleOrDefaultAsync(x => x.Id == userId);
        return user == null ? null : MapUserToDto(user);
    }

    /// <summary>
    /// Returns current user id
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public int GetCurrentUserId()
    {
        if (!int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }

    /// <summary>
    /// Returns organizers
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <param name="offset"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public async Task<IList<UserVM>> GetOrganizersForSearch(string searchTerm, int offset, int limit)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return await GetUsers(query => query.Where(x => x.Role == UserRoleEnum.Organizer), offset, limit);

        searchTerm = searchTerm.ToLower().RemoveDiacritics();
        return await GetUsers(query => 
            query.Where(x => x.Role == UserRoleEnum.Organizer && DbContext.Unaccent(x.Username).ToLower().Contains(searchTerm.ToLower())), offset, limit);
    }

    /// <summary>
    /// Returns organizers for user view
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<IList<OrganizerVM>> GetOrganizers(OrganizersFilter filter)
    {
        var userId = GetCurrentUserId();
        var subscribed = DbContext.Subscriber.Where(s => s.UserId == userId).Select(x => x.OrganizerId);

        var query = DbContext.User.Where(x => x.Role == UserRoleEnum.Organizer);

        query = filter.Type switch
        {
            OrganizerTypeEnum.Subscribed => query.Where(x => subscribed.Contains(x.Id)),
            OrganizerTypeEnum.Others => query.Where(x => !subscribed.Contains(x.Id)),
            _ => query
        };

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(x => DbContext.Unaccent(x.Username).ToLower().Contains(filter.SearchTerm.ToLower()));
        }

        var organizers = await query
            .Select(o => new OrganizerVM
            {
                Id = o.Id, 
                Name = o.Username, 
                Subscribed = subscribed.Contains(o.Id)
            })
            .OrderBy(o => o.Name)
            .Skip(filter.Offset)
            .Take(filter.Limit)
            .ToListAsync();

        return organizers;
    }

    /// <summary>
    /// Logout
    /// </summary>
    /// <returns></returns>
    public async Task Logout()
    {
        if (_httpContextAccessor is not { HttpContext: { } })
            throw new Exception("No http context");

        var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["X-Refresh-Token"];

        DeleteCookies();

        if (refreshToken is not null)
            await DeleteRefreshToken(refreshToken);
    }

    #endregion

    #region Private Methods

    private string CreateAccessToken(int userId, string email, UserRoleEnum role)
    {
        return _jwtService.GenerateToken(userId, email, role);
    }

    private async Task<string> CreateRefreshToken(int userId)
    {
        var refreshToken = Guid.NewGuid().ToString();
        var userRefreshToken = new UserRefreshToken
        {
            RefreshToken = refreshToken,
            UserId = userId,
            ValidTo = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)
        };

        await DbContext.UserRefreshToken.AddAsync(userRefreshToken);
        await DbContext.SaveChangesAsync();
        
        

        return refreshToken;
    }

    private void SetCookies(string accessToken, string? refreshToken = null, string? username = null)
    {
        if (_httpContextAccessor is not { HttpContext: { } })
            throw new Exception("No http context");

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes());
        _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Access-Token", accessToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = accessTokenExpiration });

        if (refreshToken is not null)
        {
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Refresh-Token", refreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = refreshTokenExpiration });
        }

        if (username is not null)
        {
            var usernameExpiration = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Username", username, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Expires = usernameExpiration });
        }
    }

    private async Task<IList<UserVM>> GetUsers(Func<IQueryable<User>, IQueryable<User>> filter, int offset, int limit)
    {
        return await filter(DbContext.User).OrderBy(x => x.Username).Skip(offset).Take(limit).Select(x => new UserVM
        {
            Id = x.Id,
            Name = x.Username
        }).ToListAsync();
    }

    private void DeleteCookies()
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("X-Access-Token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddYears(-1)
        });

        _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Refresh-Token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddYears(-1)
        });

        _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Username", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddYears(-1)
        });
    }

    private async Task DeleteRefreshToken(string refreshToken)
    {
        var userRefreshToken = await DbContext.UserRefreshToken.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
        if(userRefreshToken is null)
            return;

        DbContext.UserRefreshToken.Remove(userRefreshToken);
    }

    private static UserDto MapUserToDto(User user) => new()
    {
        Email = user.Email,
        Username = user.Username,
        Role = user.Role
    };

    #endregion
}