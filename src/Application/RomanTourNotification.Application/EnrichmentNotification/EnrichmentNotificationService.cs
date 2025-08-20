using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Extensions;
using RomanTourNotification.Application.Models.Gateway;
using System.Net;
using System.Text;

namespace RomanTourNotification.Application.EnrichmentNotification;

public class EnrichmentNotificationService : IEnrichmentNotificationService
{
    private readonly ILogger<EnrichmentNotificationService> _logger;
    private readonly ILoadDataService _loadDataService;

    private readonly IFilterEnrichmentNotificationService _filterEnrichmentNotificationService;

    public EnrichmentNotificationService(
        ILogger<EnrichmentNotificationService> logger,
        ILoadDataService loadDataService,
        IFilterEnrichmentNotificationService filterEnrichmentNotificationService)
    {
        _logger = logger;
        _loadDataService = loadDataService;
        _filterEnrichmentNotificationService = filterEnrichmentNotificationService;
    }

    public async Task GetArrivalByDateAsync(DateDto dateDto, StringBuilder sb, CancellationToken cancellationToken)
    {
        IEnumerable<LoadedData> loadedData = await _loadDataService.GetLoadedRequestsAsync(dateDto, cancellationToken);

        string greetings = $"""
                             Доброе утро!
                            Выписка документов на {dateDto.From.Date:dd.MM.yyyy}.

                            """;
        sb.AppendLine(greetings);

        foreach (LoadedData loadData in loadedData)
        {
            if (loadData.Requests is null)
                continue;

            IEnumerable<IGrouping<string, Request>> groupLists = loadData.Requests
                    .GroupBy(r => r.CompanyNameRus)
                    .Where(g => !string.IsNullOrEmpty(g.Key));

            foreach (IGrouping<string, Request> groupList in groupLists)
            {
                _filterEnrichmentNotificationService.SetData(dateDto, groupList);

                _logger.LogInformation("Start combine notify message");

                var dateBeginInSomeDays = _filterEnrichmentNotificationService.GetDateBeginInSomeDays().ToList();
                var dateBeginTomorrow = _filterEnrichmentNotificationService.GetBeginTomorrow().ToList();
                var dateEndTomorrow = _filterEnrichmentNotificationService.GetEndTomorrow().ToList();

                if (dateBeginInSomeDays.Count == 0
                    && dateBeginTomorrow.Count == 0
                    && dateEndTomorrow.Count == 0)
                {
                    continue;
                }

                // Отключил, т.к. перешли в один юон
                // sb.AppendLine($"{loadData.Name}\n");
                sb.AppendLine($"ИП {groupList.Key.Split(' ')[1]}\n");

                FillDocuments(sb, dateBeginInSomeDays);

                FillTickets(sb, dateBeginTomorrow, dateEndTomorrow);
            }
        }

        if (sb.Length < 15)
            sb.AppendLine("Документов на отправку нет\n");
    }

    private void FillDocuments(StringBuilder sb, List<Request> requests)
    {
        if (requests.Count <= 0) return;

        sb.AppendLine("Документы на вылет: ");

        foreach (Request request in requests)
            sb.AppendLine(GetRequestInformation(request));
    }

    private void FillTickets(StringBuilder sb, List<Request> dateBeginTomorrow, List<Request> dateEndTomorrow)
    {
        if (dateBeginTomorrow.Count <= 0 && dateEndTomorrow.Count <= 0) return;

        sb.AppendLine("Авиабилеты: ");

        FillAirTicketsTomorrowBegin(sb, dateBeginTomorrow);

        FillAirTicketsTomorrowEnd(sb, dateEndTomorrow);
    }

    private void FillAirTicketsTomorrowBegin(StringBuilder sb, List<Request> requests)
    {
        foreach (Request request in requests)
            sb.AppendLine(GetRequestInformation(request));
    }

    private void FillAirTicketsTomorrowEnd(StringBuilder sb, List<Request> requests)
    {
        foreach (Request request in requests)
            sb.AppendLine(GetRequestInformation(request));
    }

    private string GetRequestInformation(Request request)
    {
        InformationServices? airTickerService =
            request.Services.FirstOrDefault(s => s.InformationServiceType == InformationServiceType.AirTicket);
        FlightsType flightType = airTickerService?.Flights.FirstOrDefault()?.FlightsType ?? FlightsType.Unspecified;

        string type = flightType.GetDescription();
        string tourOperator = WebUtility.HtmlDecode(request.SupplierName);

        return $"""
                Id: {request.IdSystem}, 
                ФИО: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName}, 
                Дата вылета: {request.DateBegin}, 
                Тип самолета: {type}, 
                Почта: {request.ClientEmail}, 
                Туроператор: {tourOperator}

                """;
    }
}