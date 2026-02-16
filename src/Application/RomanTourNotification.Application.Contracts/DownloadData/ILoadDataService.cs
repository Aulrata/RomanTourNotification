using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Collections.Concurrent;

namespace RomanTourNotification.Application.Contracts.DownloadData;

public interface ILoadDataService
{
    public Task<IEnumerable<LoadedData>> GetLoadedRequestsAsync(DateDto dateDto, CancellationToken cancellationToken);

    public Task<IEnumerable<Request>> GetRequestsByIdsAsync(ConcurrentBag<int> ids, CancellationToken cancellationToken);
}