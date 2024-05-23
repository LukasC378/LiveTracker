using BL;
using BL.Models.Dto;
using BL.Models.Logic;
using BL.Services;
using BL.Services.Interfaces;
using DB.Entities;
using LiveTrackerModels;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.ServicesTests;

public class SessionServiceTests : BaseMock
{
    private readonly ISessionService _sessionService;
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ISessionApiService> _sessionApiServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IArchiveService> _archiveServiceMock = new();

    private readonly int _userId;

    public SessionServiceTests()
    {
        _sessionService = new SessionService(DbContext, _userServiceMock.Object, _sessionApiServiceMock.Object, _emailServiceMock.Object,
            _archiveServiceMock.Object, _notificationServiceMock.Object);

        var user = AddUser("username", "password").GetAwaiter().GetResult();
        _userId = user.Id;
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(1);
        _userServiceMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);

        _sessionApiServiceMock.Setup(x => x.GetSessionToken(It.IsAny<TokenInput>())).ReturnsAsync("token");
    }

    [Fact]
    public async Task CreateSession_Test()
    {
        await CreateSession(DateTime.Now);

        var session = DbContext.Session.FirstOrDefault();

        Assert.NotNull(session);
        Assert.Equal(1, session.Id);
        Assert.Equal("token", session.Token);
    }

    [Fact]
    public async Task GetSessionVM_Test()
    {
        await CreateSession(DateTime.Now);

        var session = DbContext.Session.FirstOrDefault();

        Assert.NotNull(session);

        var sessionVM = await _sessionService.GetSession(session.Id);

        Assert.NotNull(sessionVM);
        Assert.Equal(1, sessionVM.Drivers.Count);
        Assert.Equal("geoJson1", sessionVM.GeoJson);
        Assert.Equal(2, sessionVM.Laps);
    }

    [Fact]
    public async Task UpdateSession_OK()
    {
        await CreateSession(DateTime.Now.AddDays(1));

        var session = DbContext.Session.FirstOrDefault();
        Assert.NotNull(session);

        var sessionDto = await _sessionService.GetSessionToEdit(session.Id);
        Assert.NotNull(sessionDto);

        const int laps = 5;
        const string newName = "UpdatedSession";
        const string newGeoJson = "newGeoJson";

        sessionDto.Name = newName;
        sessionDto.Laps = laps;
        sessionDto.LayoutId = 0;
        sessionDto.GeoJson = newGeoJson;
        await _sessionService.UpdateSession(sessionDto);

        Assert.Equal(laps, session.Laps);
        Assert.Equal(newName, session.Name);
        Assert.Equal(newGeoJson, session.GeoJson);
    }

    [Fact]
    public async Task UpdateSession_LessThanHour()
    {
        await CreateSession(DateTime.Now);

        var session = DbContext.Session.FirstOrDefault();
        Assert.NotNull(session);

        var sessionDto = await _sessionService.GetSessionToEdit(session.Id);
        Assert.NotNull(sessionDto);

        const int laps = 5;
        const string newName = "UpdatedSession";
        const string newGeoJson = "newGeoJson";

        sessionDto.Name = newName;
        sessionDto.Laps = laps;
        sessionDto.LayoutId = 0;
        sessionDto.GeoJson = newGeoJson;

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sessionService.UpdateSession(sessionDto));
    }

    [Fact]
    public async Task CancelSession_OK()
    {
        await CreateSession(DateTime.Now.AddDays(1));

        var session = DbContext.Session.FirstOrDefault();
        Assert.NotNull(session);

        await _sessionService.CancelSession(session.Id);

        var sessions = await DbContext.Session.ToListAsync();
        Assert.Empty(sessions);
    }


    [Fact]
    public async Task SessionLoad_Test()
    {
        await CreateSession(DateTime.Now);

        var session = DbContext.Session.FirstOrDefault();
        Assert.NotNull(session);

        var sessionsToLoad = (await _sessionService.GetSessionsToLoad()).ToList();
        Assert.NotNull(sessionsToLoad);
        Assert.Equal(1, sessionsToLoad.Count);

        var sessionToLoad = sessionsToLoad.First();
        Assert.Equal(1, sessionToLoad.RaceData.Drivers.Count);
        Assert.Equal("geoJson1", sessionToLoad.RaceData.GeoJsonData);

        await _sessionService.UpdateLoadedSessions(new[] { session.Id });

        Assert.Equal(true, session.Loaded);
    }

    [Fact]
    public async Task SessionFilter_Test()
    {
        await CreateSession(DateTime.Now, 1);
        await CreateSession(DateTime.Now.AddDays(1), 2);

        var session1 = DbContext.Session.FirstOrDefault(x => x.Name == "TestSession1");
        var session2 = DbContext.Session.FirstOrDefault(x => x.Name == "TestSession2");
        Assert.NotNull(session1);
        Assert.NotNull(session2);

        await _sessionService.UpdateLoadedSessions(new[] { session1.Id });

        var liveSessions = (await _sessionService.GetLiveSessions()).ToList();
        Assert.Equal(1, liveSessions.Count);
        
        var liveSessionGroup = liveSessions.First();
        Assert.Equal(DateTime.Today, liveSessionGroup.Date);
        Assert.Equal(1, liveSessionGroup.Sessions.Count);

        var liveSession = liveSessionGroup.Sessions.First();
        Assert.Equal(SessionStateEnum.Live, liveSession.State);
        Assert.Equal("TestSession1", liveSession.Name);

        var upcomingSessions = (await _sessionService.GetFilteredSessions(new SessionFilter
        {
            SessionState = SessionStateFilterEnum.Upcoming
        })).ToList();
        Assert.Equal(1, upcomingSessions.Count);
        
        var upcomingSessionGroup = upcomingSessions.First();
        Assert.Equal(DateTime.Today.AddDays(1), upcomingSessionGroup.Date);
        Assert.Equal(1, upcomingSessionGroup.Sessions.Count);

        var upcomingSession = upcomingSessionGroup.Sessions.First();
        Assert.Equal(SessionStateEnum.Preview, upcomingSession.State);
        Assert.Equal("TestSession2", upcomingSession.Name);
    }


    private async Task CreateSession(DateTime scheduledFrom, int number = 1)
    {
        var layout = new Layout
        {
            UserId = _userId,
            Active = true,
            GeoJson = "geoJson" + number,
            Name = "TestLayout" + number
        };
        DbContext.Add(layout);


        var driver = new Driver
        {
            FirstName = "FirstName" + number,
            LastName = "LastName" + number,
            GpsDevice = Guid.NewGuid(),
            Number = 33
        };

        DbContext.Add(driver);

        var collection = new Collection
        {
            Active = true,
            Name = "TestCollection" + number,
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

        var sessionDto = new SessionDto
        {
            Name = "TestSession" + number,
            CollectionId = collection.Id,
            Laps = 2,
            LayoutId = layout.Id,
            ScheduledFrom = scheduledFrom,
            ScheduledTo = scheduledFrom.AddDays(1),
        };

        await _sessionService.CreateSession(sessionDto);
    }
}