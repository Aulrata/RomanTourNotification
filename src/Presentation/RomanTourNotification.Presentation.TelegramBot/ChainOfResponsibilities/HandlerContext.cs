using RomanTourNotification.Application.Contracts.Users;
using RomanTourNotification.Application.Models.Users;
using Telegram.Bot;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public record HandlerContext(
    User User,
    Iterator Iterator,
    ITelegramBotClient BotClient,
    CancellationToken CancellationToken,
    IUserService UserService,
    int MessageId = 0);