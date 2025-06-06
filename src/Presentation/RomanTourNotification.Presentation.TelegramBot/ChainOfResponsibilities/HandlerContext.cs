using RomanTourNotification.Application.Models.Users;
using Telegram.Bot;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public record HandlerContext(
    User User,
    Iterator Iterator,
    ITelegramBotClient BotClient,
    CancellationToken CancellationToken,
    HandlerServices HandlerServices,
    int MessageId = 0);