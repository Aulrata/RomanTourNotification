using RomanTourNotification.Application.Models.EnrichmentNotification;
using System.Text;

namespace RomanTourNotification.Application.Contracts.EnrichmentNotification;

public interface IEnrichmentNotificationService
{
    public Task GetArrivalByDateAsync(DateDto dateDto, StringBuilder sb, CancellationToken cancellationToken);
}