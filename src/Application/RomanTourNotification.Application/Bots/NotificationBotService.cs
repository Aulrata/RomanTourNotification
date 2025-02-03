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
        Console.WriteLine("Start message");

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (!(DateTime.UtcNow.Hour == 6 && DateTime.UtcNow.Minute == 0))
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                continue;
            }

            await botClient.SendMessage(-1001844409797, "Запланированное сообщение на 9:00. Это означает что сервер работает стабильно.", cancellationToken: cancellationToken);
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }

        await Task.CompletedTask;
    }
}
