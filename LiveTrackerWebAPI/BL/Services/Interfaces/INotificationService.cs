using BL.Models.Logic;
using DB.Entities;
using DB.Enums;

namespace BL.Services.Interfaces;

public interface INotificationService
{
    Task CreateNotification(int sessionId, NotificationTypeEnum type);
    Task<IList<NotificationDetail>> LoadNewNotificationsWithDetails();
    Task DeleteNotifications(IEnumerable<SessionNotification> notifications);
}