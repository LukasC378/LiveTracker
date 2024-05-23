using BL.Models.Logic;
using BL.Models.ViewModels;
using BL.Services.Interfaces;
using DB.Database;
using DB.Entities;
using DB.Enums;
using Microsoft.EntityFrameworkCore;

namespace BL.Services;

public class NotificationService : BaseService, INotificationService
{
    private readonly ISubscribeService _subscribeService;

    public NotificationService(RaceTrackerDbContext dbContext, ISubscribeService subscribeService) : base(dbContext)
    {
        _subscribeService = subscribeService;
    }

    public  async Task CreateNotification(int sessionId, NotificationTypeEnum type)
    {
        var sessionNotification = new SessionNotification
        {
            SessionId = sessionId,
            NotificationType = type
        };
        await DbContext.AddAsync(sessionNotification);
        await DbContext.SaveChangesAsync();
    }

    public async Task<IList<NotificationDetail>> LoadNewNotificationsWithDetails()
    {
        var query = from notification in DbContext.SessionNotification
            join session in DbContext.Session on notification.SessionId equals session.Id
            join organizer in DbContext.User on session.UserId equals organizer.Id
            select new NotificationDetail
            {
                Session = new SessionEmailWMExtended
                {
                    Id = session.Id,
                    Name = session.Name,
                    ScheduledFrom = session.ScheduledFrom,
                    ScheduledTo = session.ScheduledTo,
                    Organizer = organizer.Username,
                    OrganizerId = organizer.Id
                },
                Notification = notification,
            };

        var notificationDetails = await query.ToListAsync();

        foreach (var notificationDetail in notificationDetails)
        {
            notificationDetail.Emails = (await _subscribeService.GetSubscribersForOrganizer(notificationDetail.Session.OrganizerId)).Select(x => x.Email);
        }

        return notificationDetails;
    }

    public async Task DeleteNotifications(IEnumerable<SessionNotification> notifications)
    {
        DbContext.RemoveRange(notifications);
        await DbContext.SaveChangesAsync();
    }
}