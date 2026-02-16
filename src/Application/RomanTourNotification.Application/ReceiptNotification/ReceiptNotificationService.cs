using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.ReceiptNotification;
using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace RomanTourNotification.Application.ReceiptNotification;

public class ReceiptNotificationService : IReceiptNotificationService
{
    private readonly ILogger<ReceiptNotificationService> _logger;
    private readonly ILoadBills _loadBills;
    private readonly ILoadDataService _loadDataService;
    private string _currentUrl = string.Empty;

    public ReceiptNotificationService(
        ILoadBills loadBills,
        ILogger<ReceiptNotificationService> logger,
        ILoadDataService loadDataService)
    {
        _loadBills = loadBills;
        _logger = logger;
        _loadDataService = loadDataService;
    }

    public async Task<string> GetReceiptMessageAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        await GetClientReceiptMessageAsync(dateDto, builder, cancellationToken);
        builder.AppendLine();
        await GetCloseReceiptMessageAsync(dateDto, builder, cancellationToken);

        if (builder.Length == 2)
        {
            builder.Clear();
            builder.AppendLine("Нет чеков");
        }

        return builder.ToString();
    }

    private async Task GetCloseReceiptMessageAsync(DateDto dateDto, StringBuilder builder, CancellationToken cancellationToken)
    {
        IEnumerable<LoadedData> loadedDataList = await _loadDataService.GetLoadedRequestsAsync(dateDto, cancellationToken);
        _logger.LogInformation("Start creating close receipt message");
        foreach (LoadedData loadedData in loadedDataList)
        {
            IEnumerable<Request>? requests = loadedData.Requests;
            _currentUrl = loadedData.Url;
            if (requests is null)
                continue;

            IEnumerable<Request> filteredRequests = requests
                .Where(r => r is not { Status:
                                                    RequestStatus.ApplicationClosed or
                                                    RequestStatus.Cancelled or
                                                    RequestStatus.Test }

                                && r.DateBeginAsDate < DateTime.Today.Date)
                .ToList();

            if (!filteredRequests.Any())
                continue;

            builder.Append("<u>Нет закрывающих чеков</u>\n");

            foreach (Request request in filteredRequests)
            {
                string message = $"""
                                 
                                 Турист: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName} 
                                 Заявка: <a href="{_currentUrl}{request.IdSystem}">{request.IdSystem}</a>  
                                 ИП: {request.CompanyNameShort}
                                 
                                 """;
                builder.Append(message);
            }
        }

        _logger.LogInformation("Stop creating close receipt message");
    }

    private async Task GetClientReceiptMessageAsync(DateDto dateDto, StringBuilder builder, CancellationToken cancellationToken)
    {
        IEnumerable<Bill> bills = (await _loadBills.GetLoadedBillsAsync(cancellationToken)).ToList();
        IEnumerable<LoadedData> loadedDataList = await _loadDataService.GetLoadedRequestsAsync(dateDto, cancellationToken);
        List<Request> filteredRequests = [];

        _logger.LogInformation("Start creating client receipt message");
        foreach (LoadedData loadedData in loadedDataList)
        {
            IEnumerable<Request>? requests = loadedData.Requests;
            _currentUrl = loadedData.Url;

            if (requests is null)
                continue;

            var ids = new ConcurrentBag<int>(
                requests
                    .Where(r => r.Status is not RequestStatus.Cancelled &&
                                r.Status is not RequestStatus.Test &&
                                r.ClientDebt > 0)
                    .Select(r => r.Id));

            if (ids.IsEmpty)
                continue;

            IEnumerable<Request> fullRequestList = await _loadDataService.GetRequestsByIdsAsync(ids, cancellationToken);

            foreach (Request request in fullRequestList)
            {
                IEnumerable<Bill> billsByRequest = bills
                    .Where(b => b.RequestId == request.Id &&
                                b.GetDate.Date != DateTime.Today.Date)
                    .Select(b =>
                    {
                        b.Price = b.Price.Replace('.', ',');
                        return b;
                    });

                decimal paymentSum = request.Payments
                    .Where(p => p.PaymentType == PaymentType.Client)
                    .Sum(p => p.Price);

                decimal billSum = billsByRequest.Sum(b => b.GetPrice);

                request.PaymentDebt = billSum - paymentSum;
                if (request.PaymentDebt <= 0)
                    continue;

                filteredRequests.Add(request);
            }
        }

        if (filteredRequests.Count == 0)
            return;

        builder.Append("<u>Нет чека клиентам</u>\n");

        var ruCulture = new CultureInfo("ru-RU");
        foreach (Request request in filteredRequests)
        {
            string message = $"""

                             Турист: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName}
                             Сумма: {request.PaymentDebt.ToString("C", ruCulture)}
                             Заявка: <a href="{_currentUrl}{request.IdSystem}">{request.IdSystem}</a>  
                             ИП: {request.CompanyNameShort}
                             
                             """;

            builder.Append(message);
        }

        _logger.LogInformation("Stop creating client receipt message");
    }
}