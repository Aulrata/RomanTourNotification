using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Messages;
using RomanTourNotification.Application.Contracts.NotificationService;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace RomanTourNotification.Application.NotificationService;

public class TelegramService : INotificationService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly IGroupService _groupService;
    private readonly IMessageHandlerService _messageHandlerService;
    private readonly ITelegramBotClient _botClient;
    private readonly IReturnNotificationService _returnNotificationService;

    public TelegramService(
        ILogger<TelegramService> logger,
        IGroupService groupService,
        IMessageHandlerService messageHandlerService,
        ITelegramBotClient botClient,
        IReturnNotificationService returnNotificationService)
    {
        _logger = logger;
        _groupService = groupService;
        _messageHandlerService = messageHandlerService;
        _botClient = botClient;
        _returnNotificationService = returnNotificationService;
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
        var returnGroups = groups.Where(x => x.GroupType == GroupType.Return && string.IsNullOrEmpty(x.ManagerFullname)).ToList();

        await SendArrivalNotificationAsync(arrivalGroups, currentDay, cancellationToken);
        await SendPaymentNotificationAsync(paymentGroups, currentDay, cancellationToken);
        await SendDailyReturnNotificationAsync(returnGroups, cancellationToken);
    }

    public async Task SendForcedNotificationAsync(Group? group, GroupType type, CancellationToken cancellationToken)
    {
        if (group is null)
        {
            _logger.LogWarning("No group found");
            return;
        }

        var groupList = new List<Group> { group };
        var currentDay = new DateDto(DateTime.Today);

        switch (type)
        {
            case GroupType.Arrival:
                await SendArrivalNotificationAsync(groupList, currentDay, cancellationToken);
                break;
            case GroupType.Payment:
                await SendPaymentNotificationAsync(groupList, currentDay, cancellationToken);
                break;
            case GroupType.Return:
                await SendReturnNotificationAsync(groupList, cancellationToken);
                await SendDailyReturnNotificationAsync(groupList, cancellationToken);
                break;
        }
    }

    public async Task SendSpecialNotificationAsync(CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetAllWorksGroupsAsync(cancellationToken)).ToList();

        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found");
            return;
        }

        var returnGroups = groups.Where(x => x.GroupType == GroupType.Return).ToList();

        await SendReturnNotificationAsync(returnGroups, cancellationToken);
    }

    private async Task SendDailyReturnNotificationAsync(
        List<Group> groups,
        CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found for return");
            return;
        }

        foreach (Group group in groups)
        {
            string message = await _returnNotificationService.GetReturnCompleteMessageAsync(cancellationToken);

            try
            {
                await _botClient.SendMessage(
                    group.ChatId,
                    message,
                    cancellationToken: cancellationToken,
                    parseMode: ParseMode.Html);

                _logger.LogInformation("Send daily return message for group: {Title}",  group.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to send daily return notification to group: {Title}. {Message}",
                    group.Title,
                    ex.Message);
            }
        }
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

        foreach (Group group in groups)
        {
            string message = await _messageHandlerService.CreateArrivalMessageAsync(currentDay, group, cancellationToken);

            await SendNotificationAsync(message, group, cancellationToken);
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
            string message =
                await _messageHandlerService.CreatePaymentMessageAsync(currentDay, group, cancellationToken);

            await SendNotificationAsync(message, group, cancellationToken);
        }
    }

    private async Task SendReturnNotificationAsync(
        List<Group> groups,
        CancellationToken cancellationToken)
    {
        if (groups.Count == 0)
        {
            _logger.LogInformation("No groups found for return");
            return;
        }

        foreach (Group group in groups)
        {
            string managerLastName = group.ManagerFullname.Split(' ')[0];
            string message = await _returnNotificationService.GetReturnMessageAsync(managerLastName, cancellationToken);

            await SendNotificationAsync(message, group, cancellationToken);
        }
    }

    private async Task SendNotificationAsync(string message, Group group, CancellationToken cancellationToken)
    {
        try
        {
            if (message.Length > 4096)
            {
                _logger.LogWarning("Message too long: {Length} chars", message.Length);
                message = message.Substring(0, 4093) + "...";
            }

            await _botClient.SendMessage(
                group.ChatId,
                message,
                cancellationToken: cancellationToken,
                parseMode: ParseMode.Html);

            _logger.LogInformation("Send return message for group: {Title}", group.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send arrival notification to group: {Title}. ChatId: {ChatId}. Message: {Message}. InnerException: {InnerException}",
                group.Title,
                group.ChatId,
                ex.Message,
                ex.InnerException?.Message ?? "None");
        }
    }
}