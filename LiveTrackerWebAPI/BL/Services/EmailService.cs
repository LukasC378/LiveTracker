using BL.Models.ViewModels;
using BL.Services.Interfaces;
using DB.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BL.Services;

public class EmailService : IEmailService
{
    #region Declaration

    private readonly string _fromEmail;
    private readonly string _emailPassword;
    private readonly int _port;
    private readonly string _host;
    private readonly string _webSiteUrl;
    private readonly bool _debugMode;

    private const string EmailName = "Live Tracker Team";

    #endregion

    public EmailService(IConfiguration configuration)
    {
        var emailConfig = configuration.GetSection("EmailSettings");
        _fromEmail = emailConfig["Email"]!;
        _emailPassword = emailConfig["Password"]!;
        _port = int.Parse(emailConfig["Port"]!);
        _host = emailConfig["Host"]!;
        _debugMode = int.Parse(emailConfig["Debug"]!) == 1;
        _webSiteUrl = configuration.GetSection("WebSite")["Url"]!;
    }

    public async Task SendRegistrationLink(string email, string link)
    {
        const string subject = "Live Tracker Registration";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, thanks for joining us!</h1>
                    <p>
                        Please continue with your registration on <a href=""{_webSiteUrl}/sign-up/{link}"">Registration page</a>
                    </p>
                </body>
            </html>
        ";

        await SendEmail(email, subject, body);
    }

    public async Task SendSessionToken(string email, string token, Session session)
    {
        const string subject = "Live Tracker Session Token";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, this is info for your session</h1>                 
                    <p>
                        Token: <strong>{token}</strong><br>
                        Name: {session.Name}<br>
                        Scheduled from: {session.ScheduledFrom}<br>
                        Scheduled to: {session.ScheduledTo}
                    </p>
                    <p>
                        Please keep this token securely.
                    </p>
                </body>
            </html>
        ";

        await SendEmail(email, subject, body);
    }

    public (string Subject, string? Body) SessionCreatedNotificationTemplate(SessionEmailWMExtended session)
    {
        const string subject = "New Session Scheduled";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, organizer {session.Organizer} just scheduled new session</h1>                 
                    <p>                 
                        Session: {session.Name}<br>
                        Scheduled from: {session.ScheduledFrom}<br>
                        Scheduled to: {session.ScheduledTo}
                    </p>
                    <p>
                        Check more info on <a href=""{_webSiteUrl}/sessions/{session.Id}"">.
                    </p>
                </body>
            </html>
        ";

        return (subject, body);
    }

    public (string Subject, string? Body) SessionUpdatedNotificationTemplate(SessionEmailVM session)
    {
        const string subject = "Session Rescheduled";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, session {session.Name} has been rescheduled</h1>                 
                    <p>                 
                        Session: {session.Name}<br>
                        Scheduled from: {session.ScheduledFrom}<br>
                        Scheduled to: {session.ScheduledTo}
                    </p>
                    <p>
                        Check more info on <a href=""{_webSiteUrl}/sessions/{session.Id}"">.
                    </p>
                </body>
            </html>
        ";

        return (subject, body);
    }

    public (string Subject, string? Body) SessionCancelledNotificationTemplate(SessionEmailVM session)
    {
        const string subject = "Session Cancelled";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, session {session.Name} has been cancelled</h1>                 
                    <p>                 
                        Session: {session.Name}<br>
                        Scheduled from: {session.ScheduledFrom}<br>
                        Scheduled to: {session.ScheduledTo}
                    </p>
                </body>
            </html>
        ";

        return (subject, body);
    }

    public (string Subject, string? Body) ReminderTemplate(SessionEmailVM session)
    {
        const string subject = "Session Starts In Less Than 30 Minutes";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, session {session.Name} has been starts in less than 30 minutes</h1>                 
                    <p>                 
                        Session: {session.Name}<br>
                        Scheduled from: {session.ScheduledFrom}<br>
                        Scheduled to: {session.ScheduledTo}
                    </p>
                    <p>
                        Check more info on <a href=""{_webSiteUrl}/sessions/{session.Id}"">.
                    </p>
                </body>
            </html>
        ";

        return (subject, body);
    }

    public (string Subject, string? Body) SessionEndedTemplate(SessionEmailVM session)
    {
        const string subject = "Session Ended";
        var body = @$"
            <html>
                <head>
                    <title>{subject}</title>
                </head>
                <body>
                    <h1>Hello, session {session.Name} has ended</h1>                 
                    <p>                 
                        Session: {session.Name}<br>
                        Scheduled from: {session.ScheduledFrom}<br>
                        Scheduled to: {session.ScheduledTo}
                    </p>
                    <p>
                        Check result on <a href=""{_webSiteUrl}/sessions/result/{session.Id}"">.
                    </p>
                </body>
            </html>
        ";

        return (subject, body);
    }

    public async Task SendEmail(string toEmail, string subject, string? body)
    {
        if(_debugMode)
            return;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(EmailName, _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };

        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_fromEmail, _emailPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception)
        {
            throw new Exception("Email send failed");
        }
    }
}