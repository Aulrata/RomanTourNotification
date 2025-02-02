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
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (DateTime.UtcNow.Hour != 0 || DateTime.UtcNow.Minute != 57) continue;

            await botClient.SendMessage(-1001844409797, "Запланированное сообщение", cancellationToken: cancellationToken);
            await Task.Delay(61 * 1000, cancellationToken);
        }

        await Task.CompletedTask;
    }
}