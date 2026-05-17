using RomanTourNotification.Application.Abstractions.Time;

namespace RomanTourNotification.Application.Time;

/// <summary>Production implementation of <see cref="IClock"/> backed by <see cref="DateTime"/>.</summary>
public class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime Today => DateTime.Today;
}
