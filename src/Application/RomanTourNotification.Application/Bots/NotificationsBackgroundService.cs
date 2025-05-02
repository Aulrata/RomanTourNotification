using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.PaymentNotification;
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
    private readonly IPaymentNotificationService _paymentNotificationService;
    private readonly ILogger<NotificationsBackgroundService> _logger;
    private readonly TimeSettings _timeSettings;

    public NotificationsBackgroundService(
        ITelegramBotClient botClient,
        IGroupService groupService,
        IEnrichmentNotificationService enrichmentNotificationService,
        ILogger<NotificationsBackgroundService> logger,
        TimeSettings timeSettings,
        IPaymentNotificationService paymentNotificationService)
    {
        _botClient = botClient;
        _groupService = groupService;
        _enrichmentNotificationService = enrichmentNotificationService;
        _logger = logger;
        _timeSettings = timeSettings;
        _paymentNotificationService = paymentNotificationService;
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

                if (DateTime.Today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                IEnumerable<Group> groups = (await _groupService.GetAllWorksGroupsAsync(stoppingToken)).ToList();

                if (!groups.Any())
                {
                    _logger.LogInformation("No groups found");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                await SendNotificationAsync(groups, stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task SendNotificationAsync(IEnumerable<Group> groups, CancellationToken cancellationToken)
    {
        DateTime dateTimeToday = DateTime.Today.AddDays(4);
        var currentDay = new DateDto(dateTimeToday);

        groups = groups.ToList();

        var arrivalGroups = groups.Where(x => x.GroupType == GroupType.Arrival).ToList();
        var paymentGroups = groups.Where(x => x.GroupType == GroupType.Payment).ToList();

        await SendArrivalNotificationAsync(arrivalGroups, currentDay, cancellationToken);

        await SendPaymentNotificationAsync(paymentGroups, currentDay, cancellationToken);
    }

    private async Task SendArrivalNotificationAsync(List<Group> groups, DateDto currentDay, CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found for arrival");
            return;
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append(await _enrichmentNotificationService.GetArrivalByDateAsync(currentDay, cancellationToken));

        if (currentDay.From.DayOfWeek is DayOfWeek.Friday)
        {
            var saturdayDay = new DateDto(currentDay.From.AddDays(1));
            stringBuilder.Append(
                await _enrichmentNotificationService.GetArrivalByDateAsync(saturdayDay, cancellationToken));

            var sundayDay = new DateDto(currentDay.From.AddDays(2));
            stringBuilder.Append(
                await _enrichmentNotificationService.GetArrivalByDateAsync(sundayDay, cancellationToken));
        }

        string text = stringBuilder.ToString();

        foreach (Group group in groups)
        {
            await _botClient.SendMessage(
                group.ChatId,
                text,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SendPaymentNotificationAsync(List<Group> groups, DateDto currentDay, CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found for payment");
            return;
        }

        foreach (Group group in groups)
        {
            var sb = new StringBuilder();
            string greetings = $"""
                                 Доброе утро!
                                Доплата туристов на {currentDay.From.Date:dd.MM.yyyy}.

                                """;

            sb.AppendLine(greetings);

            if (!string.IsNullOrEmpty(group.ManagerFullname))
                await _paymentNotificationService.GetPaymentMessageByManagerAsync(currentDay, sb, group.ManagerFullname, cancellationToken);
            else
                await _paymentNotificationService.GetAllPaymentMessagesAsync(currentDay, sb, cancellationToken);

            // if (currentDay.From.DayOfWeek is DayOfWeek.Friday)
            await GetWeekendPaymentMessagesAsync(sb, currentDay, group.ManagerFullname, cancellationToken);

            string message = sb.ToString();

            await _botClient.SendMessage(group.ChatId, message, cancellationToken: cancellationToken);
        }
    }

    private async Task GetWeekendPaymentMessagesAsync(
        StringBuilder sb,
        DateDto currentDay,
        string managerFullname,
        CancellationToken cancellationToken)
    {
        bool hasManager = !string.IsNullOrEmpty(managerFullname);

        var saturdayDay = new DateDto(currentDay.From.AddDays(1));
        string saturdayGreetings = $"""

                                    Доплата туристов на {saturdayDay.From.Date:dd.MM.yyyy}.

                                    
                                    """;
        sb.Append(saturdayGreetings);

        if (hasManager)
            await _paymentNotificationService.GetPaymentMessageByManagerAsync(saturdayDay, sb, managerFullname, cancellationToken);
        else
            await _paymentNotificationService.GetAllPaymentMessagesAsync(saturdayDay, sb, cancellationToken);

        var sundayDay = new DateDto(currentDay.From.AddDays(2));
        string sundayGreetings = $"""
                                  
                                  Доплата туристов на {sundayDay.From.Date:dd.MM.yyyy}.

                                  
                                  """;
        sb.Append(sundayGreetings);

        if (hasManager)
            await _paymentNotificationService.GetPaymentMessageByManagerAsync(sundayDay, sb, managerFullname, cancellationToken);
        else
            await _paymentNotificationService.GetAllPaymentMessagesAsync(sundayDay, sb, cancellationToken);
    }
}