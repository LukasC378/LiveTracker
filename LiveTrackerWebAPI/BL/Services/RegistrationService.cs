using BL.Exceptions;
using BL.Models.Dto;
using BL.Services.Interfaces;
using DB.Database;
using DB.Entities;
using DB.Enums;
using Microsoft.EntityFrameworkCore;

namespace BL.Services;

/// <summary>
/// Service for registration
/// </summary>
public class RegistrationService : BaseService, IRegistrationService
{
    #region Declaration

    private readonly IEmailService _emailService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="emailService"></param>
    public RegistrationService(RaceTrackerDbContext dbContext, IEmailService emailService) : base(dbContext)
    {
        _emailService = emailService;
    }
    #endregion

    #region Public_Methods

    /// <summary>
    /// Register new user or null if username exists
    /// </summary>
    /// <param name="userToRegister"></param>
    /// <exception cref="ItemNotFoundException"></exception>
    /// <returns></returns>
    public async Task<RegistrationSecondResultEnum> RegisterUserAsync(UserToRegisterDto userToRegister)
    {
        if (DbContext.User.Any(x => x.Username == userToRegister.Username))
            return RegistrationSecondResultEnum.UserNameExists;

        var registration = await DbContext.Registration.FirstOrDefaultAsync(r => r.Link == userToRegister.Link);
        if (registration == null)
        {
            throw new ItemNotFoundException("Registration not found");
        }

        var userDb = await DbContext.User.FirstOrDefaultAsync(x => x.Email == registration.Email);
        if (userDb is not null && userDb.Role == userToRegister.Role)
        {
            return userDb.Role == UserRoleEnum.NormalUser
                ? RegistrationSecondResultEnum.UserAccountExists
                : RegistrationSecondResultEnum.OrganizerAccountExists;
        }

        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var user = new User
        {
            Username = userToRegister.Username,
            Email = registration.Email,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(userToRegister.Password + salt),
            PasswordSalt = salt,
            Role = userToRegister.Role
        };

        DbContext.Registration.Remove(registration);
        await DbContext.AddAsync(user);
        await DbContext.SaveChangesAsync();

        return RegistrationSecondResultEnum.Ok;
    }

    /// <summary>
    /// Creates new registration link
    /// </summary>
    /// <param name="userEmail">Email of user</param>
    /// <returns>New registration link</returns>
    public async Task<RegistrationFirstResultEnum> SendRegistrationLinkAsync(string userEmail)
    {
        //user has already normal and organizer account
        if (await DbContext.User.CountAsync(user => user.Email.Equals(userEmail)) >= 2)
            return RegistrationFirstResultEnum.EmailExists;

        var registration = await DbContext.Registration.FirstOrDefaultAsync(r => r.Email.Equals(userEmail));
        if (registration != null)
        {
            if (registration.CreationTime >= DateTime.UtcNow.AddHours(-24))
                return RegistrationFirstResultEnum.RegistrationLinkExists;

            DbContext.Registration.Remove(registration);
        }

        var newLink = GenerateRegistrationLink();

        var newRegistration = new Registration
        {
            Email = userEmail,
            Link = newLink,
            CreationTime = DateTime.UtcNow
        };

        await _emailService.SendRegistrationLink(userEmail, newLink);

        await DbContext.AddAsync(newRegistration);
        await DbContext.SaveChangesAsync();

        return RegistrationFirstResultEnum.Ok;
    }

    /// <summary>
    /// Updates data for registration after expiration
    /// </summary>
    /// <param name="registrationLink">Old registration link</param>
    /// <returns>Data for resending verification email</returns>
    public async Task<string> ResendRegistrationLinkAsync(string registrationLink)
    {
        var registration = await DbContext.Registration.FirstOrDefaultAsync(x => x.Link == registrationLink);
        if (registration == null)
            throw new ItemNotFoundException("Registration not found");

        var newLink = GenerateRegistrationLink();

        registration.CreationTime = DateTime.UtcNow;
        registration.Link = newLink;

        await _emailService.SendRegistrationLink(registration.Email, newLink);

        await DbContext.SaveChangesAsync();

        return newLink;
    }

    /// <summary>
    /// Verify if link is valid registration link
    /// </summary>
    /// <param name="registrationLink"></param>
    /// <returns>Status String</returns>
    public async Task<RegistrationFirstResultEnum> VerifyRegistrationLinkAsync(string registrationLink)
    {
        var dbResult = await DbContext.Registration.FirstOrDefaultAsync(registration => registration.Link == registrationLink);
        if (dbResult == null)
            throw new ItemNotFoundException("Registration link does not exist");

        return dbResult.CreationTime >= DateTime.UtcNow.AddHours(-24) ? RegistrationFirstResultEnum.Ok : RegistrationFirstResultEnum.RegistrationLinkExpired;
    }

    #endregion

    #region Private_Methods

    private static string GenerateRegistrationLink()
    {
        return Guid.NewGuid().ToString();
    }
    #endregion
}