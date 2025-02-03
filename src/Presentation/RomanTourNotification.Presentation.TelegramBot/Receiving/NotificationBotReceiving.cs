using RomanTourNotification.Application.Contracts.Bots;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;
using RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = RomanTourNotification.Application.Models.Users.User;

namespace RomanTourNotification.Presentation.TelegramBot.Receiving;

public class NotificationBotReceiving
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly INotificationBotService _notificationBotService;
    private readonly Dictionary<long, User> _users;

    public NotificationBotReceiving(
        ITelegramBotClient botClient,
        IUserService userService,
        INotificationBotService notificationBotService)
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
            string text = string.Empty;
            long userId = 0;
            int messageId = 0;

            switch (update.Type)
            {
                case UpdateType.CallbackQuery:

                    CallbackQuery? callbackQuery = update.CallbackQuery;

                    if (callbackQuery is null)
                        return;

                    text = callbackQuery.Data ?? string.Empty;
                    userId = callbackQuery.From.Id;

                    if (callbackQuery.Message is not null)
                        messageId = callbackQuery.Message.MessageId;

                    break;
                case UpdateType.Message:

                    Message? message = update.Message;

                    if (message is null)
                        return;

                    text = message.Text ?? string.Empty;
                    userId = message.Chat.Id;

                    break;
                case UpdateType.MyChatMember:
                    ChatMemberUpdated? chatMember = update.MyChatMember;
                    if (chatMember is null)
                        return;

                    long groupId = chatMember.Chat.Id;
                    string groupTitle = chatMember.Chat.Title ?? "Нет данных";
                    long idFrom = chatMember.From.Id;
                    string firstNameFrom = chatMember.From.FirstName;
                    string lastNameFrom = chatMember.From.LastName ?? "Нет данных";
                    string userNameFrom = chatMember.From.Username ?? "Нет данных";
                    DateTime date = chatMember.Date;

                    Console.WriteLine($"Пользователь {userNameFrom}. Имя: {firstNameFrom}, фамилия: {lastNameFrom}, " +
                                      $"Id: {idFrom} добавил бота в группу: {groupTitle}, Id: {groupId}. Дата: {date}");
                    break;
            }

            if (string.IsNullOrEmpty(text) || userId == 0)
                return;

            if (!_users.TryGetValue(userId, out User? value))
            {
                User? user = await _userService.GetByIdAsync(userId, cancellationToken);

                if (user is null)
                    return;

                value = user;

                _users.Add(userId, value);
            }

            var handler = new HandlerContext(value, text, _botClient, cancellationToken, messageId);
            var startHandler = new StartHandler();
            var userHandler = new UserHandler();
            await startHandler.SetNext(userHandler);
            await startHandler.Handle(handler);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bot Error: {ex.Message} ");
        }
    }

    private Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}