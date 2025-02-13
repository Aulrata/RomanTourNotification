using RomanTourNotification.Application.Models.EnrichmentNotification;

namespace RomanTourNotification.Application.Contracts.EnrichmentNotification;

public interface IEnrichmentNotificationService
{
    public Task<string> GetArrivalByDateAsync(DateDto dateDto, CancellationToken cancellationToken);
}