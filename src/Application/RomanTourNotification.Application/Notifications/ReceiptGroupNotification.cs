using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Notifications;
using RomanTourNotification.Application.Contracts.ReceiptNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Notifications.Base;
using Telegram.Bot;

namespace RomanTourNotification.Application.Notifications;

/// <summary>Sends "чеки" notifications. Runs Mon–Fri at the receipt scheduled time.</summary>
public class ReceiptGroupNotification
    : TelegramNotificationBase, IScheduledNotification, IForcedNotification
{
    private readonly IGroupService _groupService;
    private readonly IReceiptNotificationService _receiptNotificationService;
    private readonly TimeSettings _timeSettings;

    /// <summary>Initializes a new instance of the <see cref="ReceiptGroupNotification"/> class.</summary>
    public ReceiptGroupNotification(
        ILogger<ReceiptGroupNotification> logger,
        ITelegramBotClient botClient,
        IGroupService groupService,
        IReceiptNotificationService receiptNotificationService,
        TimeSettings timeSettings)
        : base(logger, botClient)
    {
        _groupService = groupService;
        _receiptNotificationService = receiptNotificationService;
        _timeSettings = timeSettings;
    }

    /// <inheritdoc/>
    public GroupType HandledType => GroupType.Receipt;

    /// <inheritdoc/>
    public bool ShouldSend(DateTime utcNow, DayOfWeek today)
        => today is not DayOfWeek.Saturday and not DayOfWeek.Sunday
           && utcNow.Hour == _timeSettings.ReceiptHoursUtc
           && utcNow.Minute == _timeSettings.ReceiptMinutes;

    /// <inheritdoc/>
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetAllWorksGroupsAsync(cancellationToken))
            .Where(g => g.GroupType == GroupType.Receipt)
            .ToList();

        var today = new DateDto(DateTime.Today);
        string message = await _receiptNotificationService.GetReceiptMessageAsync(today, cancellationToken);

        foreach (Group group in groups)
            await SendMessageAsync(message, group, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendForGroupAsync(Group group, CancellationToken cancellationToken)
    {
        string message = await _receiptNotificationService.GetReceiptMessageAsync(
            new DateDto(DateTime.Today), cancellationToken);
        await SendMessageAsync(message, group, cancellationToken);
    }
}
