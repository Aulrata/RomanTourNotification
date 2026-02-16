using RomanTourNotification.Application.Models.Groups;

namespace RomanTourNotification.Application.Contracts.NotificationService;

public interface INotificationService
{
    public Task SendNotificationAsync(CancellationToken cancellationToken);

    public Task SendForcedNotificationAsync(Group? group, GroupType type, CancellationToken cancellationToken);

    public Task SendSpecialNotificationAsync(CancellationToken cancellationToken);

    public Task SendReceiptNotificationAsync(CancellationToken cancellationToken, Group? forcedGroup = null);
}