using RomanTourNotification.Application.Contracts.Bots;
using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Mappers;
using RomanTourNotification.Application.Models.Users;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using User = Telegram.Bot.Types.User;

namespace RomanTourNotification.Application.Bots;

public class NotificationBotService : INotificationBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ConcurrentDictionary<long, UserState> _userStates;
    private readonly IUserService _userService;

    public NotificationBotService(ITelegramBotClient botClient, IUserService userService)
    {
        _botClient = botClient;
        _userService = userService;
        _userStates = new ConcurrentDictionary<long, UserState>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        _botClient.StartReceiving(
            updateHandler: async (bot, update, token) => await HandleUpdateAsync(update, token),
            errorHandler: async (bot, exception, token) => await HandleErrorAsync(exception, token),
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken);

        User botInfo = await _botClient.GetMe(cancellationToken: cancellationToken);
        Console.WriteLine($"Hello, {botInfo.Username}!");
    }

    private async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        Message? message = update.Message;

        if (message is null || string.IsNullOrEmpty(message.Text))
            return;

        long userId = message.Chat.Id;

        if (message.Text == "Создать")
        {
            if (!_userStates.ContainsKey(userId))
            {
                _userStates[userId] = new UserState();
                await _botClient.SendMessage(userId, "Введите имя", cancellationToken: cancellationToken);
                return;
            }
        }

        UserState state = _userStates[userId];

        if (string.IsNullOrEmpty(state.FirstName))
        {
            state.FirstName = message.Text;
            await _botClient.SendMessage(userId, "Введите фамилию", cancellationToken: cancellationToken);
        }
        else if (string.IsNullOrEmpty(state.LastName))
        {
            state.LastName = message.Text;
            await _botClient.SendMessage(userId, "Введите роль: Разработчик, админ, менеджер", cancellationToken: cancellationToken);
        }
        else if (state.Role == UserRole.Unspecified)
        {
            UserRole? role = UserRoleMapper.MapRole(message.Text);

            if (role is null)
            {
                await _botClient.SendMessage(userId, "Вы ввели неверную роль, попробуйте еще раз.", cancellationToken: cancellationToken);
                return;
            }

            state.Role = role.Value;
            await _botClient.SendMessage(userId, "Введите айди чата, если хотите использовать текущий, то поставьте +", cancellationToken: cancellationToken);
        }
        else if (state.ChatId == 0)
        {
            string text = message.Text;

            if (text == "+")
            {
                state.ChatId = message.Chat.Id;
            }
            else if (long.TryParse(text, out long chatId))
            {
                state.ChatId = chatId;
            }
            else
            {
                await _botClient.SendMessage(userId, "Вы ввели неверную айди чата, попробуйте еще раз.", cancellationToken: cancellationToken);
                return;
            }
        }
        else if (!string.IsNullOrEmpty(state.FirstName) && !string.IsNullOrEmpty(state.LastName) && state.Role != UserRole.Unspecified)
        {
            await _botClient.SendMessage(userId, $"Вы создали пользователя. Имя: {state.FirstName}, фамилия: {state.LastName} , роль: {state.Role.ToString()}, айди чата: {state.ChatId}", cancellationToken: cancellationToken);

            var user = new RomanTourNotification.Application.Models.Users.User(null, state.FirstName, state.LastName, state.Role, state.ChatId, DateTime.UtcNow);
            await _userService.CreateAsync(user, cancellationToken);
            _userStates[message.Chat.Id] = new UserState();
        }
    }

    private Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}