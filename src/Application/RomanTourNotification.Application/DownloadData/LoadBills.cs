using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.DownloadData;

public class LoadBills : ILoadBills
{
    private readonly ILogger<LoadBills> _logger;
    private readonly IGatewayService _gatewayService;
    private readonly IEnumerable<ApiSettings> _apiSettings;
    private readonly List<Bill> _bills;
    private DateTime _lastUpdate;

    public LoadBills(ILogger<LoadBills> logger, IGatewayService gatewayService, IEnumerable<ApiSettings> apiSettings)
    {
        _logger = logger;
        _gatewayService = gatewayService;
        _apiSettings = apiSettings;
        _bills = [];
    }

    public async Task<IEnumerable<Bill>> GetLoadedBillsAsync(CancellationToken cancellationToken)
    {
        if (_lastUpdate.Date != DateTime.Now.Date)
            await GetBillsAsync(cancellationToken);

        return _bills;
    }

    private async Task GetBillsAsync(CancellationToken cancellationToken)
    {
        _bills.Clear();
        _logger.LogInformation("Starting load bills");
        foreach (ApiSettings apiSettings in _apiSettings)
        {
            IEnumerable<Bill> newBills = await _gatewayService.GetAllBillsAsync(apiSettings.Api, cancellationToken);

            _bills.AddRange(newBills);
        }

        _logger.LogInformation("Finishing load bills");

        _lastUpdate = DateTime.Now;
    }
}