namespace RomanTourNotification.Application.Contracts.Notifications;

/// <summary>
/// Represents a notification that knows its own schedule and can send itself.
/// Implementations are resolved via <see cref="System.Collections.Generic.IEnumerable{T}"/>
/// by the background scheduler, which calls <see cref="ShouldSend"/> each minute and
/// delegates to <see cref="SendAsync"/> when the time matches.
/// </summary>
public interface IScheduledNotification
{
    /// <summary>Returns <see langword="true"/> when this notification should fire.</summary>
    public bool ShouldSend(DateTime utcNow, DayOfWeek today);

    /// <summary>Executes the notification for all relevant groups.</summary>
    public Task SendAsync(CancellationToken cancellationToken);
}
