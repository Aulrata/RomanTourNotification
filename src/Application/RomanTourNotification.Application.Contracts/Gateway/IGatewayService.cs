using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.Contracts.Gateway;

public interface IGatewayService
{
    public Task<ContextDto> GetArrivalByDateAsync(
        string key,
        DateTime dateFrom,
        DateTime dateTo,
        int page = 0,
        string format = "json");
}