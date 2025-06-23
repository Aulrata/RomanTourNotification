using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Contracts.EnrichmentNotification;

public interface IFilterEnrichmentNotificationService
{
    public void SetData(DateDto dateDto, IEnumerable<Request> dateRequests);

    public IEnumerable<Request> GetDateBeginInSomeDays();

    public IEnumerable<Request> GetBeginTomorrow();

    public IEnumerable<Request> GetEndTomorrow();
}