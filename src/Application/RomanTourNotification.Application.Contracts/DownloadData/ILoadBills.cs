using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Contracts.DownloadData;

public interface ILoadBills
{
    public Task<IEnumerable<Bill>> GetLoadedBillsAsync(CancellationToken cancellationToken);
}