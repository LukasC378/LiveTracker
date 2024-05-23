using BL.Models.Dto;
using BL.Services;
using BL.Services.Interfaces;
using DB.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.ServicesTests;

public class LayoutServiceTests : BaseMock
{
    private readonly ILayoutsService _layoutService;
    private readonly Mock<IUserService> _userServiceMock = new();

    private readonly int _userId;

    public LayoutServiceTests()
    {
        _layoutService = new LayoutsService(DbContext, _userServiceMock.Object);

        var user = AddUser("username", "password").GetAwaiter().GetResult();
        _userId = user.Id;
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(1);
        _userServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
    }

    [Fact]
    public async Task CreateLayout_Test()
    {
        await CreateLayout();

        var layout = await DbContext.Layout.FirstOrDefaultAsync();
        Assert.NotNull(layout);
        Assert.Equal("TestLayout", layout.Name);
        Assert.Equal("geoJson", layout.GeoJson);
        Assert.Equal(_userId, layout.UserId);
        Assert.Equal(true, layout.Active);
    }

    [Fact]
    public async Task GetLayout_Test()
    {
        await CreateLayout();

        var layout = await DbContext.Layout.FirstOrDefaultAsync();
        Assert.NotNull(layout);

        var layoutDto = await _layoutService.GetLayout(layout.Id);
        Assert.NotNull(layoutDto);

        Assert.Equal(layout.Name, layoutDto.Name);
        Assert.Equal(layout.GeoJson, layoutDto.GeoJson);
    }

    [Fact]
    public async Task UpdateUnusedLayout_Test()
    {
        await CreateLayout();

        var layout = await DbContext.Layout.FirstOrDefaultAsync();
        Assert.NotNull(layout);

        var layoutDto = await _layoutService.GetLayout(layout.Id);
        Assert.NotNull(layoutDto);

        const string newGeoJson = "newGeoJson";
        const string newLayoutName = "newLayoutName";
        layoutDto.GeoJson = newGeoJson;
        layoutDto.Name = newLayoutName;

        await _layoutService.UpdateLayout(layoutDto);

        Assert.Equal(newGeoJson, layout.GeoJson);
        Assert.Equal(newLayoutName, layout.Name);
    }

    [Fact]
    public async Task UpdateUsedLayout_Test()
    {
        await CreateLayout();

        var layout = await DbContext.Layout.FirstOrDefaultAsync();
        Assert.NotNull(layout);

        await CreateSession(layout.Id);

        var layoutDto = await _layoutService.GetLayout(layout.Id);
        Assert.NotNull(layoutDto);

        const string newGeoJson = "newGeoJson";
        const string newLayoutName = "newLayoutName";
        layoutDto.GeoJson = newGeoJson;
        layoutDto.Name = newLayoutName;

        await _layoutService.UpdateLayout(layoutDto);

        var newLayout = await DbContext.Layout.FirstOrDefaultAsync(x => x.Name == newLayoutName);
        Assert.NotNull(newLayout);
        Assert.Equal(newGeoJson, newLayout.GeoJson);
        Assert.Equal(newLayoutName, newLayout.Name);
        Assert.Equal("geoJson", layout.GeoJson);
        Assert.Equal("TestLayout", layout.Name);

        
    }

    [Fact]
    public async Task DeleteUnusedLayout_Test()
    {
        await CreateLayout();

        var layout = await DbContext.Layout.FirstOrDefaultAsync();
        Assert.NotNull(layout);

        await _layoutService.DeleteLayout(layout.Id);

        var layouts = await DbContext.Layout.ToListAsync();
        Assert.Empty(layouts);
    }

    [Fact]
    public async Task DeleteUsedLayout_Test()
    {
        await CreateLayout();

        var layout = await DbContext.Layout.FirstOrDefaultAsync();
        Assert.NotNull(layout);

        await CreateSession(layout.Id);

        await _layoutService.DeleteLayout(layout.Id);

        Assert.Equal(false, layout.Active);
    }

    private async Task CreateLayout()
    {
        var layout = new LayoutDto
        {
            GeoJson = "geoJson",
            Name = "TestLayout",
        };
        await _layoutService.CreateLayout(layout);
    }

    private async Task CreateSession(int layoutId)
    {
        var driver = new Driver
        {
            FirstName = "FirstName",
            LastName = "LastName",
            GpsDevice = Guid.NewGuid(),
            Number = 33
        };

        DbContext.Add(driver);

        var collection = new Collection
        {
            Active = true,
            Name = "TestCollection",
            UserId = _userId,
            UseTeams = false
        };
        DbContext.Add(collection);

        await DbContext.SaveChangesAsync();

        var driverCollection = new DriverCollection
        {
            DriverId = driver.Id,
            CollectionId = collection.Id,
        };
        DbContext.Add(driverCollection);
        await DbContext.SaveChangesAsync();

        var session = new Session
        {
            Name = "TestSession",
            CollectionId = collection.Id,
            Laps = 2,
            LayoutId = layoutId,
            ScheduledFrom = DateTime.Now,
            ScheduledTo = DateTime.Now.AddDays(1),
        };

        DbContext.Add(session);
        await DbContext.SaveChangesAsync();
    }
}