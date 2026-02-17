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
        _logger.LogInformation(
            "Starting background notification service. With UTC time {HoursUtc}:{Minutes}",
            _timeSettings.HoursUtc,
            _timeSettings.Minutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (DateTime.Today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                if (DateTime.UtcNow.Hour == _timeSettings.HoursUtc && DateTime.UtcNow.Minute == _timeSettings.Minutes)
                    await _notificationService.SendNotificationAsync(stoppingToken);

                if (DateTime.UtcNow.Hour == _timeSettings.ReceiptHoursUtc
                    && DateTime.UtcNow.Minute == _timeSettings.ReceiptMinutes)
                {
                    await _notificationService.SendReceiptNotificationAsync(stoppingToken);
                }

                if (DateTime.UtcNow.Hour == _timeSettings.ReturnHoursUtc
                    && DateTime.UtcNow.Minute == _timeSettings.ReturnMinute
                    && DateTime.Today.DayOfWeek is DayOfWeek.Wednesday)
                {
                    await _notificationService.SendSpecialNotificationAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}