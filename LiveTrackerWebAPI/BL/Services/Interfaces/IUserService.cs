using BL.Models.Dto;
using BL.Models.Logic;
using BL.Models.ViewModels;
using DB.Entities;

namespace BL.Services.Interfaces
{
    /// <summary>
    /// Interface for user service
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        Task<UserDto> Login(string username, string password);

        /// <summary>
        /// Refresh user access token
        /// </summary>
        /// <returns></returns>
        Task Refresh();

        /// <summary>
        /// Returns current user
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        Task<User> GetCurrentUser();

        /// <summary>
        /// For user authorization
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        Task<UserDto> GetCurrentUserDtoWithAuthorization();

        /// <summary>
        /// Returns current logged user or null
        /// </summary>
        /// <returns></returns>
        Task<UserDto?> GetCurrentUserDto();

        /// <summary>
        /// Returns current user id
        /// </summary>
        /// <returns></returns>
        int GetCurrentUserId();

        /// <summary>
        /// Returns organizers
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IList<UserVM>> GetOrganizersForSearch(string searchTerm, int offset, int limit);

        /// <summary>
        /// Returns organizers for user view
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IList<OrganizerVM>> GetOrganizers(OrganizersFilter filter);

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        Task Logout();
    }
}
