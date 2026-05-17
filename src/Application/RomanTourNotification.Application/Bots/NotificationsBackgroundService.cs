using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Abstractions.Time;
using RomanTourNotification.Application.Contracts.Notifications;

namespace RomanTourNotification.Application.Bots;

/// <summary>
/// Polls once per minute and delegates to every registered <see cref="IScheduledNotification"/>.
/// Each implementation decides for itself whether it should fire (time, day-of-week, etc.).
/// </summary>
public class NotificationsBackgroundService : BackgroundService
{
    private readonly ILogger<NotificationsBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IClock _clock;

    /// <summary>Initializes a new instance of the <see cref="NotificationsBackgroundService"/> class.</summary>
    public NotificationsBackgroundService(
        ILogger<NotificationsBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IClock clock)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _clock = clock;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                DateTime utcNow = _clock.UtcNow;
                DayOfWeek today = _clock.Today.DayOfWeek;

                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IEnumerable<IScheduledNotification> notifications =
                    scope.ServiceProvider.GetRequiredService<IEnumerable<IScheduledNotification>>();

                foreach (IScheduledNotification notification in notifications)
                {
                    if (notification.ShouldSend(utcNow, today))
                        await notification.SendAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background notification service error");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
