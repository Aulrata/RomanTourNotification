namespace RomanTourNotification.Application.Models.EnrichmentNotification;

public class TimeSettings
{
    /// <summary>Gets the UTC times of day ("HH:mm") for the main notifications.</summary>
    public IReadOnlyCollection<TimeSpan> MainTimesUtc { get; init; } = [];

    /// <summary>Gets the UTC times of day ("HH:mm") for the Wednesday return notifications.</summary>
    public IReadOnlyCollection<TimeSpan> ReturnTimesUtc { get; init; } = [];

    /// <summary>Gets the UTC times of day ("HH:mm") at which receipt notifications are sent.</summary>
    public IReadOnlyCollection<TimeSpan> ReceiptTimesUtc { get; init; } = [];

    /// <summary>Returns true if <paramref name="utcNow"/> falls on one of <paramref name="times"/> (to the minute).</summary>
    public static bool IsSendTime(IEnumerable<TimeSpan> times, DateTime utcNow)
        => times.Any(t => t.Hours == utcNow.Hour && t.Minutes == utcNow.Minute);
}
