using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Notifications;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Models.Notifications;
using RomanTourNotification.Application.Notifications.Base;
using RomanTourNotification.Domain.ValueObjects;
using Telegram.Bot;

namespace RomanTourNotification.Application.Notifications;

/// <summary>
/// Sends the daily return summary to groups that have no assigned manager (schedule: <see cref="ScheduleSettings.DailyReturn"/>).
/// Also handles forced sends from the bot, in which case it sends both the manager-specific
/// return message and the complete daily summary for the given group.
/// </summary>
public class DailyReturnNotification
    : TelegramNotificationBase, IScheduledNotification, IForcedNotification
{
    private readonly IGroupService _groupService;
    private readonly IReturnNotificationService _returnNotificationService;
    private readonly ScheduleSettings _schedules;

    /// <summary>Initializes a new instance of the <see cref="DailyReturnNotification"/> class.</summary>
    public DailyReturnNotification(
        ILogger<DailyReturnNotification> logger,
        ITelegramBotClient botClient,
        IGroupService groupService,
        IReturnNotificationService returnNotificationService,
        ScheduleSettings schedules)
        : base(logger, botClient)
    {
        _groupService = groupService;
        _returnNotificationService = returnNotificationService;
        _schedules = schedules;
    }

    /// <inheritdoc/>
    public GroupType HandledType => GroupType.Return;

    /// <inheritdoc/>
    public bool ShouldSend(DateTime utcNow, DayOfWeek today)
        => _schedules.DailyReturn.IsDue(utcNow, today);

    /// <inheritdoc/>
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetAllWorksGroupsAsync(cancellationToken))
            .Where(g => g.GroupType == GroupType.Return && string.IsNullOrEmpty(g.ManagerFullname))
            .ToList();

        string message = await _returnNotificationService.GetReturnCompleteMessageAsync(cancellationToken);
        foreach (Group group in groups)
            await SendMessageAsync(message, group, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendForGroupAsync(Group group, CancellationToken cancellationToken)
    {
        // For forced sends from the bot, mirror original behaviour:
        // 1. manager-specific return message (if manager is set)
        // 2. daily complete summary
        if (!string.IsNullOrEmpty(group.ManagerFullname))
        {
            string managerLastName = new ManagerName(group.ManagerFullname).LastName;
            string returnMessage = await _returnNotificationService.GetReturnMessageAsync(managerLastName, cancellationToken);
            await SendMessageAsync(returnMessage, group, cancellationToken);
        }

        string dailyMessage = await _returnNotificationService.GetReturnCompleteMessageAsync(cancellationToken);
        await SendMessageAsync(dailyMessage, group, cancellationToken);
    }
}
