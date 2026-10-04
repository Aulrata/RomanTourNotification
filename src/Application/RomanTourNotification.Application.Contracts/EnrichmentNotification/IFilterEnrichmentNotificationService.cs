using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Contracts.EnrichmentNotification;

/// <summary>Filters requests into the three date-window buckets used for enrichment notifications.</summary>
public interface IFilterEnrichmentNotificationService
{
    /// <summary>
    /// Returns requests whose departure falls within the configured "some days ahead" window.
    /// </summary>
    public IEnumerable<Request> GetDateBeginInSomeDays(
        DateDto dateDto,
        IEnumerable<Request> requests,
        string managerFullname);

    /// <summary>Returns requests whose outbound flight departs tomorrow.</summary>
    public IEnumerable<Request> GetBeginTomorrow(
        DateDto dateDto,
        IEnumerable<Request> requests,
        string managerFullname);

    /// <summary>Returns requests whose return flight departs tomorrow.</summary>
    public IEnumerable<Request> GetEndTomorrow(
        DateDto dateDto,
        IEnumerable<Request> requests,
        string managerFullname);
}
