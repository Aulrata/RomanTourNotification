using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;

namespace RomanTourNotification.Application.Contracts.DownloadData;

public interface ILoadDataService
{
    public Task<IEnumerable<LoadedData>> GetLoadedRequests(DateDto dateDto, CancellationToken cancellationToken);
}