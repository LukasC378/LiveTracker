using BL.Models.ViewModels;
using DB.Entities;

namespace BL.Models.Logic;

public class NotificationDetail
{
    public required SessionEmailWMExtended Session { get; set; }
    public IEnumerable<string> Emails { get; set; } = Enumerable.Empty<string>();
    public required SessionNotification Notification { get; set; }
}