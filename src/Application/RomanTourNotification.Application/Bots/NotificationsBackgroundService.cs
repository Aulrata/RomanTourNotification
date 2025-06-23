using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.NotificationService;
using RomanTourNotification.Application.Models.EnrichmentNotification;

namespace RomanTourNotification.Application.Bots;

public class NotificationsBackgroundService : BackgroundService
{
    private readonly ILogger<NotificationsBackgroundService> _logger;
    private readonly TimeSettings _timeSettings;
    private readonly INotificationService _notificationService;

    public NotificationsBackgroundService(
        ILogger<NotificationsBackgroundService> logger,
        TimeSettings timeSettings,
        INotificationService notificationService)
    {
        _logger = logger;
        _timeSettings = timeSettings;
        _notificationService = notificationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting background notification service. With UTC time " +
                               $"{_timeSettings.HoursUtc}:{_timeSettings.Minutes}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!(DateTime.UtcNow.Hour == _timeSettings.HoursUtc
                      && DateTime.UtcNow.Minute == _timeSettings.Minutes))
                {
                    continue;
                }

                if (DateTime.Today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                await _notificationService.SendNotificationAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}