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
}
