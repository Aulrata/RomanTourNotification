using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.Gateway;
using System.Net;

namespace RomanTourNotification.Application.Gateway;

public class GatewayService : IGatewayService
{
    private const string ClassUrl = "request";
    private readonly HttpClient _httpClient;

    public GatewayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

        using HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            Console.WriteLine($"Request failed with status code {response.StatusCode}");
            return new ContextDto(string.Empty, response.StatusCode);
        }

        string content = await response.Content.ReadAsStringAsync();

        return new ContextDto(content, response.StatusCode);
    }
}