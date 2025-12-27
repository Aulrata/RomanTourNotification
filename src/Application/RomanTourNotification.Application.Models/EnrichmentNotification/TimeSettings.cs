namespace RomanTourNotification.Application.Models.EnrichmentNotification;

public class TimeSettings
{
    public int HoursUtc { get; init; }

    public int Minutes { get; init; }

    public int ReturnHoursUtc { get; init; }

    public int ReturnMinute { get; init; }
}