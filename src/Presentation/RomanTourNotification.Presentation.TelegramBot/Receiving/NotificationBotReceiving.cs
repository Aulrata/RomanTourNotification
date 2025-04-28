using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Models.Groups;
using RomanTourNotification.Application.Models.Users;
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
    private readonly IGroupService _groupService;
    private readonly ILogger<NotificationBotReceiving> _logger;

    // TODO Учесть усдалеие пользователей
    private readonly Dictionary<long, User> _users;

    public NotificationBotReceiving(
        ITelegramBotClient botClient,
        IUserService userService,
        IGroupService groupService,
        ILogger<NotificationBotReceiving> logger)
    {
        _botClient = botClient;
        _userService = userService;
        _groupService = groupService;
        _logger = logger;
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

        Telegram.Bot.Types.User bot = await _botClient.GetMe(cancellationToken);
        _logger.LogInformation($"{bot.Username} started");
    }

    private async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            string text = string.Empty;
            long userId = 0;
            int messageId = 0;
            string? firstName = string.Empty;
            string? lastName = string.Empty;

            switch (update.Type)
            {
                case UpdateType.CallbackQuery:

                    CallbackQuery? callbackQuery = update.CallbackQuery;

                    if (callbackQuery is null)
                        return;

                    text = callbackQuery.Data ?? string.Empty;
                    userId = callbackQuery.From.Id;
                    firstName = callbackQuery.From.FirstName;
                    lastName = callbackQuery.From.LastName;

                    if (callbackQuery.Message is not null)
                        messageId = callbackQuery.Message.MessageId;

                    break;

                case UpdateType.Message:

                    Message? message = update.Message;

                    if (message is null)
                        return;

                    text = message.Text ?? string.Empty;
                    userId = message.Chat.Id;
                    firstName = message.Chat.FirstName;
                    lastName = message.Chat.LastName;

                    break;

                case UpdateType.MyChatMember:
                    ChatMemberUpdated? chatMember = update.MyChatMember;
                    if (chatMember is null)
                        return;

                    long id = chatMember.From.Id;

                    if (!_users.ContainsKey(id))
                    {
                        User? user = await _userService.GetByChatIdAsync(id, cancellationToken);

                        if (user is null)
                            return;
                    }

                    firstName = chatMember.Chat.FirstName;
                    lastName = chatMember.Chat.LastName;
                    switch (chatMember.NewChatMember.Status)
                    {
                        case ChatMemberStatus.Left:
                            await DeleteGroup(chatMember, cancellationToken);
                            break;

                        case ChatMemberStatus.Member:
                            await AddGroup(chatMember, cancellationToken);
                            break;
                    }

                    break;
            }

            if (string.IsNullOrEmpty(text) || userId <= 0)
                return;

            if (!_users.TryGetValue(userId, out User? value))
            {
                User? user = await _userService.GetByChatIdAsync(userId, cancellationToken);

                if (user is null)
                {
                    var newUser = new User(0, firstName ?? string.Empty, lastName ?? string.Empty, UserRole.Unspecified, userId, DateTime.Now);
                    long newUserId = await _userService.CreateAsync(newUser, cancellationToken);

                    user = await _userService.GetByChatIdAsync(newUserId, cancellationToken);

                    if (user is null)
                        return;
                }

                value = user;

                if (value.Role is UserRole.Unspecified)
                    return;

                _users.Add(userId, value);
            }

            var iterator = new Iterator(text);
            var context = new HandlerContext(value, iterator, _botClient, cancellationToken, _userService, messageId);
            var startHandler = new StartHandler();
            var userHandler = new UserHandler();
            await startHandler.SetNext(userHandler);
            await startHandler.Handle(context);
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Bot Error: {ex.Message} ");
        }
    }

    private Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task AddGroup(ChatMemberUpdated chatMember, CancellationToken cancellationToken)
    {
        long groupId = chatMember.Chat.Id;
        string groupTitle = chatMember.Chat.Title ?? "Нет данных";
        long idFrom = chatMember.From.Id;
        string userNameFrom = chatMember.From.Username ?? "Нет данных";

        User? user = await _userService.GetByChatIdAsync(idFrom, cancellationToken);

        if (user is null)
        {
            _logger.LogError("Не удалось добавить группу. Пользователь, который добавлял в группу не найден в базе данных");
            return;
        }

        var group = new Group(0, groupTitle, groupId, user.Id, GroupType.Unspecified, DateTime.Now);

        Group? addedGroup = await _groupService.AddAsync(group, cancellationToken);

        if (addedGroup is null)
        {
            _logger.LogInformation("Group already exists");
            return;
        }

        _logger.LogInformation($"Пользователь {userNameFrom} добавил бота в группу {groupTitle}");

        await _botClient.SendMessage(
            group.ChatId,
            $"Пользователь {userNameFrom} добавил бота в группу {groupTitle}",
            cancellationToken: cancellationToken);
    }

    private async Task DeleteGroup(ChatMemberUpdated chatMember, CancellationToken cancellationToken)
    {
        long deletedGroup = await _groupService.DeleteAsync(chatMember.Chat.Id, cancellationToken);

        if (deletedGroup == 0)
        {
            Console.WriteLine("Group already deleted");
            return;
        }

        Console.WriteLine($"Пользователь {chatMember.From.Username} удалил бота из группы {chatMember.Chat.Title}");
    }
}