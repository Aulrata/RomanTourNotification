namespace RomanTourNotification.Application.Models.Notifications;

/// <summary>Per-notification schedules, bound from the "Schedules" configuration section.</summary>
public class ScheduleSettings
{
    public NotificationSchedule DocumentsForDeparture { get; init; } = new();

    public NotificationSchedule AirTickets { get; init; } = new();

    public NotificationSchedule Payment { get; init; } = new();

    public NotificationSchedule DailyReturn { get; init; } = new();

    public NotificationSchedule SpecialReturn { get; init; } = new();

    public NotificationSchedule Receipt { get; init; } = new();

    /// <summary>Returns every schedule paired with its configuration key.</summary>
    public IEnumerable<(string Name, NotificationSchedule Schedule)> GetAll()
    {
        yield return (nameof(DocumentsForDeparture), DocumentsForDeparture);
        yield return (nameof(AirTickets), AirTickets);
        yield return (nameof(Payment), Payment);
        yield return (nameof(DailyReturn), DailyReturn);
        yield return (nameof(SpecialReturn), SpecialReturn);
        yield return (nameof(Receipt), Receipt);
    }
}
