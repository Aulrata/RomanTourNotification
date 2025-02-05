using Microsoft.Extensions.Hosting;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Models.Groups;
using Telegram.Bot;

namespace RomanTourNotification.Application.Bots;

public class NotificationsBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IGroupService _groupService;

    public NotificationsBackgroundService(ITelegramBotClient botClient, IGroupService groupService)
    {
        _botClient = botClient;
        _groupService = groupService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Start message");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!(DateTime.UtcNow.Hour == 14 && DateTime.UtcNow.Minute == 30))
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            IEnumerable<Group>? groups = await _groupService.GetAllAsync(stoppingToken);

            if (groups is null)
            {
                Console.WriteLine("No groups found");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            await SendArrivalNotification(groups.Where(x => x.GroupType == GroupType.Arrival), stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task SendArrivalNotification(IEnumerable<Group>? groups, CancellationToken cancellationToken)
    {
        if (groups is null)
        {
            Console.WriteLine("No groups found for arrival");
            return;
        }

        foreach (Group group in groups)
        {
            await _botClient.SendMessage(
                group.GroupId,
                $"Текст для группы {group.Title}",
                cancellationToken: cancellationToken);
        }
    }
}
