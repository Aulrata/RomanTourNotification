using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Groups;
using RomanTourNotification.Application.Contracts.NotificationService;
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

public class NotificationBotReceiving : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationBotReceiving> _logger;

    // TODO Учесть удаление пользователей
    private readonly Dictionary<long, User> _users = [];

    public NotificationBotReceiving(
        ITelegramBotClient botClient,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationBotReceiving> logger)
    {
        _botClient = botClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        _botClient.StartReceiving(
            updateHandler: async (bot, update, token) => await HandleUpdateAsync(update, token),
            errorHandler: (bot, exception, token) =>
            {
                _logger.LogError(exception, "Telegram polling error");
                return Task.CompletedTask;
            },
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        Telegram.Bot.Types.User bot = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("{Username} started", bot.Username);

        await Task.Delay(Timeout.Infinite, stoppingToken)
            .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    private async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            IGroupService groupService = scope.ServiceProvider.GetRequiredService<IGroupService>();
            ILoadEmployees loadEmployees = scope.ServiceProvider.GetRequiredService<ILoadEmployees>();
            INotificationService notificationService =
                scope.ServiceProvider.GetRequiredService<INotificationService>();

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
                        User? user = await userService.GetByChatIdAsync(id, cancellationToken);

                        if (user is null)
                            return;
                    }

                    firstName = chatMember.Chat.FirstName;
                    lastName = chatMember.Chat.LastName;
                    switch (chatMember.NewChatMember.Status)
                    {
                        case ChatMemberStatus.Left:
                            await DeleteGroup(chatMember, groupService, cancellationToken);
                            break;

                        case ChatMemberStatus.Member:
                            await AddGroup(chatMember, userService, groupService, cancellationToken);
                            break;
                    }

                    return;
            }

            if (string.IsNullOrEmpty(text) || userId <= 0)
                return;

            if (!_users.TryGetValue(userId, out User? value))
            {
                User? user = await userService.GetByChatIdAsync(userId, cancellationToken);

                if (user is null)
                {
                    var newUser = new User(
                        0,
                        firstName ?? string.Empty,
                        lastName ?? string.Empty,
                        UserRole.Unspecified,
                        userId,
                        DateTime.Now);
                    await userService.CreateAsync(newUser, cancellationToken);

                    user = await userService.GetByChatIdAsync(userId, cancellationToken);

                    if (user is null)
                        return;
                }

                value = user;

                if (value.Role is UserRole.Unspecified)
                    return;

                _users.Add(userId, value);
            }

            var iterator = new Iterator(text);
            var handlerServices = new HandlerServices(userService, groupService, loadEmployees, notificationService);
            var context =
                new HandlerContext(value, iterator, _botClient, cancellationToken, handlerServices, messageId);
            var startHandler = new StartHandler();
            var userHandler = new UserHandler();
            var groupHandler = new GroupHandler();
            startHandler.SetNext(userHandler).SetNext(groupHandler);
            await startHandler.Handle(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bot update error");
        }
    }

    private async Task AddGroup(
        ChatMemberUpdated chatMember,
        IUserService userService,
        IGroupService groupService,
        CancellationToken cancellationToken)
    {
        long groupId = chatMember.Chat.Id;
        string groupTitle = chatMember.Chat.Title ?? "Нет данных";
        long idFrom = chatMember.From.Id;
        string userNameFrom = chatMember.From.Username ?? "Нет данных";

        User? user = await userService.GetByChatIdAsync(idFrom, cancellationToken);

        if (user is null)
        {
            _logger.LogError(
                "Failed to add group. The user who added to the group was not found in the database");
            return;
        }

        var group = new Group(0, groupTitle, groupId, user.Id, string.Empty, GroupType.Unspecified, DateTime.Now);

        Group? addedGroup = await groupService.AddAsync(group, cancellationToken);

        if (addedGroup is null)
        {
            Group? oldGroup = await groupService.GetByChatIdAsync(groupId, cancellationToken);

            if (oldGroup is null)
                return;

            var updatedGroup = new Group(
                oldGroup.Id,
                chatMember.Chat.Title ?? oldGroup.Title,
                groupId,
                user.Id,
                oldGroup.Title,
                oldGroup.GroupType,
                oldGroup.CreatedAt);

            await groupService.UpdateAsync(updatedGroup, cancellationToken);

            _logger.LogInformation(
                "Пользователь {UserName} обновил бота в группе {Title}",
                userNameFrom,
                groupTitle);

            await _botClient.SendMessage(
                group.ChatId,
                $"Пользователь {userNameFrom} обновил бота в группе {groupTitle}",
                cancellationToken: cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Пользователь {UserName} добавил бота в группу {Title}",
                userNameFrom,
                groupTitle);

            await _botClient.SendMessage(
                group.ChatId,
                $"Пользователь {userNameFrom} добавил бота в группу {groupTitle}",
                cancellationToken: cancellationToken);
        }
    }

    private async Task DeleteGroup(
        ChatMemberUpdated chatMember,
        IGroupService groupService,
        CancellationToken cancellationToken)
    {
        long deletedGroup = await groupService.DeleteAsync(chatMember.Chat.Id, cancellationToken);

        _logger.LogWarning(
            "Пользователь {Username} удалил бота из группы {Title}. Id группы: {GroupId}",
            chatMember.From.Username,
            chatMember.Chat.Title,
            deletedGroup);
    }
}
