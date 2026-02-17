using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Models.Gateway;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace RomanTourNotification.Application.Gateway;

public class GatewayService : IGatewayService
{
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
            $"{_httpClient.BaseAddress?.OriginalString}/{key}/requests/{dateFrom:yyyy-MM-dd}/{dateTo:yyyy-MM-dd}/{page}.{format}";

        return await SendRequest(HttpMethod.Get, url, cancellationToken);
    }

    public async Task<ContextDto> GetAllEmployeeAsync(string key, CancellationToken cancellationToken, string format = "json")
    {
        string url = $"{_httpClient.BaseAddress?.OriginalString}/{key}/manager.{format}";

        return await SendRequest(HttpMethod.Get, url, cancellationToken);
    }

    public async Task<IEnumerable<Bill>> GetAllBillsAsync(string key, CancellationToken cancellationToken, string format = "json")
    {
        const int threadCount = 5;
        List<HttpThread> threads = [];

        ConcurrentBag<Bill> bills = [];

        for (int i = 0; i < threadCount; i++)
        {
            threads.Add(new HttpThread(i + 1, i + 1));
        }

        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = threadCount,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(
            threads,
            options,
            async (thread, token) =>
            {
                do
                {
                    string url = $"{_httpClient.BaseAddress?.OriginalString}/{key}/bills/{thread.CurrentPage}.{format}";

                    using HttpRequestMessage request = new(HttpMethod.Get, url);

                    using HttpResponseMessage response = await _httpClient.SendAsync(request, token);

                    string content = await response.Content.ReadAsStringAsync(token);

                    if (response.StatusCode is not HttpStatusCode.OK)
                    {
                        _logger.LogError("Request failed with status code {StatusCode}", response.StatusCode);
                        throw new HttpRequestException($"Request failed with status code {response.StatusCode}. {content}");
                    }

                    RootBill? root = JsonSerializer.Deserialize<RootBill>(content);

                    if (root is null || !root.Bills.Any())
                    {
                        break;
                    }

                    foreach (Bill bill in root.Bills)
                    {
                        bills.Add(bill);
                    }

                    thread.CurrentPage += threadCount;
                }
                while (true);
            });

        return bills;
    }

    public Task<ContextDto> GetRequestByIdAsync(string key, CancellationToken cancellationToken, int id, string format = "json")
    {
        string url = $"{_httpClient.BaseAddress?.OriginalString}/{key}/request/{id}.{format}";

        return SendRequest(HttpMethod.Get, url, cancellationToken);
    }

    private async Task<ContextDto> SendRequest(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, url);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            _logger.LogError("Request failed with status code {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}. {content}");
        }

        return new ContextDto(content, response.StatusCode);
    }
}