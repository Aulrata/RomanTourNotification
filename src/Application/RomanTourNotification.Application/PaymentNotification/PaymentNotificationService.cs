using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.PaymentNotification;
using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Text;

namespace RomanTourNotification.Application.PaymentNotification;

public class PaymentNotificationService : IPaymentNotificationService
{
    private readonly ILogger<PaymentNotificationService> _logger;
    private readonly ILoadDataService _loadDataService;

    public PaymentNotificationService(
        ILogger<PaymentNotificationService> logger,
        ILoadDataService loadDataService)
    {
        _logger = logger;
        _loadDataService = loadDataService;
    }

    public async Task<string> GetPaymentMessageAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        IEnumerable<IGrouping<string, Request>> managerData = await GetPaymentByDateAsync(dateDto, cancellationToken);

        var manageDataList = managerData.ToList();
        var sb = new StringBuilder();
        string greetings = $"""
                             Доброе утро!
                            Доплата туристов на {dateDto.From.Date:dd.MM.yyyy}.
                            
                            """;

        sb.AppendLine(greetings);
        _logger.LogInformation($"Start formation message");

        foreach (IGrouping<string, Request> requests in manageDataList)
        {
            sb.Append($"\n{requests.Key}\n");
            foreach (Request request in requests)
            {
                string message = $"""

                                  Id: {request.IdSystem}, 
                                  ФИО: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName}, 
                                  ИП: {request.CompanyNameShort}

                                  """;
                sb.Append(message);
            }

            sb.AppendLine();
        }

        if (manageDataList.Count == 0)
            sb.Append("Сегодня нет клиентов, которым надо выставлять счет");

        _logger.LogInformation($"End formation message");

        return sb.ToString();
    }

    private async Task<IEnumerable<IGrouping<string, Request>>> GetPaymentByDateAsync(
        DateDto dateDto,
        CancellationToken cancellationToken)
    {
        IEnumerable<LoadedData> paymentData = await _loadDataService.GetLoadedRequestsAsync(dateDto, cancellationToken);

        var requestsWithClientDebt = new List<Request>();

        _logger.LogInformation($"Payment data for {dateDto.From} has been loaded.");

        foreach (LoadedData loadedData in paymentData)
        {
            var data = loadedData.Requests?.ToList();

            if (data is null || data.Count == 0)
            {
                _logger.LogWarning($"Payment data for {loadedData.Name} {dateDto.From} has been loaded but is empty.");
                continue;
            }

            var tmp = data.Where(r =>
                    r is { ClientDebt: > 0, Status: not RequestStatus.Cancelled, } &&
                    r.DatePaymentDeadline == dateDto.From)
                .ToList();

            requestsWithClientDebt.AddRange(tmp);
        }

        return requestsWithClientDebt.GroupBy(r => r.ManagerFullName);
    }
}