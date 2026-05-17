using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Abstractions.Time;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.DownloadData.Cache;
using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.DownloadData;

public class LoadDataService : ILoadDataService
{
    private readonly IGatewayService _gatewayService;
    private readonly IEnumerable<ApiSettings> _apiSettings;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<LoadDataService> _logger;
    private readonly LoadDataCache _cache;
    private readonly IClock _clock;

    public LoadDataService(
        IGatewayService gatewayService,
        ILogger<LoadDataService> logger,
        IEnumerable<ApiSettings> apiSettings,
        LoadDataCache cache,
        IClock clock)
    {
        _gatewayService = gatewayService;
        _logger = logger;
        _apiSettings = apiSettings;
        _cache = cache;
        _clock = clock;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };
    }

    public async Task<IEnumerable<LoadedData>> GetLoadedRequestsAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        if (_cache.LastLoaded.AddHours(1) < _clock.UtcNow)
            await RefreshCacheAsync(dateDto, cancellationToken);

        return _cache.Data;
    }

    public async Task<IEnumerable<Request>> GetRequestsByIdsAsync(ConcurrentBag<int> ids, CancellationToken cancellationToken)
    {
        ConcurrentBag<Request> requests = [];

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(
            ids,
            options,
            async (id, token) =>
            {
                ContextDto context = await _gatewayService.GetRequestByIdAsync(_apiSettings.First().Api, token, id);

                RootRequest? rootRequest = JsonSerializer.Deserialize<RootRequest>(context.Stream, _jsonSerializerOptions);

                Request? request = rootRequest?.Request.FirstOrDefault();
                if (request is not null)
                    requests.Add(request);

                await Task.Delay(800, token);
            });

        return requests;
    }

    private async Task RefreshCacheAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start loading all requests data");

        var freshData = new List<LoadedData>();

        foreach (ApiSettings apiSetting in _apiSettings)
        {
            List<Request> data = [];

            var loadData = new LoadedData
            {
                Name = apiSetting.Name,
                Url = apiSetting.Url,
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
            freshData.Add(loadData);
        }

        _cache.Update(freshData, _clock.UtcNow);
    }
}