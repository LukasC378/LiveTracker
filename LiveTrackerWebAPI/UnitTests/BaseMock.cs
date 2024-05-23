using DB.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using DB.Entities;
using DB.Enums;

namespace UnitTests;

public class BaseMock : IDisposable
{
    protected readonly Mock<IHttpContextAccessor> HttpContextAccessorMock;
    protected readonly TestDbContext DbContext;

    public BaseMock()
    {
        HttpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, "1")
            }))
        };
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var inMemorySettings = new Dictionary<string, string> {
            {"ConnectionStrings:DefaultConnection", "connection"}
        };
        IConfiguration configurationMock = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        var options = new DbContextOptionsBuilder<RaceTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new TestDbContext(options, configurationMock);
    }

    protected async Task<User> AddUser(string username = "username", string password = "password", 
        string email = "testuser@example.com", UserRoleEnum role = UserRoleEnum.NormalUser)
    {
        var user = new User
        {
            Username = username,
            PasswordSalt = "salt",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(password + "salt"),
            Email = email,
            Role = role
        };

        DbContext.User.Add(user);
        await DbContext.SaveChangesAsync();

        return user;
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}