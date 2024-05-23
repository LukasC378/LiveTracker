using BL.Models.Logic;
using BL.Services.Interfaces;
using DB.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BL.Workers;

public class NotificationWorker : BackgroundService
{
    private const int PeriodSeconds = 50000;
    private readonly ILogger<NotificationWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationWorker(ILogger<NotificationWorker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            _logger.LogInformation("Worker running at {date}", DateTime.Now);

            var startTime = DateTime.Now;

            var processNotificationsTask = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await ProcessSessionNotifications(notificationService, emailService);
            }, stoppingToken);

            var processLoadRacesTask = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                var sessionApiService = scope.ServiceProvider.GetRequiredService<ISessionApiService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await ProcessRacesLoad(sessionService, sessionApiService, emailService);
            }, stoppingToken);

            var processUnloadRacesTask = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
                var sessionApiService = scope.ServiceProvider.GetRequiredService<ISessionApiService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await ProcessRacesUnload(sessionService, sessionApiService, emailService);
            }, stoppingToken);

            await Task.WhenAll(processNotificationsTask, processLoadRacesTask, processUnloadRacesTask);

            var endTime = DateTime.Now;
            var delay = TimeSpan.FromSeconds(PeriodSeconds) - (endTime - startTime);
            if (delay.Seconds <= 0)
            {
                delay = TimeSpan.FromSeconds(0);
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task ProcessSessionNotifications(INotificationService notificationService, IEmailService emailService)
    {
        var notificationsWithDetails = await notificationService.LoadNewNotificationsWithDetails();
        var emailTasks = new List<Task>();

        foreach (var notification in notificationsWithDetails)
        {
            var template = notification.Notification.NotificationType switch
            {
                NotificationTypeEnum.Created => emailService.SessionCreatedNotificationTemplate(notification.Session),
                NotificationTypeEnum.Updated => emailService.SessionUpdatedNotificationTemplate(notification.Session),
                NotificationTypeEnum.Cancelled => emailService.SessionCancelledNotificationTemplate(
                    notification.Session),
                _ => throw new NotSupportedException(
                    $"Unknown notificationType {notification.Notification.NotificationType}")
            };

            emailTasks.AddRange(notification.Emails.Select(email => SendEmail(emailService, email, template)));
        }

        var notificationToDelete = notificationsWithDetails.Select(x => x.Notification);
        if (notificationToDelete.Any())
        {
            await notificationService.DeleteNotifications(notificationToDelete);
        }

        await Task.WhenAll(emailTasks);
    }

    private async Task ProcessRacesLoad(ISessionService sessionService, ISessionApiService sessionApiService, IEmailService emailService)
    {
        var sessionToLoad = await sessionService.GetSessionsToLoad();
        var loadedSessionIds = new List<int>();
        var emailTasks = new List<Task>();

        async ValueTask Action(SessionToLoad session, CancellationToken cancellationToken)
        {
            try
            {
                await sessionApiService.LoadRace(session.RaceData);
                loadedSessionIds.Add(session.RaceData.RaceId);

                var template = emailService.ReminderTemplate(session.EmailInfo);
                emailTasks.AddRange(session.Emails.Select(email => SendEmail(emailService, email, template)));
            }
            catch (Exception ex)
            {
                _logger.LogError("{method}, {ex}", nameof(ProcessRacesLoad), ex);
            }
        }

        await Parallel.ForEachAsync(sessionToLoad, Action);
        await sessionService.UpdateLoadedSessions(loadedSessionIds);
        await Task.WhenAll(emailTasks);
    }

    private async Task ProcessRacesUnload(ISessionService sessionService, ISessionApiService sessionApiService, IEmailService emailService)
    {
        var sessionToUnload = await sessionService.GetSessionsToUnload();
        var unloadedSessionResults = new List<(int RaceId, IEnumerable<Guid> Result)>();
        var emailTasks = new List<Task>();

        async ValueTask Action(SessionToUnload session, CancellationToken cancellationToken)
        {
            try
            {
                var result = await sessionApiService.UnloadRace(session.EmailInfo.Id);
                if (result != null)
                {
                    unloadedSessionResults.Add((session.EmailInfo.Id, result));
                }

                var template = emailService.SessionEndedTemplate(session.EmailInfo);
                emailTasks.AddRange(session.Emails.Select(email => SendEmail(emailService, email, template)));
            }
            catch (Exception ex)
            {
                _logger.LogError("{method}, {ex}", nameof(ProcessRacesUnload), ex);
            }
        }

        await Parallel.ForEachAsync(sessionToUnload, Action);
        await sessionService.UpdateEndedSessions(unloadedSessionResults);
        await Task.WhenAll(emailTasks);
    }

    private async Task SendEmail(IEmailService emailService, string email, (string Subject, string? Body) template)
    {
        try
        {
            await emailService.SendEmail(email, template.Subject, template.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError("{method}, {ex}", nameof(SendEmail), ex);
        }
    }
}