using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Models.DownloadData;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Extensions;
using RomanTourNotification.Application.Models.Gateway;
using RomanTourNotification.Domain.ValueObjects;
using System.Net;
using System.Text;

namespace RomanTourNotification.Application.EnrichmentNotification;

public class EnrichmentNotificationService : IEnrichmentNotificationService
{
    private readonly ILoadDataService _loadDataService;
    private readonly IFilterEnrichmentNotificationService _filterService;

    public EnrichmentNotificationService(
        ILoadDataService loadDataService,
        IFilterEnrichmentNotificationService filterService)
    {
        _loadDataService = loadDataService;
        _filterService = filterService;
    }

    /// <inheritdoc/>
    public async Task GetDocumentsForDepartureAsync(
        DateDto dateDto,
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        IEnumerable<LoadedData> loadedData =
            await _loadDataService.GetLoadedRequestsAsync(dateDto, cancellationToken);

        sb.AppendLine($"""
                        Доброе утро!
                       <b><u>Документы на вылет на {dateDto.From.Date:dd.MM.yyyy}</u></b>.

                       """);

        bool hasDocs = false;

        foreach (LoadedData loadData in loadedData)
        {
            if (loadData.Requests is null)
                continue;

            foreach (IGrouping<string, Request> groupList in GroupByCompany(loadData.Requests))
            {
                var docs = _filterService
                    .GetDateBeginInSomeDays(dateDto, groupList, managerFullname)
                    .ToList();

                if (docs.Count == 0)
                    continue;

                sb.AppendLine($"ИП {new CompanyName(groupList.Key).ShortName}\n");
                hasDocs = true;

                sb.AppendLine("Документы на вылет: ");
                foreach (Request request in docs)
                    sb.AppendLine(FormatRequest(request, loadData.Url));
            }
        }

        if (!hasDocs)
            sb.AppendLine("Сегодня нет документов для отправки клиентам.\n");
    }

    /// <inheritdoc/>
    public async Task GetAirTicketsAsync(
        DateDto dateDto,
        StringBuilder sb,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        IEnumerable<LoadedData> loadedData =
            await _loadDataService.GetLoadedRequestsAsync(dateDto, cancellationToken);

        sb.AppendLine($"""
                        Доброе утро!
                       <b><u>Авиабилеты на {dateDto.From.Date:dd.MM.yyyy}</u></b>.

                       """);

        bool hasTickets = false;

        foreach (LoadedData loadData in loadedData)
        {
            if (loadData.Requests is null)
                continue;

            foreach (IGrouping<string, Request> groupList in GroupByCompany(loadData.Requests))
            {
                var beginTomorrow = _filterService
                    .GetBeginTomorrow(dateDto, groupList, managerFullname)
                    .ToList();

                var endTomorrow = _filterService
                    .GetEndTomorrow(dateDto, groupList, managerFullname)
                    .ToList();

                if (beginTomorrow.Count == 0 && endTomorrow.Count == 0)
                    continue;

                sb.AppendLine($"ИП {new CompanyName(groupList.Key).ShortName}\n");
                hasTickets = true;

                sb.AppendLine("Авиабилеты: ");
                foreach (Request request in beginTomorrow)
                    sb.AppendLine(FormatRequest(request, loadData.Url));
                foreach (Request request in endTomorrow)
                    sb.AppendLine(FormatRequest(request, loadData.Url));
            }
        }

        if (!hasTickets)
            sb.AppendLine("Сегодня нет авиабилетов для отправки.\n");
    }

    private static IEnumerable<IGrouping<string, Request>> GroupByCompany(IEnumerable<Request> requests)
    {
        return requests
            .GroupBy(r => r.CompanyNameRus)
            .Where(g => !string.IsNullOrEmpty(g.Key));
    }

    private static string FormatRequest(Request request, string baseUrl)
    {
        InformationServices? airTicketService =
            request.Services.FirstOrDefault(s => s.InformationServiceType == InformationServiceType.AirTicket);
        FlightsType flightType =
            airTicketService?.Flights.FirstOrDefault()?.FlightsType ?? FlightsType.Unspecified;

        string type = flightType.GetDescription();
        string tourOperator = WebUtility.HtmlDecode(request.SupplierName);

        return $"""
                Id: <a href="{baseUrl}{request.IdSystem}">{request.IdSystem}</a>, 
                ФИО: {request.ClientSurname} {request.ClientFirstName} {request.ClientMiddleName}, 
                Дата вылета: <b>{request.DateBegin}</b>., 
                Тип самолета: {type}, 
                Почта: {request.ClientEmail}, 
                Туроператор: {tourOperator}

                """;
    }
}
