namespace RomanTourNotification.Application.Contracts.Bots;

public interface INotificationBotService
{
    public Task StartAsync(CancellationToken cancellationToken);
}