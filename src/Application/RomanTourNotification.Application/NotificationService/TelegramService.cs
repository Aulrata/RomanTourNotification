using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Messages;
using RomanTourNotification.Application.Contracts.NotificationService;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;

namespace RomanTourNotification.Application.NotificationService;

public class TelegramService : INotificationService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IGroupService _groupService;
    private readonly IMessageHandlerService _messageHandlerService;
    private readonly ITelegramBotClient _botClient;

    public TelegramService(
        ILogger<TelegramService> logger,
        IGroupService groupService,
        IMessageHandlerService messageHandlerService,
        ITelegramBotClient botClient)
    {
        _logger = logger;
        _groupService = groupService;
        _messageHandlerService = messageHandlerService;
        _botClient = botClient;
    }

    public async Task SendNotificationAsync(CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetAllWorksGroupsAsync(cancellationToken)).ToList();

        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found");
            return;
        }

        var currentDay = new DateDto(DateTime.Today);

        var arrivalGroups = groups.Where(x => x.GroupType == GroupType.Arrival).ToList();
        var paymentGroups = groups.Where(x => x.GroupType == GroupType.Payment).ToList();

        await SendArrivalNotificationAsync(arrivalGroups, currentDay, cancellationToken);

        await SendPaymentNotificationAsync(paymentGroups, currentDay, cancellationToken);
    }

    private async Task SendArrivalNotificationAsync(
        List<Group> groups,
        DateDto currentDay,
        CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found for arrival");
            return;
        }

        string message = await _messageHandlerService.CreateArrivalMessageAsync(currentDay, cancellationToken);

        foreach (Group group in groups)
        {
            await _botClient.SendMessage(
                group.ChatId,
                message,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SendPaymentNotificationAsync(
        List<Group> groups,
        DateDto currentDay,
        CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found for payment");
            return;
        }

        foreach (Group group in groups)
        {
            string message = await _messageHandlerService.CreatePaymentMessageAsync(currentDay, group, cancellationToken);

            await _botClient.SendMessage(group.ChatId, message, cancellationToken: cancellationToken);
        }
    }
}