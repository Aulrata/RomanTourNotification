using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.Extensions;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RomanTourNotification.Application.EnrichmentNotification;

public class EnrichmentNotificationService : IEnrichmentNotificationService
{
    private readonly IEnumerable<ApiSettings> _apiSettings;
    private readonly IGatewayService _gatewayService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly List<LoadData> _loadedData;
    private readonly ILogger<EnrichmentNotificationService> _logger;
    private DateTime _lastLoadDate;

    public EnrichmentNotificationService(
        IEnumerable<ApiSettings> apiSettings,
        IGatewayService gatewayService,
        ILogger<EnrichmentNotificationService> logger)
    {
        _apiSettings = apiSettings;
        _gatewayService = gatewayService;
        _logger = logger;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        _loadedData = [];
    }

    public async Task<string> GetArrivalByDateAsync(DateDto dateDto, CancellationToken cancellationToken)
    {
        if (_lastLoadDate != DateTime.Today)
            await LoadAllRequestsAsync(dateDto);

        StringBuilder sb = new();
        sb.AppendLine($"{EnrichmentNotificationMapper.DaysMapper(dateDto.From.DayOfWeek)}\n ");

        foreach (LoadData loadData in _loadedData)
        {
            if (loadData.Requests is null)
                continue;

            var dataRequests = loadData.Requests
                .Where(x => x.Status is not RequestStatus.Cancelled)
                .ToList();

            var dateBeginInSomeDays = GetDateBeginInSomeDays(dataRequests, dateDto).ToList();
            var dateBeginTomorrow = GetBeginTomorrow(dataRequests, dateDto).ToList();
            var dateEndTomorrow = GetEndTomorrow(dataRequests, dateDto).ToList();

            if (dateBeginInSomeDays.Count == 0
                && dateBeginTomorrow.Count == 0
                && dateEndTomorrow.Count == 0)
            {
                continue;
            }

            sb.AppendLine($"{loadData.Name}\n");

            FillDocuments(sb, dateBeginInSomeDays);

            FillTickets(sb, dateBeginTomorrow, dateEndTomorrow);
        }

        if (sb.Length < 15)
            sb.AppendLine("Сегодня ничего нет\n");

        return sb.ToString();
    }

    public IEnumerable<Request> GetEndTomorrow(IEnumerable<Request> requests, DateDto dateDto)
    {
        DateTime tomorrow = dateDto.From.AddDays(1).Date;

        return requests
            .Where(r => r.Services.
                Any(s => s is { InformationServiceType: InformationServiceType.AirTicket } &&
                         s.Flights.Any(f => f.FlightsType == FlightsType.Charter)))
            .Where(r =>
            {
                InformationServices airTicketService = r.Services.
                    First(s => s.InformationServiceType == InformationServiceType.AirTicket);

                IEnumerable<Flights> charterFlights = airTicketService.Flights
                    .Where(f => f.FlightsType == FlightsType.Charter);

                IEnumerable<Flights> flightsWithDates = charterFlights
                    .Where(f => f.DateBeginAsDate is not null)
                    .ToList();

                var flightsWithLaterDates = flightsWithDates
                    .Where(f1 =>
                        flightsWithDates
                            .Any(f2 => f2.DateBeginAsDate?.AddDays(2) < f1.DateBeginAsDate))
                    .ToList();

                if (flightsWithLaterDates.Count == 0) return false;

                Flights? minLaterFlight = flightsWithLaterDates.MinBy(f => f.DateBeginAsDate);

                return minLaterFlight?.DateBeginAsDate == tomorrow;
            });
    }

    public IEnumerable<Request> GetDateBeginInSomeDays(IEnumerable<Request> requests, DateDto dateDto)
    {
        DateTime targetDate = dateDto.From.AddDays(dateDto.Days).Date;

        return requests
            .Where(r =>
                r.DateBeginAsDate == targetDate ||
                (r.DateBeginAsDate < targetDate &&
                r.DateRequestAsDate?.AddDays(1) == dateDto.From))
            .DistinctBy(r => r.IdSystem);
    }

    public IEnumerable<Request> GetBeginTomorrow(IEnumerable<Request> requests, DateDto dateDto)
    {
        DateTime tomorrow = dateDto.From.AddDays(1).Date;

        return requests
            .Where(r => r.DateBeginAsDate == tomorrow &&
                r.Services?
                    .Any(s =>
                        s.InformationServiceType == InformationServiceType.AirTicket &&
                        s.Flights?
                            .Any(f => f.FlightsType == FlightsType.Charter) == true) == true)
            .DistinctBy(r => r.IdSystem);
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
        InformationServices? airTickerService = request.Services.FirstOrDefault(s => s.InformationServiceType == InformationServiceType.AirTicket);
        FlightsType flightType = airTickerService?.Flights?.FirstOrDefault()?.FlightsType ?? FlightsType.Unspecified;

        string type = flightType.GetDescription();
        string tourOperator = WebUtility.HtmlDecode(request.SupplierName);

        return $@"Id: {request.IdSystem}, 
ФИО: {request.ClientLastName} {request.ClientFirstName} {request.ClientMiddleName}, 
Дата вылета: {request.DateBegin}, 
Тип самолета: {type}, 
Почта: {request.ClientEmail}, 
Туроператор: {tourOperator}
";
    }

    private async Task LoadAllRequestsAsync(DateDto dateDto)
    {
        Console.WriteLine("Start loading all requests data");
        _loadedData.Clear();
        foreach (ApiSettings apiSetting in _apiSettings)
        {
            List<Request> data = [];
            var loadData = new LoadData
            {
                Name = apiSetting.Name,
            };

            int page = 1;
            Root? root;

            do
            {
                // TODO Use Stream
                ContextDto context = await _gatewayService.GetArrivalByDateAsync(
                    apiSetting.Api,
                    dateDto.From.AddYears(-1),
                    dateDto.From,
                    page);

                // TODO DeserializeAsync
                root = JsonSerializer.Deserialize<Root>(context.Stream, _jsonSerializerOptions);

                if (root?.Requests is null)
                {
                    _logger.LogWarning("Requests is null");
                    return;
                }

                data.AddRange(root.Requests);

                page++;
            }
            while (page <= root.PagesAll);

            loadData.Requests = data;
            _loadedData.Add(loadData);
        }

        _lastLoadDate = DateTime.Today;
    }
}