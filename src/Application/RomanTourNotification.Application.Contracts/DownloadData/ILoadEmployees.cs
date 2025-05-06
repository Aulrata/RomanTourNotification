using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Contracts.DownloadData;

public interface ILoadEmployees
{
    public Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken);
}