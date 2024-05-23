using BL;
using BL.Models.Dto;
using BL.Services;
using BL.Services.Interfaces;
using DB.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.ServicesTests;

public class RegistrationServiceTests : BaseMock
{
    private readonly IRegistrationService _registrationService;
    private readonly Mock<IEmailService> _emailServiceMock = new();

    public RegistrationServiceTests()
    {
        _registrationService = new RegistrationService(DbContext, _emailServiceMock.Object);
    }

    [Fact]
    public async Task SendRegistrationLink_OK()
    {
        const string email = "user@test.com";
        var result = await _registrationService.SendRegistrationLinkAsync(email);

        Assert.Equal(RegistrationFirstResultEnum.Ok, result);
    }

    [Fact]
    public async Task SendRegistrationLink_EmailExists()
    {
        const string email = "user@test.com";

        await AddUser("username", "password", email, UserRoleEnum.NormalUser);
        await AddUser("username1", "password1", email, UserRoleEnum.Organizer);

        var result = await _registrationService.SendRegistrationLinkAsync(email);

        Assert.Equal(RegistrationFirstResultEnum.EmailExists, result);
    }

    [Fact]
    public async Task SendRegistrationLink_RegistrationLinkExists()
    {
        const string email = "user@test.com";
        await _registrationService.SendRegistrationLinkAsync(email);

        var result = await _registrationService.SendRegistrationLinkAsync(email);
        Assert.Equal(RegistrationFirstResultEnum.RegistrationLinkExists, result);
    }

    [Fact]
    public async Task RegisterUser_OK()
    {
        const string email = "user@test.com";
        await _registrationService.SendRegistrationLinkAsync(email);

        var registration = await DbContext.Registration.FirstOrDefaultAsync();
        Assert.NotNull(registration);

        const string username = "username";
        const string password = "password";
        var userToRegister = new UserToRegisterDto
        {
            Link = registration.Link,
            Username = username,
            Password = password,
            Role = UserRoleEnum.NormalUser
        };

        var result = await _registrationService.RegisterUserAsync(userToRegister);
        Assert.Equal(RegistrationSecondResultEnum.Ok, result);

        var user = await DbContext.User.FirstOrDefaultAsync();
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.Equal(username, user.Username);
        Assert.Equal(UserRoleEnum.NormalUser, user.Role);
    }

    [Fact]
    public async Task RegisterUser_UsernameExists()
    {
        const string username = "username";
        const string password = "password";

        await AddUser(username, "password2", "email@test.com", UserRoleEnum.NormalUser);

        const string email = "user@test.com";
        await _registrationService.SendRegistrationLinkAsync(email);
        var registration = await DbContext.Registration.FirstOrDefaultAsync();
        Assert.NotNull(registration);

        var userToRegister = new UserToRegisterDto
        {
            Link = registration.Link,
            Username = username,
            Password = password,
            Role = UserRoleEnum.NormalUser
        };

        var result = await _registrationService.RegisterUserAsync(userToRegister);
        Assert.Equal(RegistrationSecondResultEnum.UserNameExists, result);
    }

    [Fact]
    public async Task RegisterUser_UserAccountExists()
    {
        const string email = "user@test.com";

        await AddUser("username1", "password1", email, UserRoleEnum.NormalUser);

        await _registrationService.SendRegistrationLinkAsync(email);
        var registration = await DbContext.Registration.FirstOrDefaultAsync();
        Assert.NotNull(registration);

        var userToRegister = new UserToRegisterDto
        {
            Link = registration.Link,
            Username = "username2",
            Password = "password2",
            Role = UserRoleEnum.NormalUser
        };

        var result = await _registrationService.RegisterUserAsync(userToRegister);
        Assert.Equal(RegistrationSecondResultEnum.UserAccountExists, result);
    }
}