using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Notifications;
using RomanTourNotification.Application.Contracts.NotificationService;
using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.NotificationService;

/// <summary>
/// Dispatches forced (bot-triggered) notifications by finding the matching
/// <see cref="IForcedNotification"/> handler for the requested <see cref="GroupType"/>.
/// Scheduled notifications are handled independently by <see cref="IScheduledNotification"/> implementations.
/// </summary>
public class TelegramService : INotificationService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IEnumerable<IForcedNotification> _handlers;

    /// <summary>Initializes a new instance of the <see cref="TelegramService"/> class.</summary>
    public TelegramService(
        ILogger<TelegramService> logger,
        IEnumerable<IForcedNotification> handlers)
    {
        _logger = logger;
        _handlers = handlers;
    }

    /// <inheritdoc/>
    public async Task SendForcedNotificationAsync(Group? group, GroupType type, CancellationToken cancellationToken)
    {
        if (group is null)
        {
            _logger.LogWarning("SendForcedNotificationAsync called with null group for type {Type}", type);
            return;
        }

        IForcedNotification? handler = _handlers.FirstOrDefault(h => h.HandledType == type);

        if (handler is null)
        {
            _logger.LogWarning("No forced notification handler registered for GroupType {Type}", type);
            return;
        }

        await handler.SendForGroupAsync(group, cancellationToken);
    }
}
