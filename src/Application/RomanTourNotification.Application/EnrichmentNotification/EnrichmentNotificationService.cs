using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Gateway;
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
            await LoadDataAsync(dateDto);

        StringBuilder sb = new();
        sb.AppendLine($"{EnrichmentNotificationMapper.DaysMapper(dateDto.From.DayOfWeek)}\n");

        foreach (LoadData loadData in _loadedData)
        {
            if (loadData.Requests is null)
                continue;

            var dateBeginInSomeDays = loadData.Requests.Where(x =>
                    !string.IsNullOrEmpty(x.DateBegin)
                    && DateTime.Parse(x.DateBegin).Date == dateDto.From.AddDays(dateDto.Days))
                .ToList();

            var dateBeginTomorrow = loadData.Requests.Where(x =>
                    !string.IsNullOrEmpty(x.DateBegin)
                    && DateTime.Parse(x.DateBegin).Date ==
                    dateDto.From.AddDays(1).Date)
                .ToList();

            var dateEndTomorrow = loadData.Requests.Where(x =>
                    !string.IsNullOrEmpty(x.DateEnd)
                    && DateTime.Parse(x.DateEnd).Date ==
                    dateDto.From.AddDays(1))
                .ToList();

            dateBeginInSomeDays = dateBeginInSomeDays.DistinctBy(x => x.Id).ToList();
            dateBeginTomorrow = dateBeginTomorrow.DistinctBy(x => x.Id).ToList();
            dateEndTomorrow = dateEndTomorrow.DistinctBy(x => x.Id).ToList();

            if (dateBeginInSomeDays.Count == 0
                && dateBeginTomorrow.Count == 0
                && dateEndTomorrow.Count == 0)
            {
                continue;
            }

            sb.AppendLine($"{loadData.Name}\n");

            if (dateBeginInSomeDays.Count > 0)
            {
                sb.AppendLine("Документы на вылет: ");

                foreach (Request date in dateBeginInSomeDays)
                {
                    sb.AppendLine(
                        $"Id: {date.Id}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}," +
                        $" \nДата вылета: {date.DateBegin}, \nПочта: {date.ClientEmail}, \nТуроператор: {WebUtility.HtmlDecode(date.SupplierName)}\n");
                }
            }

            if (dateBeginTomorrow.Count > 0 || dateEndTomorrow.Count > 0)
            {
                sb.AppendLine("Авиабилеты: ");

                foreach (Request date in dateBeginTomorrow)
                {
                    sb.AppendLine(
                        $"Id: {date.Id}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}," +
                        $" \nДата вылета: {date.DateBegin}, \nПочта: {date.ClientEmail}, \nТуроператор: {WebUtility.HtmlDecode(date.SupplierName)}\n");
                }

                foreach (Request date in dateEndTomorrow)
                {
                    sb.AppendLine(
                        $"Id: {date.Id}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}," +
                        $" \nДата вылета: {date.DateEnd}, \nПочта: {date.ClientEmail}, \nТуроператор: {WebUtility.HtmlDecode(date.SupplierName)}\n");
                }
            }
        }

        if (sb.Length < 15)
            sb.AppendLine("Сегодня ничего нет\n");

        return sb.ToString();
    }

    private async Task LoadDataAsync(DateDto dateDto)
    {
        Console.WriteLine("Start loading data");
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

                _logger.LogInformation("Start deserialize");

                // TODO DeserializeAsync
                root = JsonSerializer.Deserialize<Root>(
                    context.Stream,
                    _jsonSerializerOptions);

                _logger.LogInformation("End deserialize");

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