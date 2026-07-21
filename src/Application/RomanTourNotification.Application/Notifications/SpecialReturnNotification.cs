using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Notifications;
using RomanTourNotification.Application.Contracts.ReturnNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Notifications.Base;
using RomanTourNotification.Domain.ValueObjects;
using Telegram.Bot;

namespace RomanTourNotification.Application.Notifications;

/// <summary>
/// Sends manager-specific return messages every Wednesday.
/// Only implements <see cref="IScheduledNotification"/> — there is no forced-send variant
/// for this type; the bot's "Return" forced send is handled by <see cref="DailyReturnNotification"/>.
/// </summary>
public class SpecialReturnNotification : TelegramNotificationBase, IScheduledNotification
{
    private readonly IGroupService _groupService;
    private readonly IReturnNotificationService _returnNotificationService;
    private readonly TimeSettings _timeSettings;

    /// <summary>Initializes a new instance of the <see cref="SpecialReturnNotification"/> class.</summary>
    public SpecialReturnNotification(
        ILogger<SpecialReturnNotification> logger,
        ITelegramBotClient botClient,
        IGroupService groupService,
        IReturnNotificationService returnNotificationService,
        TimeSettings timeSettings)
        : base(logger, botClient)
    {
        _groupService = groupService;
        _returnNotificationService = returnNotificationService;
        _timeSettings = timeSettings;
    }

    /// <inheritdoc/>
    public bool ShouldSend(DateTime utcNow, DayOfWeek today)
        => today is DayOfWeek.Wednesday
           && utcNow.Hour == _timeSettings.ReturnHoursUtc
           && utcNow.Minute == _timeSettings.ReturnMinute;

    /// <inheritdoc/>
    public async Task SendAsync(CancellationToken cancellationToken)
    {
        var groups = (await _groupService.GetAllWorksGroupsAsync(cancellationToken))
            .Where(g => g.GroupType == GroupType.Return)
            .ToList();

        foreach (Group group in groups)
        {
            string managerLastName = new ManagerName(group.ManagerFullname).LastName;
            string message = await _returnNotificationService.GetReturnMessageAsync(managerLastName, cancellationToken);
            await SendMessageAsync(message, group, cancellationToken);
        }
    }
}
