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
    private IEnumerable<IGrouping<string, Request>> _groupings;
    private DateTime _lastUpdateDate;

    public PaymentNotificationService(
        ILogger<PaymentNotificationService> logger,
        ILoadDataService loadDataService)
    {
        _logger = logger;
        _loadDataService = loadDataService;
        _groupings = [];
    }

    public async Task GetPaymentMessageAsync(
        DateDto currentDay,
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        if (_lastUpdateDate != currentDay.From.Date)
            await LoadPaymentDataAsync(currentDay, cancellationToken);

        if (!string.IsNullOrEmpty(managerFullname))
            GetPaymentMessageByManagerAsync(sb, managerFullname, cancellationToken);
        else
            GetAllPaymentMessagesAsync(sb, cancellationToken);
    }

    private void GetAllPaymentMessagesAsync(StringBuilder sb, CancellationToken cancellationToken)
    {
        var managerData = _groupings.ToList();

        _logger.LogInformation("The formation of a message for all payments has begun.");

        if (managerData.Count == 0)
        {
            sb.Append("Сегодня нет клиентов, которым надо выставлять счет\n");
            _logger.LogInformation("There are no clients to invoice today.");
            return;
        }

        foreach (IGrouping<string, Request> requests in managerData)
        {
            sb.Append($"\n{requests.Key}\n");
            foreach (Request request in requests)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                string message = $"""

                                  Id: {request.IdSystem}, 
                                  ФИО: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName}, 
                                  ИП: {request.CompanyNameShort}

                                  """;
                sb.Append(message);
            }
        }

        _logger.LogInformation("Generation of messages for all payments has been completed.");
    }

    private void GetPaymentMessageByManagerAsync(
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        var managerData = _groupings.Where(g => g.Key == managerFullname).ToList();

        _logger.LogInformation($"The formation of the message on payments of the manager has begun: {managerFullname}.");

        if (managerData.Count == 0)
        {
            sb.Append("\nСегодня нет клиентов, которым надо выставлять счет\n");
            _logger.LogInformation($"There are no clients to invoice today for: {managerFullname}.");
            return;
        }

        IGrouping<string, Request> requests = managerData.First();

        foreach (Request request in requests)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            string message = $"""

                              Id: {request.IdSystem}, 
                              ФИО: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName}, 
                              ИП: {request.CompanyNameShort}

                              """;
            sb.Append(message);
        }

        _logger.LogInformation($"The formation of the message on payments of the manager has been completed: {managerFullname}.");
    }

    private async Task LoadPaymentDataAsync(DateDto dateDto, CancellationToken cancellationToken)
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

        _groupings = requestsWithClientDebt.GroupBy(r => r.ManagerFullName);
        _lastUpdateDate = DateTime.Today.Date;
    }
}