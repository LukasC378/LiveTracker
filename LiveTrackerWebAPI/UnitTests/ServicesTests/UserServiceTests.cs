using BL.Services;
using BL.Services.Interfaces;
using Moq;
using DB.Enums;

namespace UnitTests.ServicesTests;

public class UserServiceTests : BaseMock
{
    private readonly Mock<IJWTService> _jwtServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _jwtServiceMock = new Mock<IJWTService>();
        _userService = new UserService(DbContext, HttpContextAccessorMock.Object, _jwtServiceMock.Object);

        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserRoleEnum>()))
            .Returns("access_token");
    }

    [Fact]
    public async Task Login_OK()
    {
        const string username = "testuser";
        const string password = "password";

        var user = await AddUser(username, password);

        var result = await _userService.Login(username, password);

        Assert.NotNull(result);
        Assert.Equal(user.Username, result.Username);
    }

    [Fact]
    public async Task Login_Unauthorized()
    {
        const string username = "testuser";
        const string password = "password";

        await AddUser(username, password);

        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<UserRoleEnum>()))
            .Returns("access_token");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _userService.Login(username, "wrong_password"));
    }

    [Fact]
    public async Task CurrentUser()
    {
        const string username = "testuser";
        const string password = "password";

        var user = await AddUser(username, password);

        await _userService.Login(username, password);

        var currentUser = await _userService.GetCurrentUser();

        Assert.NotNull(currentUser);
        Assert.Equal(user.Id, currentUser.Id);
        Assert.Equal(user.Username, currentUser.Username);
    }
}