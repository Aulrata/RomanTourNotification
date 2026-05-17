using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.Notifications;

/// <summary>
/// Represents a notification that can be triggered manually for a specific group
/// via the Telegram bot UI (forced send).
/// Only notification types that make sense as one-off triggers need to implement this.
/// </summary>
public interface IForcedNotification
{
    /// <summary>Gets the <see cref="GroupType"/> this handler is responsible for.</summary>
    public GroupType HandledType { get; }

    /// <summary>Sends the notification immediately for the given <paramref name="group"/>.</summary>
    public Task SendForGroupAsync(Group group, CancellationToken cancellationToken);
}
