using RomanTourNotification.Application.Models.EnrichmentNotification;
using System.Text;

namespace RomanTourNotification.Application.Contracts.EnrichmentNotification;

/// <summary>Builds enrichment notification messages for departure-related events.</summary>
public interface IEnrichmentNotificationService
{
    /// <summary>
    /// Appends departure document rows (dateBeginInSomeDays) to <paramref name="sb"/>.
    /// </summary>
    public Task GetDocumentsForDepartureAsync(
        DateDto dateDto,
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken);

    /// <summary>
    /// Appends air-ticket rows (dateBeginTomorrow + dateEndTomorrow) to <paramref name="sb"/>.
    /// </summary>
    public Task GetAirTicketsAsync(
        DateDto dateDto,
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken);
}
