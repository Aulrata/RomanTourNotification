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
    private DateTime _lastLoadDate;

    public EnrichmentNotificationService(IEnumerable<ApiSettings> apiSettings, IGatewayService gatewayService)
    {
        _apiSettings = apiSettings;
        _gatewayService = gatewayService;
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
        sb.Append($"{EnrichmentNotificationMapper.DaysMapper(dateDto.From.DayOfWeek)}\n");

        foreach (LoadData loadData in _loadedData)
        {
            if (loadData.Requests is null)
                continue;

            var dateBeginInSomeDays = loadData.Requests.Where(x =>
                !string.IsNullOrEmpty(x.DateBegin)
                && DateTime.Parse(x.DateBegin).Date == dateDto.From.AddDays(dateDto.Days)).ToList();

            var dateBeginTomorrow = loadData.Requests.Where(x =>
                !string.IsNullOrEmpty(x.DateBegin)
                && DateTime.Parse(x.DateBegin).Date ==
                dateDto.From.AddDays(1).Date).ToList();

            var dateEndTomorrow = loadData.Requests.Where(x =>
                !string.IsNullOrEmpty(x.DateEnd)
                && DateTime.Parse(x.DateEnd).Date ==
                dateDto.From.AddDays(1)).ToList();

            sb.Append($"\n{loadData.Name}\n");

            dateBeginInSomeDays = dateBeginInSomeDays.DistinctBy(x => x.Id).ToList();
            dateBeginTomorrow = dateBeginTomorrow.DistinctBy(x => x.Id).ToList();
            dateEndTomorrow = dateEndTomorrow.DistinctBy(x => x.Id).ToList();

            if (dateBeginInSomeDays.Count == 0
                && dateBeginTomorrow.Count == 0
                && dateEndTomorrow.Count == 0)
            {
                sb.AppendLine("Сегодня все спокойно. Приятного дня");
            }

            if (dateBeginInSomeDays.Count > 0)
            {
                sb.AppendLine("Отправление через 3 дня: ");

                foreach (Request date in dateBeginInSomeDays)
                {
                    sb.AppendLine(
                        $"Id: {date.Id}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}, \nДата вылета: {date.DateBegin}, \nПочта: {date.ClientEmail}\n");
                }
            }

            if (dateBeginTomorrow.Count > 0)
            {
                sb.AppendLine("Отправление завтра: ");

                foreach (Request date in dateBeginTomorrow)
                {
                    sb.AppendLine(
                        $"Id: {date.Id}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}, \nДата вылета: {date.DateBegin}, \nПочта: {date.ClientEmail}\n");
                }
            }

            if (dateEndTomorrow.Count > 0)
            {
                sb.AppendLine("Прибытие завтра: ");

                foreach (Request date in dateEndTomorrow)
                {
                    sb.AppendLine(
                        $"Id: {date.Id}, \nФИО: {date.ClientLastName} {date.ClientFirstName} {date.ClientMiddleName}, \nДата вылета: {date.DateEnd}, \nПочта: {date.ClientEmail}\n");
                }
            }
        }

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
                ContextDto context = await _gatewayService.GetArrivalByDateAsync(
                    apiSetting.Api,
                    dateDto.From.AddMonths(-12),
                    dateDto.From,
                    page);

                if (context.StatusCode is not HttpStatusCode.OK)
                {
                    // TODO dto
                    Console.WriteLine($"Status Code: {context.StatusCode}");
                    return;
                }

                // TODO DeserializeAsync
                root = JsonSerializer.Deserialize<Root>(
                    context.Stream,
                    _jsonSerializerOptions);

                if (root?.Requests is null)
                {
                    Console.WriteLine("Requests is null");
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