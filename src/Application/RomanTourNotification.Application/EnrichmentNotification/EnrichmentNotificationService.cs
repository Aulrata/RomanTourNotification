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
                .Where(x =>
                    int.TryParse(x.StatusId, out int value) &&
                    value != (int)RequestStatus.Cancelled)
                .ToList();

            List<Request> dateBeginInSomeDays = GetDateBeginInSomeDays(dataRequests, dateDto);
            List<Request> dateBeginTomorrow = GetBeginTomorrow(dataRequests, dateDto);
            List<Request> dateEndTomorrow = GetEndTomorrow(dataRequests, dateDto);

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

    private void FillDocuments(StringBuilder sb, List<Request> requests)
    {
        if (requests.Count <= 0) return;

        sb.AppendLine("Документы на вылет: ");

        foreach (Request date in requests)
        {
            InformationServices? service = date.Services?
                .Where(s => s.InformationServiceType == InformationServiceType.AirTicket).FirstOrDefault();

            string type = (service?.Flights?.FirstOrDefault()?.FlightsType ?? FlightsType.Unspecified).GetDescription();

            sb.AppendLine(
                $"Id: {date.IdSystem}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}," +
                $" \nДата вылета: {date.DateBegin}, \nТип самолета: {type}, \nПочта: {date.ClientEmail}, \nТуроператор: {WebUtility.HtmlDecode(date.SupplierName)}\n");
        }
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
        foreach (Request date in requests)
        {
            InformationServices? service = date.Services?
                .Where(s => s.InformationServiceType == InformationServiceType.AirTicket).FirstOrDefault();

            string type = (service?.Flights?.FirstOrDefault()?.FlightsType ?? FlightsType.Unspecified).GetDescription();

            sb.AppendLine(
                $"Id: {date.IdSystem}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}," +
                $" \nДата вылета: {date.DateBegin}, \nТип самолета: {type}, \nПочта: {date.ClientEmail}, \nТуроператор: {WebUtility.HtmlDecode(date.SupplierName)}\n");
        }
    }

    private void FillAirTicketsTomorrowEnd(StringBuilder sb, List<Request> requests)
    {
        foreach (Request date in requests)
        {
            InformationServices? service = date.Services?
                .Where(s => s.InformationServiceType == InformationServiceType.AirTicket).FirstOrDefault();

            string type = (service?.Flights?.FirstOrDefault()?.FlightsType ?? FlightsType.Unspecified).GetDescription();

            sb.AppendLine(
                $"Id: {date.IdSystem}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}," +
                $" \nДата вылета: {date.DateEnd}, \nТип самолета: {type}, \nПочта: {date.ClientEmail}, \nТуроператор: {WebUtility.HtmlDecode(date.SupplierName)}\n");
        }
    }

    private List<Request> GetEndTomorrow(IEnumerable<Request> requests, DateDto dateDto)
    {
        var results = requests
            .Where(r =>
                r.Services?
                    .Any(s =>
                        s.InformationServiceType == InformationServiceType.AirTicket &&
                        s.Flights?.Any(f => f.FlightsType == FlightsType.Charter) == true &&
                        DateTime.TryParse(
                            s.Flights?
                            .Where(f1 => s.Flights?
                                .Any(f2 =>
                                    f1 != f2 &&
                                    DateTime.TryParse(f1.DateBegin, out DateTime value1) &&
                                    DateTime.TryParse(f2.DateBegin, out DateTime value2) &&
                                    value1 > value2.AddDays(2)) == true)
                            .MinBy(f => DateTime.Parse(f.DateBegin ?? "2000-01-01"))?.DateBegin,
                            out DateTime value) &&
                        value == dateDto.From.AddDays(1)) == true)
            .DistinctBy(r => r.IdSystem)
            .ToList();

        return results;
    }

    private List<Request> GetDateBeginInSomeDays(List<Request> requests, DateDto dateDto)
    {
        IEnumerable<Request> r1 = requests
            .Where(r =>
                !string.IsNullOrEmpty(r.DateBegin) &&
                DateTime.TryParse(r.DateBegin, out DateTime value) &&
                value.Date == dateDto.From.AddDays(dateDto.Days).Date);
        var r2 = r1
            .DistinctBy(r => r.IdSystem)
            .ToList();
        return r2;
    }

    private List<Request> GetBeginTomorrow(List<Request> requests, DateDto dateDto)
    {
        return requests
            .Where(r =>
                !string.IsNullOrEmpty(r.DateBegin) &&
                DateTime.TryParse(r.DateBegin, out DateTime value) &&
                value.Date == dateDto.From.AddDays(1).Date &&
                r.Services?
                    .Any(s =>
                        s.InformationServiceType == InformationServiceType.AirTicket &&
                        s.Flights?
                            .Any(f => f.FlightsType == FlightsType.Charter) == true) == true)
            .DistinctBy(r => r.IdSystem)
            .ToList();
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