using RomanTourNotification.Application.Contracts.Bots;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;
using RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using User = RomanTourNotification.Application.Models.Users.User;

namespace RomanTourNotification.Presentation.TelegramBot.Receiving;

public class NotificationBotReceiving
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly INotificationBotService _notificationBotService;
    private readonly Dictionary<long, User> _users;

    public NotificationBotReceiving(ITelegramBotClient botClient, IUserService userService, INotificationBotService notificationBotService)
    {
        _botClient = botClient;
        _userService = userService;
        _notificationBotService = notificationBotService;
        _users = [];
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        _botClient.StartReceiving(
            updateHandler: async (bot, update, token) => await HandleUpdateAsync(update, token),
            errorHandler: async (bot, exception, token) => await HandleErrorAsync(exception, token),
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken);

        Console.WriteLine($"Notification bot started");

        await _notificationBotService.StartAsync(_botClient, cancellationToken);
    }

    private async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            Message? message = update.Message;

            if (message is null || string.IsNullOrEmpty(message.Text))
                return;

            long userId = message.Chat.Id;

            if (!_users.ContainsKey(userId))
            {
                User? user = await _userService.GetByIdAsync(userId, cancellationToken);

                if (user is null)
                    return;

                _users.Add(userId, user);
            }

            var handler = new HandlerContext(_users[userId], message.Text, _botClient);
            var startHandler = new StartHandler();
            await startHandler.Handle(handler);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message} ");
        }
    }

    private Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}