using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Messages;
using RomanTourNotification.Application.Contracts.Notifications;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Notifications.Base;
using Telegram.Bot;

namespace RomanTourNotification.Application.Notifications;

/// <summary>Sends "оплата" notifications. Runs Mon–Fri at the main scheduled time.</summary>
public class PaymentGroupNotification
    : TelegramNotificationBase, IScheduledNotification, IForcedNotification
{
    private readonly IGroupService _groupService;
    private readonly IMessageHandlerService _messageHandlerService;
    private readonly TimeSettings _timeSettings;

    /// <summary>Initializes a new instance of the <see cref="PaymentGroupNotification"/> class.</summary>
    public PaymentGroupNotification(
        ILogger<PaymentGroupNotification> logger,
        ITelegramBotClient botClient,
        IGroupService groupService,
        IMessageHandlerService messageHandlerService,
        TimeSettings timeSettings)
        : base(logger, botClient)
    {
        _groupService = groupService;
        _messageHandlerService = messageHandlerService;
        _timeSettings = timeSettings;
    }

    /// <inheritdoc/>
    public GroupType HandledType => GroupType.Payment;

    /// <inheritdoc/>
    public bool ShouldSend(DateTime utcNow, DayOfWeek today)
        => today is not DayOfWeek.Saturday and not DayOfWeek.Sunday
           && utcNow.Hour == _timeSettings.HoursUtc
           && utcNow.Minute == _timeSettings.Minutes;

    /// <inheritdoc/>
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetAllWorksGroupsAsync(cancellationToken))
            .Where(g => g.GroupType == GroupType.Payment)
            .ToList();

        var today = new DateDto(DateTime.Today);
        foreach (Group group in groups)
            await SendForGroupCoreAsync(group, today, cancellationToken);
    }

    /// <inheritdoc/>
    public Task SendForGroupAsync(Group group, CancellationToken cancellationToken)
        => SendForGroupCoreAsync(group, new DateDto(DateTime.Today), cancellationToken);

    private async Task SendForGroupCoreAsync(Group group, DateDto currentDay, CancellationToken cancellationToken)
    {
        string message = await _messageHandlerService.CreatePaymentMessageAsync(
            currentDay, group, cancellationToken);
        await SendMessageAsync(message, group, cancellationToken);
    }
}
