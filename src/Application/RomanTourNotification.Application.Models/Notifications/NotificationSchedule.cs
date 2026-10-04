namespace RomanTourNotification.Application.Models.Notifications;

/// <summary>When a single notification type is sent: on which days and at which UTC times.</summary>
public class NotificationSchedule
{
    /// <summary>Gets the UTC times of day ("HH:mm") at which the notification is sent.</summary>
    public IReadOnlyCollection<TimeSpan> TimesUtc { get; init; } = [];

    /// <summary>Gets the days of week ("Monday", "Tuesday", ...) on which the notification is sent.</summary>
    public IReadOnlyCollection<DayOfWeek> Days { get; init; } = [];

    /// <summary>Returns true if <paramref name="today"/> and <paramref name="utcNow"/> (to the minute) match the schedule.</summary>
    public bool IsDue(DateTime utcNow, DayOfWeek today)
        => Days.Contains(today)
           && TimesUtc.Any(t => t.Hours == utcNow.Hour && t.Minutes == utcNow.Minute);
}
