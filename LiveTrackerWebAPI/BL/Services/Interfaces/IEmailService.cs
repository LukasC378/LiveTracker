using BL.Models.ViewModels;
using DB.Entities;

namespace BL.Services.Interfaces;

public interface IEmailService
{
    Task SendRegistrationLink(string email, string link);
    Task SendSessionToken(string email, string token, Session session);
    (string Subject, string? Body) SessionCreatedNotificationTemplate(SessionEmailWMExtended session);
    (string Subject, string? Body) SessionUpdatedNotificationTemplate(SessionEmailVM session);
    (string Subject, string? Body) SessionCancelledNotificationTemplate(SessionEmailVM session);
    (string Subject, string? Body) ReminderTemplate(SessionEmailVM session);
    (string Subject, string? Body) SessionEndedTemplate(SessionEmailVM session);
    Task SendEmail(string toEmail, string subject, string? body);
}