using RomanTourNotification.Application.Contracts.Bots;
using Telegram.Bot;

namespace RomanTourNotification.Application.Bots;

public class NotificationBotService : INotificationBotService
{
    public NotificationBotService()
    {
    }

    public async Task StartAsync(ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}