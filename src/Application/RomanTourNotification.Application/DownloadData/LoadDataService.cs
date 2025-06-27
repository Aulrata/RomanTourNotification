using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.DownloadData;

public class LoadDataService : ILoadDataService
{
    private readonly IGatewayService _gatewayService;
    private readonly IEnumerable<ApiSettings> _apiSettings;
    private readonly JsonSerializerOptions? _jsonSerializerOptions;
    private readonly ILogger<LoadDataService> _logger;
    private readonly List<LoadedData> _loadedData;
    private DateTime _lastLoadData;

    public LoadDataService(
        IGatewayService gatewayService,
        ILogger<LoadDataService> logger,
        IEnumerable<ApiSettings> apiSettings)
    {
        _gatewayService = gatewayService;
        _logger = logger;
        _apiSettings = apiSettings;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new FlightsTypeConverter() },
        };
        _loadedData = [];
    }

    public async Task<IEnumerable<LoadedData>> GetLoadedRequestsAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        if (_lastLoadData != DateTime.Today.Date)
            await GetAllRequestAsync(dateDto, cancellationToken);

        return _loadedData;
    }

    private async Task GetAllRequestAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start loading all requests data");
        _loadedData.Clear();

        foreach (ApiSettings apiSetting in _apiSettings)
        {
            List<Request> data = [];

            var loadData = new LoadedData
            {
                Name = apiSetting.Name,
            };

            int page = 1;
            RootRequests? root;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                // TODO Use Stream
                ContextDto context = await _gatewayService.GetArrivalByDateAsync(
                    apiSetting.Api,
                    dateDto.From.AddYears(-1),
                    dateDto.From,
                    cancellationToken: cancellationToken,
                    page);

                // TODO DeserializeAsync
                root = JsonSerializer.Deserialize<RootRequests>(context.Stream, _jsonSerializerOptions);

                if (root is null)
                    continue;

                if (!root.Requests.Any())
                {
                    _logger.LogWarning("Requests is empty");
                    continue;
                }

                data.AddRange(root.Requests);

                page++;
            }
            while (page <= root?.PagesAll);

            loadData.Requests = data;
            _loadedData.Add(loadData);
        }

        _lastLoadData = DateTime.Today;
    }
}