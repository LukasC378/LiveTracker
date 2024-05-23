using BL;
using BL.Models.Dto;
using BL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for registration
/// </summary>
[Route("register")]
public class RegistrationController : Controller
{
    #region Declaration
    private readonly IRegistrationService _registrationService;

    /// <summary>
    /// Constructor for registration controller
    /// </summary>
    /// <param name="registrationService">Service for registration</param>
    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }
    #endregion

    #region GET Methods
    /// <summary>
    /// Returns true if link is valid registration link
    /// </summary>
    /// <param name="registrationLink"></param>
    /// <returns></returns>
    [HttpGet("{registrationLink}")]
    public async Task<RegistrationFirstResultEnum> VerifyRegistrationLink(string registrationLink)
    {        
        return await _registrationService.VerifyRegistrationLinkAsync(registrationLink);
    }

    #endregion

    #region POST Methods

    /// <summary>
    /// Register user
    /// </summary>
    /// <param name="userToRegister"></param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<RegistrationSecondResultEnum> RegisterUser([FromBody] UserToRegisterDto userToRegister)
    {
        return await _registrationService.RegisterUserAsync(userToRegister);
    }


    /// <summary>
    /// Returns unique registration link 
    /// </summary>
    /// <param name="userEmail">Email of user</param>
    /// <returns></returns>
    [HttpPost("{userEmail}")]
    public async Task<RegistrationFirstResultEnum> SendRegistrationLink(string userEmail)
    { 
        return await _registrationService.SendRegistrationLinkAsync(userEmail);
    }
    #endregion

    #region PUT Methods
    /// <summary>
    /// Endpoint for update data for registration after expiration
    /// </summary>
    /// <param name="registrationLink">Old registration link</param>
    /// <returns>Data for resending verification email</returns>
    [HttpPut("{registrationLink}")]
    public async Task<string> UpdateRegistrationData(string registrationLink)
    {
        return await _registrationService.ResendRegistrationLinkAsync(registrationLink);
    }
    #endregion
}