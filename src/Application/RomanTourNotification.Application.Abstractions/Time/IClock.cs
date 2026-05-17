namespace RomanTourNotification.Application.Abstractions.Time;

/// <summary>Provides the current date and time, abstracting <see cref="System.DateTime"/> for testability.</summary>
public interface IClock
{
    /// <summary>Gets the current UTC date and time.</summary>
    public DateTime UtcNow { get; }

    /// <summary>Gets today's local date (date only, time is midnight).</summary>
    public DateTime Today { get; }
}
