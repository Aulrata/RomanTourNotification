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
            string text;
            long userId;
            Message? message = update.Message;
            CallbackQuery? callbackQuery = update.CallbackQuery;

            if (message is not null)
            {
                text = message.Text ?? string.Empty;
                userId = message.Chat.Id;
            }
            else if (callbackQuery is not null)
            {
                text = callbackQuery.Data ?? string.Empty;
                userId = callbackQuery.From.Id;
            }
            else
            {
                return;
            }

            if (string.IsNullOrEmpty(text))
                return;

            if (!_users.TryGetValue(userId, out User? value))
            {
                User? user = await _userService.GetByIdAsync(userId, cancellationToken);

                if (user is null)
                    return;

                value = user;

                _users.Add(userId, value);
            }

            var handler = new HandlerContext(value, text, _botClient);
            var startHandler = new StartHandler();
            var userHandler = new UserHandler();
            await startHandler.SetNext(userHandler);
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