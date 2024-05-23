using BL.Models.ViewModels;
using BL.Services;
using BL.Services.Interfaces;
using DB.Entities;
using DB.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests.ServicesTests;

public class NotificationServiceTests : BaseMock
{
    private readonly INotificationService _notificationService;
    private readonly Mock<ISubscribeService> _subscribeServiceMock = new();

    private readonly int _userId;

    public NotificationServiceTests()
    {
        _notificationService = new NotificationService(DbContext, _subscribeServiceMock.Object);

        var user = AddUser().GetAwaiter().GetResult();
        _userId = user.Id;

        _subscribeServiceMock.Setup(x => x.GetSubscribersForOrganizer(It.IsAny<int>()))
            .ReturnsAsync(new List<UserVM>());
    }

    [Fact]
    public async Task CreateNotification_Test()
    {
        await CreateSession();

        var session = await DbContext.Session.FirstOrDefaultAsync();
        Assert.NotNull(session);

        await _notificationService.CreateNotification(session.Id, NotificationTypeEnum.Created);

        var notification = await DbContext.SessionNotification.FirstOrDefaultAsync();
        Assert.NotNull(notification);
        Assert.Equal(session.Id, notification.SessionId);
        Assert.Equal(NotificationTypeEnum.Created, notification.NotificationType);
    }

    [Fact]
    public async Task LoadNotification_Test()
    {
        await CreateSession();

        var session = await DbContext.Session.FirstOrDefaultAsync();
        Assert.NotNull(session);

        await _notificationService.CreateNotification(session.Id, NotificationTypeEnum.Created);

        var notifications = await _notificationService.LoadNewNotificationsWithDetails();
        Assert.NotNull(notifications);
        Assert.Equal(1, notifications.Count);

        var notification = notifications[0];
        Assert.Equal(_userId, notification.Session.OrganizerId);
        Assert.Equal(session.Id, notification.Notification.SessionId);
    }

    [Fact]
    public async Task DeleteNotification_Test()
    {
        await CreateSession();

        var session = await DbContext.Session.FirstOrDefaultAsync();
        Assert.NotNull(session);

        await _notificationService.CreateNotification(session.Id, NotificationTypeEnum.Created);

        var notifications = await DbContext.SessionNotification.ToListAsync();
        Assert.Equal(1, notifications.Count);

        await _notificationService.DeleteNotifications(new[] { notifications[0] });

        var notificationsCount2 = await DbContext.SessionNotification.CountAsync();
        Assert.Equal(0, notificationsCount2);
    }

    private async Task CreateSession()
    {
        var layout = new Layout
        {
            UserId = _userId,
            Active = true,
            GeoJson = "geoJson",
            Name = "TestLayout"
        };
        DbContext.Add(layout);

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
            LayoutId = layout.Id,
            ScheduledFrom = DateTime.Now,
            ScheduledTo = DateTime.Now.AddDays(1),
            UserId = _userId
        };

        DbContext.Add(session);
        await DbContext.SaveChangesAsync();
    }
}