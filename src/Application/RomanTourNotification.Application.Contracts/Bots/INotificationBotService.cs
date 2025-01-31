using Telegram.Bot;

namespace RomanTourNotification.Application.Contracts.Bots;

public interface INotificationBotService
{
    public Task StartAsync(ITelegramBotClient botClient, CancellationToken cancellationToken);
}