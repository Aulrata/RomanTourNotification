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
        int page = 0,
        string format = "json")
    {
        string url =
            $"{_httpClient.BaseAddress?.OriginalString}/{key}/{ClassUrl}s/{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}/{page}.{format}";

        return await SendRequest(HttpMethod.Get, url);
    }

    private async Task<ContextDto> SendRequest(HttpMethod method, string url)
    {
        using HttpRequestMessage request = new(method, url);

        _logger.LogInformation($"Request to {url}");
        using HttpResponseMessage response = await _httpClient.SendAsync(request);

        _logger.LogInformation($"Response to {url}");
        if (response.StatusCode is not HttpStatusCode.OK)
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");

        string content = await response.Content.ReadAsStringAsync();

        return new ContextDto(content, response.StatusCode);
    }
}