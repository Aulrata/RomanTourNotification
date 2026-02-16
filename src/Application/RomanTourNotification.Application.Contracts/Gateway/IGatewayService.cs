using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Contracts.Gateway;

public interface IGatewayService
{
    public Task<ContextDto> GetArrivalByDateAsync(
        string key,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken,
        int page = 0,
        string format = "json");

    public Task<ContextDto> GetAllEmployeeAsync(
        string key,
        CancellationToken cancellationToken,
        string format = "json");

    public Task<IEnumerable<Bill>> GetAllBillsAsync(
        string key,
        CancellationToken cancellationToken,
        string format = "json");

    public Task<ContextDto> GetRequestByIdAsync(
        string key,
        CancellationToken cancellationToken,
        int id,
        string format = "json");
}