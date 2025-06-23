namespace RomanTourNotification.Application.Contracts.NotificationService;

public interface INotificationService
{
    public Task SendNotificationAsync(CancellationToken cancellationToken);
}