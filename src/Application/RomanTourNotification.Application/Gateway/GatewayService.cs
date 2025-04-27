using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.Gateway;
using System.Net;

namespace RomanTourNotification.Application.Gateway;

public class GatewayService : IGatewayService
{
    private const string ClassUrl = "request";
    private readonly HttpClient _httpClient;
    private readonly ILogger<GatewayService> _logger;

    public GatewayService(HttpClient httpClient, ILogger<GatewayService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ContextDto> GetArrivalByDateAsync(
        string key,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken,
        int page = 0,
        string format = "json")
    {
        string url =
            $"{_httpClient.BaseAddress?.OriginalString}/{key}/{ClassUrl}s/{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}/{page}.{format}";

        return await SendRequest(HttpMethod.Get, url, cancellationToken);
    }

    private async Task<ContextDto> SendRequest(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, url);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            _logger.LogError($"Request failed with status code {response.StatusCode}");
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        return new ContextDto(content, response.StatusCode);
    }
}