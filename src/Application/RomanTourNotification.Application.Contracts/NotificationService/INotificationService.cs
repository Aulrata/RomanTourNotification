using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.NotificationService;

/// <summary>
/// Dispatches a manual (forced) notification for a specific group and type,
/// triggered by the Telegram bot UI.
/// </summary>
public interface INotificationService
{
    /// <summary>Sends a forced notification for <paramref name="group"/> of the given <paramref name="type"/>.</summary>
    public Task SendForcedNotificationAsync(Group? group, GroupType type, CancellationToken cancellationToken);
}
