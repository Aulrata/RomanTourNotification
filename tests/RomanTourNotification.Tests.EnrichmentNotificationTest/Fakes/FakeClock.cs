using RomanTourNotification.Application.Abstractions.Time;

namespace RomanTourNotification.Tests.EnrichmentNotificationTest.Fakes;

/// <summary>Test double for IClock with a fixed, controllable time.</summary>
internal class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;

    public DateTime Today { get; set; } = DateTime.Today;
}
