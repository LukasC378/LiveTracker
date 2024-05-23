using System.ComponentModel.DataAnnotations;
using DB.Enums;
using Microsoft.EntityFrameworkCore;

namespace DB.Entities;

[Index(nameof(SessionId))]
public class SessionNotification
{
    [Key]
    public int Id { get; set; }
    public int SessionId { get; set; }
    public NotificationTypeEnum NotificationType { get; set; }
}