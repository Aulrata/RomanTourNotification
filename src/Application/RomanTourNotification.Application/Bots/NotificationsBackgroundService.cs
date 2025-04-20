using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using System.Text;
using Telegram.Bot;

namespace RomanTourNotification.Application.Bots;

public class NotificationsBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IGroupService _groupService;
    private readonly IEnrichmentNotificationService _enrichmentNotificationService;
    private readonly ILogger<NotificationsBackgroundService> _logger;
    private readonly TimeSettings _timeSettings;

    public NotificationsBackgroundService(
        ITelegramBotClient botClient,
        IGroupService groupService,
        IEnrichmentNotificationService enrichmentNotificationService,
        ILogger<NotificationsBackgroundService> logger,
        TimeSettings timeSettings)
    {
        _botClient = botClient;
        _groupService = groupService;
        _enrichmentNotificationService = enrichmentNotificationService;
        _logger = logger;
        _timeSettings = timeSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting background notification service. With UTC time " +
                               $"{_timeSettings.HoursUtc}:{_timeSettings.Minutes}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!(DateTime.UtcNow.Hour == _timeSettings.HoursUtc
                      && DateTime.UtcNow.Minute == _timeSettings.Minutes))
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                // if (DateTime.Today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                // {
                //     await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                //     continue;
                // }
                IEnumerable<Group>? groups = await _groupService.GetAllAsync(stoppingToken);

                if (groups is null)
                {
                    _logger.LogInformation("No groups found");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                await SendArrivalNotification(groups.Where(x => x.GroupType == GroupType.Arrival), stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }

    private async Task SendArrivalNotification(IEnumerable<Group>? groups, CancellationToken cancellationToken)
    {
        if (groups is null)
        {
            _logger.LogInformation("No groups found for arrival");
            return;
        }

        DateTime dateTimeToday = DateTime.Today;

        var stringBuilder = new StringBuilder();

        var currentDay = new DateDto(dateTimeToday);

        stringBuilder.Append(await _enrichmentNotificationService.GetArrivalByDateAsync(currentDay, cancellationToken));

        if (dateTimeToday.DayOfWeek is DayOfWeek.Friday)
        {
            var saturdayDay = new DateDto(dateTimeToday.AddDays(1));
            stringBuilder.Append(
                await _enrichmentNotificationService.GetArrivalByDateAsync(saturdayDay, cancellationToken));

            var sundayDay = new DateDto(dateTimeToday.AddDays(2));
            stringBuilder.Append(
                await _enrichmentNotificationService.GetArrivalByDateAsync(sundayDay, cancellationToken));
        }

        string text = stringBuilder.ToString();

        foreach (Group group in groups)
        {
            await _botClient.SendMessage(
                group.GroupId,
                text,
                cancellationToken: cancellationToken);
        }
    }
}