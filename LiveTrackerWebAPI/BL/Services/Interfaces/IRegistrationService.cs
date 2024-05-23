using BL.Models.Dto;

namespace BL.Services.Interfaces
{
    /// <summary>
    /// Registration service interface
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Creates new registration link
        /// </summary>
        /// <param name="userEmail">Email of user</param>
        /// <returns>New registration link</returns>
        Task<RegistrationFirstResultEnum> SendRegistrationLinkAsync(string userEmail);

        /// <summary>
        /// Updates data for registration after expiration
        /// </summary>
        /// <param name="registrationLink">Old registration link</param>
        /// <returns>Data for resending verification email</returns>
        Task<string> ResendRegistrationLinkAsync(string registrationLink);

        /// <summary>
        /// Verify if link is valid registration link
        /// </summary>
        /// <param name="registrationLink"></param>
        /// <returns>Status String</returns>
        Task<RegistrationFirstResultEnum> VerifyRegistrationLinkAsync(string registrationLink);

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="userToRegister"></param>
        /// <returns></returns>
        Task<RegistrationSecondResultEnum> RegisterUserAsync(UserToRegisterDto userToRegister);
    }
}
